// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Runtime.InteropServices;
using System.Threading;

using CliWrap;

using GitGud.CliArguments;
using GitGud.Config;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Processes;
using GitGud.Setup.Errors;
using GitGud.Utility;


namespace GitGud.Setup;

public sealed class GitSetup : IGitSetup
{
    private readonly IConfiguration _configuration;

    public GitSetup() : this(new Configuration())
    {
    }

    public GitSetup(IConfiguration configuration)
        => _configuration = configuration;

    private sealed class CachedStatus
    {
        public CachedStatus(IGitGlobStatusResult status, DateTime expiresUtc)
            => (Status, ExpiresUtc) = (status, expiresUtc);

        public IGitGlobStatusResult Status { get; }
        public DateTime ExpiresUtc { get; }
    }

    private CachedStatus? _cachedStatus;

    public async Task<Result<IGitGlobStatusResult, IError>> GetStatus(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var cached = _cachedStatus;
        if (!forceRefresh && cached is not null && cached.ExpiresUtc > DateTime.UtcNow)
            return Result<IGitGlobStatusResult, IError>.Some(cached.Status);

        try
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (!isWindows && !isMac && !isLinux)
                return Result<IGitGlobStatusResult, IError>.None(new SetupExecutionError(Const.Setup.Operations.GIT_STATUS));

            // PATH and well-known location probes are cheap synchronous file checks, so they intentionally run inline.
            var executableName = isWindows ? Const.Setup.Status.WINDOWS_EXECUTABLE : Const.Setup.Status.UNIX_EXECUTABLE;
            var installPath = FilePathHelpers.FindOnPath(executableName);
            var isInPath = installPath is not null;
            installPath ??= FilePathHelpers.FindInstalledExecutable(isWindows, isMac);

            string? version = null;
            var isInstalled = installPath is not null;
            if (installPath is not null)
            {
                var command = Cli.Wrap(installPath)
                    .WithArguments(args => args.Add(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.VERSION), true));
                var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                if (result.ExitCode == 0)
                    version = ParseGitVersion(result.StandardOutput);

                isInstalled = result.ExitCode == 0;
            }

