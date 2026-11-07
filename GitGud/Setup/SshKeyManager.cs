// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using CliWrap;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Processes;
using GitGud.Setup.Errors;
using GitGud.Utility;

namespace GitGud.Setup;

internal static class SshKeyManager
{
    private static readonly SemaphoreSlim AgentInitializationLock = new(1, 1);
    private static IReadOnlyDictionary<string, string>? _managedAgentEnvironment;

    public static async Task<Result<IReadOnlyDictionary<string, string>, IError>> StartAgentAndAddKeyAsync(
        string privateKeyPath,
        bool requireAskPass,
        CancellationToken cancellationToken)
    {
        try
        {
            return await StartAgentAndAddKeyCoreAsync(privateKeyPath, requireAskPass, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_START,
                Const.Setup.Errors.SSH_AGENT_START_FAILED));
        }
    }

    private static async Task<Result<IReadOnlyDictionary<string, string>, IError>> StartAgentAndAddKeyCoreAsync(
        string privateKeyPath,
        bool requireAskPass,
        CancellationToken cancellationToken)
    {
        var agentResult = await EnsureAgentAsync(cancellationToken).ConfigureAwait(false);
        if (agentResult.HasNone)
            return Result<IReadOnlyDictionary<string, string>, IError>.None(agentResult.Handle());

        var windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var sshAddName = windows ? Const.Setup.Executables.WINDOWS_SSH_ADD : Const.Setup.Executables.SSH_ADD;
        var sshAddPath = FilePathHelpers.FindOnPath(sshAddName);
        if (sshAddPath is null)
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_ADD,
                Const.Setup.Errors.SSH_AGENT_ADD_FAILED));

        var environment = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var variable in agentResult.Unwrap())
            environment[variable.Key] = variable.Value;

        if (requireAskPass)
        {
            var askPassPath = FindAskPassExecutable();
            if (askPassPath is null)
                return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                    Const.Setup.Operations.GIT_SSH_AGENT_ADD,
                    Const.Setup.Errors.SSH_ASKPASS_NOT_FOUND));

            environment[Const.Setup.Ssh.SSH_ASKPASS] = askPassPath;
            environment[Const.Setup.Ssh.SSH_ASKPASS_REQUIRE] = Const.Setup.Ssh.SSH_ASKPASS_FORCE;
        }

        try
        {
            var command = Cli.Wrap(sshAddPath)
                .WithArguments(arguments => arguments.Add(privateKeyPath, true))
                .WithEnvironmentVariables(environment);
            var processResult = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            if (processResult.ExitCode != 0)
                return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupProcessError(
                    Const.Setup.Operations.GIT_SSH_AGENT_ADD,
                    Const.Setup.Errors.SSH_AGENT_ADD_FAILED,
                    processResult.ExitCode,
                    processResult.StandardError));

            return Result<IReadOnlyDictionary<string, string>, IError>.Some(agentResult.Unwrap());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_ADD,
                Const.Setup.Errors.SSH_AGENT_ADD_FAILED));
        }
    }

    public static async Task<Result<SshKeyResult, IError>> GenerateKeyPairAsync(IGitSshKeyContext context)
    {
        if (context is null)
            return Result<SshKeyResult, IError>.None(new SetupContextError());

        var requestedPath = context.PrivateKeyPath.ToString();
        if (string.IsNullOrWhiteSpace(requestedPath))
            return Result<SshKeyResult, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_KEY_GENERATION,
                Const.Setup.Errors.SSH_KEY_PATH_REQUIRED));

        string? privateKeyPath = null;
        string? publicKeyPath = null;
        try
        {
            privateKeyPath = Path.GetFullPath(requestedPath);
            publicKeyPath = string.Concat(privateKeyPath, Const.Setup.Ssh.PUBLIC_KEY_EXTENSION);
            if (File.Exists(privateKeyPath) || File.Exists(publicKeyPath))
                return Result<SshKeyResult, IError>.None(new SetupSshError(
                    Const.Setup.Operations.GIT_SSH_KEY_GENERATION,
                    Const.Setup.Errors.SSH_KEY_ALREADY_EXISTS));

            var keygenName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Const.Setup.Executables.WINDOWS_SSH_KEYGEN
                : Const.Setup.Executables.SSH_KEYGEN;
            var keygenPath = FilePathHelpers.FindOnPath(keygenName);
            if (keygenPath is null)
                return Result<SshKeyResult, IError>.None(new SetupSshError(
                    Const.Setup.Operations.GIT_SSH_KEY_GENERATION,
                    Const.Setup.Errors.SSH_KEYGEN_NOT_FOUND));

            var directory = Path.GetDirectoryName(privateKeyPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var command = Cli.Wrap(keygenPath).WithArguments(arguments =>
            {
                arguments.Add(new DashedArgument<string>(Const.Setup.Ssh.Arguments.TYPE, Const.Setup.Ssh.ED25519), true);
                arguments.Add(new DashedArgument<string>(Const.Setup.Ssh.Arguments.PASSPHRASE, context.Passphrase ?? string.Empty), true);
                arguments.Add(new DashedArgument<string>(Const.Setup.Ssh.Arguments.OUTPUT, privateKeyPath), true);
                arguments.Add(new DashedArgument(Const.Setup.Ssh.Arguments.QUIET), true);
            });

            var processResult = await ProcessRunner.ExecuteAsync(command, context.CancellationToken).ConfigureAwait(false);
            if (processResult.ExitCode != 0)
            {
                return Result<SshKeyResult, IError>.None(new SetupProcessError(
                    Const.Setup.Operations.GIT_SSH_KEY_GENERATION,
                    Const.Setup.Errors.SSH_KEY_GENERATION_FAILED,
                    processResult.ExitCode,
                    processResult.StandardError));
            }

            if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
            {
                return Result<SshKeyResult, IError>.None(new SetupSshError(
                    Const.Setup.Operations.GIT_SSH_KEY_GENERATION,
                    Const.Setup.Errors.SSH_KEY_GENERATION_FAILED));
            }

            var publicKey = File.ReadAllText(publicKeyPath).Trim();
            return Result<SshKeyResult, IError>.Some(new SshKeyResult(
                new FilePath(privateKeyPath),
                new FilePath(publicKeyPath),
                publicKey,
                new Dictionary<string, string>(StringComparer.Ordinal)));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<SshKeyResult, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_KEY_GENERATION,
                Const.Setup.Errors.SSH_KEY_GENERATION_FAILED));
        }
    }

    private static async Task<Result<IReadOnlyDictionary<string, string>, IError>> EnsureAgentAsync(CancellationToken cancellationToken)
    {
        await AgentInitializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_managedAgentEnvironment is not null)
                return Result<IReadOnlyDictionary<string, string>, IError>.Some(_managedAgentEnvironment);

            var result = await EnsureAgentCoreAsync(cancellationToken).ConfigureAwait(false);
            if (result.HasNone)
                return result;

            var environment = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var variable in result.Unwrap())
                environment[variable.Key] = variable.Value;
            _managedAgentEnvironment = new ReadOnlyDictionary<string, string>(environment);
            return Result<IReadOnlyDictionary<string, string>, IError>.Some(_managedAgentEnvironment);
        }
        finally
        {
            AgentInitializationLock.Release();
        }
    }

    private static async Task<Result<IReadOnlyDictionary<string, string>, IError>> EnsureAgentCoreAsync(CancellationToken cancellationToken)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return await EnsureWindowsAgentAsync(cancellationToken).ConfigureAwait(false);

        var isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        if (!isMac && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_START,
                Const.Setup.Errors.SSH_AGENT_START_FAILED));

        var existingSocket = Environment.GetEnvironmentVariable(Const.Setup.Ssh.SSH_AUTH_SOCK);
        if (!string.IsNullOrWhiteSpace(existingSocket))
        {
            var existingEnvironment = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [Const.Setup.Ssh.SSH_AUTH_SOCK] = existingSocket
            };
            var existingPid = Environment.GetEnvironmentVariable(Const.Setup.Ssh.SSH_AGENT_PID);
            if (!string.IsNullOrWhiteSpace(existingPid))
                existingEnvironment[Const.Setup.Ssh.SSH_AGENT_PID] = existingPid;
            return Result<IReadOnlyDictionary<string, string>, IError>.Some(existingEnvironment);
        }

        var agentPath = FilePathHelpers.FindOnPath(Const.Setup.Executables.SSH_AGENT);
        if (agentPath is null)
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_START,
                Const.Setup.Errors.SSH_AGENT_START_FAILED));

        try
        {
            var command = Cli.Wrap(agentPath)
                .WithArguments(arguments => arguments.Add(Const.Setup.Ssh.SSH_AGENT_OUTPUT_FORMAT, true));
            var processResult = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            var agentEnvironment = processResult.ExitCode == 0
                ? ParseAgentEnvironment(processResult.StandardOutput)
                : null;
            if (agentEnvironment is null
                || !agentEnvironment.TryGetValue(Const.Setup.Ssh.SSH_AUTH_SOCK, out var agentSocket)
                || string.IsNullOrWhiteSpace(agentSocket))
                return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupProcessError(
                    Const.Setup.Operations.GIT_SSH_AGENT_START,
                    Const.Setup.Errors.SSH_AGENT_START_FAILED,
                    processResult.ExitCode,
                    processResult.StandardError));

            return Result<IReadOnlyDictionary<string, string>, IError>.Some(agentEnvironment);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_START,
                Const.Setup.Errors.SSH_AGENT_START_FAILED));
        }
    }

    private static async Task<Result<IReadOnlyDictionary<string, string>, IError>> EnsureWindowsAgentAsync(CancellationToken cancellationToken)
    {
        var serviceControlPath = FilePathHelpers.FindOnPath(Const.Setup.Executables.WINDOWS_SERVICE_CONTROL);
        if (serviceControlPath is null)
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_START,
                Const.Setup.Errors.SSH_AGENT_START_FAILED));

        try
        {
            var query = await ProcessRunner.ExecuteAsync(
                Cli.Wrap(serviceControlPath).WithArguments(arguments =>
                {
                    arguments.Add(Const.Setup.Ssh.Arguments.SERVICE_QUERY, true);
                    arguments.Add(Const.Setup.Ssh.WINDOWS_SSH_AGENT_SERVICE, true);
                }), cancellationToken).ConfigureAwait(false);

            if (query.ExitCode == 0 && (query.StandardOutput?.IndexOf(Const.Setup.Ssh.SSH_AGENT_RUNNING, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                return Result<IReadOnlyDictionary<string, string>, IError>.Some(new Dictionary<string, string>(StringComparer.Ordinal));

            if (!ElevationHelper.IsElevated())
                return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupElevationRequiredError(
                    Const.Setup.Operations.GIT_SSH_AGENT_START,
                    Const.Setup.Elevation.REQUIRED_WINDOWS));

            await ProcessRunner.ExecuteAsync(
                    Cli.Wrap(serviceControlPath).WithArguments(arguments =>
                    {
                        arguments.Add(Const.Setup.Ssh.Arguments.SERVICE_CONFIG, true);
                        arguments.Add(Const.Setup.Ssh.WINDOWS_SSH_AGENT_SERVICE, true);
                        arguments.Add(Const.Setup.Ssh.Arguments.SERVICE_START_SETTING, true);
                        arguments.Add(Const.Setup.Ssh.Arguments.SERVICE_DEMAND_SETTING, true);
                    }), cancellationToken).ConfigureAwait(false);

            var start = await ProcessRunner.ExecuteAsync(
                Cli.Wrap(serviceControlPath).WithArguments(arguments =>
                {
                    arguments.Add(Const.Setup.Ssh.Arguments.SERVICE_START, true);
                    arguments.Add(Const.Setup.Ssh.WINDOWS_SSH_AGENT_SERVICE, true);
                }), cancellationToken).ConfigureAwait(false);

            if (start.ExitCode == 0)
                return Result<IReadOnlyDictionary<string, string>, IError>.Some(new Dictionary<string, string>(StringComparer.Ordinal));

            var status = await ProcessRunner.ExecuteAsync(
                Cli.Wrap(serviceControlPath).WithArguments(arguments =>
                {
                    arguments.Add(Const.Setup.Ssh.Arguments.SERVICE_QUERY, true);
                    arguments.Add(Const.Setup.Ssh.WINDOWS_SSH_AGENT_SERVICE, true);
                }), cancellationToken).ConfigureAwait(false);
            if ((status.StandardOutput?.IndexOf(Const.Setup.Ssh.SSH_AGENT_RUNNING, StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
                return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupProcessError(
                    Const.Setup.Operations.GIT_SSH_AGENT_START,
                    Const.Setup.Errors.SSH_AGENT_START_FAILED,
                    start.ExitCode,
                    start.StandardError));

            return Result<IReadOnlyDictionary<string, string>, IError>.Some(new Dictionary<string, string>(StringComparer.Ordinal));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result<IReadOnlyDictionary<string, string>, IError>.None(new SetupSshError(
                Const.Setup.Operations.GIT_SSH_AGENT_START,
                Const.Setup.Errors.SSH_AGENT_START_FAILED));
        }
    }

    private static Dictionary<string, string>? ParseAgentEnvironment(string? output)
    {
        if (output is null || string.IsNullOrWhiteSpace(output))
            return null;

        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var assignmentEnd = line.IndexOf(';');
            var assignment = assignmentEnd >= 0 ? line.Substring(0, assignmentEnd) : line;
            var separator = assignment.IndexOf('=');
            if (separator <= 0)
                continue;

            var name = assignment.Substring(0, separator).Trim();
            var value = assignment.Substring(separator + 1).Trim().Trim('"', '\'');
            if (name == Const.Setup.Ssh.SSH_AUTH_SOCK || name == Const.Setup.Ssh.SSH_AGENT_PID)
                environment[name] = value;
        }

        return environment;
    }

    private static string? FindAskPassExecutable()
    {
        var configuredPath = Environment.GetEnvironmentVariable(Const.Setup.Ssh.SSH_ASKPASS);
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
            return Path.GetFullPath(configuredPath);

        var executable = FilePathHelpers.FindOnPath(Const.Setup.Executables.SSH_ASKPASS)
            ?? FilePathHelpers.FindOnPath(Const.Setup.Executables.KSSH_ASKPASS)
            ?? FilePathHelpers.FindOnPath(Const.Setup.Executables.GNOME_SSH_ASKPASS);
        if (executable is not null)
            return executable;

        var macAskPass = Const.Setup.Ssh.MAC_SSH_ASKPASS_PATH;
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && File.Exists(macAskPass)
            ? macAskPass
            : null;
    }

}