            var status = CreateStatus(isWindows, isMac, isInstalled, isInPath, installPath, version);
            _cachedStatus = new CachedStatus(status, DateTime.UtcNow.AddSeconds(Const.Setup.Status.CACHE_SECONDS));
            return Result<IGitGlobStatusResult, IError>.Some(status);
        }
        catch (OperationCanceledException)
        {
            return Result<IGitGlobStatusResult, IError>.None(new SetupCancelledError(Const.Setup.Operations.GIT_STATUS));
        }
        catch (Exception)
        {
            return Result<IGitGlobStatusResult, IError>.None(new SetupExecutionError(Const.Setup.Operations.GIT_STATUS));
        }
    }

    private static IGitGlobStatusResult CreateStatus(bool isWindows, bool isMac, bool isInstalled, bool isInPath, string? installPath, string? version)
    {
        var parsed = ParseVersionNumber(version);
        if (isWindows)
            return new WindowsGitGlobalStatus { IsInstalled = isInstalled, IsInPATH = isInPath, InstallPath = installPath, Version = version, ParsedVersion = parsed };

        if (isMac)
            return new MacGitGlobalStatus { IsInstalled = isInstalled, IsInPATH = isInPath, InstallPath = installPath, Version = version, ParsedVersion = parsed };

        return new LinuxGitGlobalStatus { IsInstalled = isInstalled, IsInPATH = isInPath, InstallPath = installPath, Version = version, ParsedVersion = parsed };
    }

    private static Version? ParseVersionNumber(string? version)
    {
        if (string.IsNullOrEmpty(version))
            return null;

        var text = version!;
        var end = 0;
        var dots = 0;
        while (end < text.Length && (char.IsDigit(text[end]) || (text[end] == '.' && dots < 3)))
        {
            if (text[end] == '.')
                dots++;
            end++;
        }

        var numeric = text.Substring(0, end).TrimEnd('.');
        return Version.TryParse(numeric, out var parsed) ? parsed : null;
    }

    private static string? ParseGitVersion(string? standardOutput)
    {
        if (string.IsNullOrWhiteSpace(standardOutput))
            return null;

        var version = standardOutput!.Trim();
        var prefix = Const.Setup.Status.GIT_VERSION_PREFIX;
        return version.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? version.Substring(prefix.Length).Trim()
            : version;
    }

    public Task<Result<Nothing, IError>> Setup(IGitSetupContext setupContext)
        => ExecuteAsync(setupContext, SetupOperation.Install);

    public Task<Result<Nothing, IError>> Uninstall(IGitUninstallContext uninstallContext)
        => ExecuteAsync(uninstallContext, SetupOperation.Uninstall);

    public Task<Result<Nothing, IError>> Update(IGitUpdateContext updateContext)
        => ExecuteAsync(updateContext, SetupOperation.Update);

    public async Task<Result<SshKeyResult, IError>> GenerateSshKeyPair(IGitSshKeyContext context)
    {
        if (context is null)
            return Result<SshKeyResult, IError>.None(new SetupContextError());

        var keyName = ConfigKey.Create(Const.Config.Keys.CORE_SSH_COMMAND);
        if (keyName.HasNone)
            return Result<SshKeyResult, IError>.None(keyName.Handle());

        var checkResult = await EnsureSshCommandCanBeSetAsync(context, keyName.Unwrap()).ConfigureAwait(false);
        if (checkResult.HasNone)
            return Result<SshKeyResult, IError>.None(checkResult.Handle());

        var keyResult = await SshKeyManager.GenerateKeyPairAsync(context).ConfigureAwait(false);
        if (keyResult.HasNone)
            return Result<SshKeyResult, IError>.None(keyResult.Handle());

        var generatedKey = keyResult.Unwrap();

        var agentResult = await SshKeyManager.StartAgentAndAddKeyAsync(
            generatedKey.PrivateKeyPath.ToString(),
            !string.IsNullOrEmpty(context.Passphrase),
            context.CancellationToken).ConfigureAwait(false);
        if (agentResult.HasNone)
        {
            DeleteKeyFiles(generatedKey);
            return Result<SshKeyResult, IError>.None(agentResult.Handle());
        }

        var configResult = await SetSshCommandAsync(context, keyName.Unwrap(), generatedKey.PrivateKeyPath.ToString()).ConfigureAwait(false);
        if (configResult.HasNone)
        {
            DeleteKeyFiles(generatedKey);
            return Result<SshKeyResult, IError>.None(configResult.Handle());
        }

        return Result<SshKeyResult, IError>.Some(new SshKeyResult(
            generatedKey.PrivateKeyPath,
            generatedKey.PublicKeyPath,
            generatedKey.PublicKey,
            agentResult.Unwrap()));
    }

    private async Task<Result<Nothing, IError>> EnsureSshCommandCanBeSetAsync(IGitSshKeyContext context, ConfigKey key)
    {
        var existing = await _configuration.GetAsync(
            key,
            new ConfigGetQueryParameters(FileOptions.Global),
            context.CancellationToken).ConfigureAwait(false);
        if (existing.HasNone)
            return Result<Nothing, IError>.None(existing.Handle());

        var entries = existing.Unwrap();
        if (entries.Count == 0)
            return Result<Nothing, IError>.Some(new Nothing());

        var overwrite = context.ShouldOverwriteSshCommand is not null
            && await context.ShouldOverwriteSshCommand(entries[entries.Count - 1].Value ?? string.Empty).ConfigureAwait(false);

        return overwrite
            ? Result<Nothing, IError>.Some(new Nothing())
            : Result<Nothing, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_CONFIGURATION,
                Const.Setup.Errors.SSH_COMMAND_EXISTS));
    }

    private Task<Result<Nothing, IError>> SetSshCommandAsync(IGitSshKeyContext context, ConfigKey key, string privateKeyPath)
    {
        var escapedKeyPath = privateKeyPath
            .Replace('\\', '/')
            .Replace(Const.Setup.Ssh.SHELL_SINGLE_QUOTE, Const.Setup.Ssh.SHELL_ESCAPED_SINGLE_QUOTE);
        var command = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            Const.Setup.Ssh.SSH_COMMAND_FORMAT,
            escapedKeyPath);

        return _configuration.SetAsync(
            key,
            ConfigValue.FromString(command),
            new ConfigSetQueryParameters(FileOptions.Global),
            context.CancellationToken);
    }

    private static void DeleteKeyFiles(SshKeyResult key)
    {
        TryDelete(key.PrivateKeyPath.ToString());
        TryDelete(key.PublicKeyPath.ToString());
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
        catch (Exception)
        {
        }
    }

    private static async Task<Result<Nothing, IError>> ExecuteAsync(IOperationContext context, SetupOperation operation)
    {
        if (context is null)
            return Result<Nothing, IError>.None(new SetupContextError());

        var res = await FindPackageManager(context).ConfigureAwait(false);

        return await res.Transform(
               async pMan => await PerformInstall(context, operation, pMan).ConfigureAwait(false),
              error => Task.FromResult(Result<Nothing, IError>.None(error))
        );
    }

    private static async Task<Result<PackageManager, IError>> FindPackageManager(IOperationContext context)
    {
        try
        {
            var packageManager = await DetectPackageManagerAsync(context.CancellationToken).ConfigureAwait(false);
            if (packageManager is not PackageManager pman)
                return Result<PackageManager, IError>.None(new SetupExecutionError(Const.Setup.Operations.GIT_SETUP));
            return Result<PackageManager, IError>.Some(pman);
        }
        catch (Exception)
        {
            return Result<PackageManager, IError>.None(new SetupExecutionError(Const.Setup.Operations.GIT_SETUP));
        }
    }

    private static async Task<Result<Nothing, IError>> PerformInstall(IOperationContext context, SetupOperation operation, PackageManager pMan)
    {
        if (!pMan.TryGetExecutable(out var executable))
            return Result<Nothing, IError>.None(new SetupPackageManagerNotSupportedError());

        if (pMan.RequiresElevation() && !ElevationHelper.IsElevated())
        {
            var message = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Const.Setup.Elevation.REQUIRED_WINDOWS
                : Const.Setup.Elevation.REQUIRED_UNIX;
            return Result<Nothing, IError>.None(new SetupElevationRequiredError(operation.GetOperationName(), message));
        }

        try
        {
            var packageCommand = CreatePackageCommand(executable, pMan, operation);
            if (!packageCommand.ArgumentsSupported)
                return Result<Nothing, IError>.None(new SetupPackageManagerNotSupportedError());

            var result = await ProcessRunner.ExecuteAsync(packageCommand.Command, context.CancellationToken).ConfigureAwait(false);
            if (result.ExitCode == 0)
                return Result<Nothing, IError>.Some(new Nothing());

            var operationName = operation.GetOperationName();
            return Result<Nothing, IError>.None(new SetupProcessError(
                operationName,
                string.Format(System.Globalization.CultureInfo.InvariantCulture, Const.Setup.Errors.PACKAGE_COMMAND_FAILED_FORMAT, executable, result.ExitCode),
                result.ExitCode,
                result.StandardError));
        }
        catch (Exception)
        {
            return Result<Nothing, IError>.None(new SetupExecutionError(operation.GetOperationName()));
        }
    }

    private static async Task<PackageManager?> DetectPackageManagerAsync(CancellationToken cancellationToken)
    {
        PackageManager[] candidates;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            candidates = PackageManagers.WindowsManagers;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            candidates = PackageManagers.MacManagers;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            candidates = PackageManagers.LinuxManagers;
        else
            return null;

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await IsAvailableAsync(candidate, cancellationToken).ConfigureAwait(false))
                return candidate;
        }

        return null;
    }

    private static async Task<bool> IsAvailableAsync(PackageManager packageManager, CancellationToken cancellationToken)
    {
        try
        {
            if (!packageManager.TryGetExecutable(out var executable))
                return false;

            var command = Cli.Wrap(executable)
                .WithArguments(args => args.Add(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.VERSION), true));
            var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            return result.ExitCode == 0;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private static (Command Command, bool ArgumentsSupported) CreatePackageCommand(string executable, PackageManager packageManager, SetupOperation operation)
    {
        var argumentsSupported = false;
        var command = Cli.Wrap(executable)
            .WithArguments(args => argumentsSupported = AddPackageArguments(new(args), packageManager, operation));
        return (command, argumentsSupported);
    }

    private static bool AddPackageArguments(CliWrapArgumentBuilder args, PackageManager packageManager, SetupOperation operation)
    {
        return packageManager switch
        {
            PackageManager.Winget => AddWingetArguments(args, operation),
            PackageManager.Chocolatey => AddChocolateyArguments(args, operation),
            PackageManager.Scoop => AddScoopArguments(args, operation),
            PackageManager.Brew => AddBrewArguments(args, operation),
            PackageManager.AptGet or PackageManager.Apt => AddAptArguments(args, operation),
            PackageManager.Dnf or PackageManager.Yum => AddDnfArguments(args, operation),
            PackageManager.Pacman => AddPacmanArguments(args, operation),
            PackageManager.Zypper => AddZypperArguments(args, operation),
            PackageManager.Apk => AddApkArguments(args, operation),
            _ => false,
        };

    }

    private static bool AddWingetArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.UNINSTALL, Const.Setup.PackageManagers.Actions.UPGRADE), true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.ID), true);
        args.AddArgument(Const.Setup.PackageManagers.ID_WINGET, true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.EXACT), true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.SILENT), true);
        if (operation == SetupOperation.Uninstall)
            return true;

        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.ACCEPT_PACKAGE_AGREEMENTS), true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.ACCEPT_SOURCE_AGREEMENTS), true);
        return true;
    }

    private static bool AddChocolateyArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.UNINSTALL, Const.Setup.PackageManagers.Actions.UPGRADE), true);
        args.AddArgument(Const.Setup.PackageManagers.ID_CHOCOLATEY, true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.YES), true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.NO_PROGRESS), true);
        return true;
    }

    private static bool AddScoopArguments(CliWrapArgumentBuilder     args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.UNINSTALL, Const.Setup.PackageManagers.Actions.UPDATE), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static bool AddBrewArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.UNINSTALL, Const.Setup.PackageManagers.Actions.UPGRADE), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static bool AddAptArguments(CliWrapArgumentBuilder  args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.REMOVE, Const.Setup.PackageManagers.Actions.INSTALL), true);
        if (operation == SetupOperation.Update)
            args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.ONLY_UPGRADE), true);
        args.AddArgument(new DashedArgument(Const.Setup.PackageManagers.Arguments.YES_SHORT), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static bool AddDnfArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.REMOVE, Const.Setup.PackageManagers.Actions.UPGRADE), true);
        args.AddArgument(new DashedArgument(Const.Setup.PackageManagers.Arguments.YES_SHORT), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static bool AddPacmanArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        if (operation == SetupOperation.Uninstall)
            args.AddArgument(new DashedArgument(Const.Setup.PackageManagers.Arguments.PACMAN_REMOVE), true);
        else
            args.AddArgument(new DashedArgument(Const.Setup.PackageManagers.Arguments.PACMAN_SYNC), true);

        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.PACMAN_NO_CONFIRM), true);
        if (operation == SetupOperation.Install)
            args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.PACMAN_NEEDED), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static bool AddZypperArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.INSTALL, Const.Setup.PackageManagers.Actions.REMOVE, Const.Setup.PackageManagers.Actions.UPDATE), true);
        args.AddArgument(new DashedArgument(Const.Setup.PackageManagers.Arguments.YES_SHORT), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static bool AddApkArguments(CliWrapArgumentBuilder args, SetupOperation operation)
    {
        args.AddArgument(GetAction(operation, Const.Setup.PackageManagers.Actions.ADD, Const.Setup.PackageManagers.Actions.DELETE, Const.Setup.PackageManagers.Actions.UPGRADE), true);
        args.AddArgument(new DoubleDashedArgument(Const.Setup.PackageManagers.Arguments.NO_PROGRESS), true);
        args.AddArgument(Const.GIT, true);
        return true;
    }

    private static string GetAction(SetupOperation operation, string installAction, string uninstallAction, string updateAction)
        => operation == SetupOperation.Install
            ? installAction
            : operation == SetupOperation.Uninstall
            ? uninstallAction
            : updateAction;
}



