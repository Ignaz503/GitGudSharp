// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using CliWrap;
using CliWrap.Builders;

using GitGud.CliArguments;
using GitGud.Config;
using GitGud.Config.Errors;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Processes;
using GitGud.Utility;

namespace GitGud;

public class Configuration : IConfiguration
{
    public Task<Result<Nothing, IError>> Apply(IConfigCollection configCollection, CancellationToken cancelationToken = default)
    {
        if (configCollection is null)
            return Task.FromResult(Result<Nothing, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidOptions)));

        return configCollection.Apply(this, cancelationToken);
    }

    public Task<Result<Nothing, IError>> EditAsync(FileOption fileOption, CancellationToken cancellationToken = default)
        => RunAsync(Const.Config.Commands.EDIT, fileOption, args => args.AddArgument(Const.Config.Flags.EDIT, true), cancellationToken);

    public Task<Result<List<ConfigEntry>, IError>> GetAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, false, false, e => new ConfigEntry(e.Name, e.Value), cancellationToken);

    public Task<Result<List<ConfigEntryNameOnly>, IError>> GetNameOnlyAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, false, false, e => new ConfigEntryNameOnly(e.Name), cancellationToken);

    public Task<Result<List<OriginConfigEntry>, IError>> GetOriginAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, false, true, e => new OriginConfigEntry(new FilePath(e.Origin), e.Name, e.Value), cancellationToken);

    public Task<Result<List<OriginConfigEntryNameOnly>, IError>> GetOriginNameOnlyAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, false, true, e => new OriginConfigEntryNameOnly(new FilePath(e.Origin), e.Name), cancellationToken);

    public Task<Result<List<ScopedConfigEntry>, IError>> GetScopedAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, true, false, e => new ScopedConfigEntry(e.Scope, e.Name, e.Value), cancellationToken);

    public Task<Result<List<ScopedConfigEntryNameOnly>, IError>> GetScopedNameOnlyAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, true, false, e => new ScopedConfigEntryNameOnly(e.Scope, e.Name), cancellationToken);

    public Task<Result<List<ScopedOriginConfigEntry>, IError>> GetScopedOriginAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, true, true, e => new ScopedOriginConfigEntry(e.Scope, new FilePath(e.Origin), e.Name, e.Value), cancellationToken);

    public Task<Result<List<ScopedOriginConfigEntryNameOnly>, IError>> GetScopedOriginNameOnlyAsync(ConfigKey name, ConfigGetQueryParameters query, CancellationToken cancellationToken = default)
        => GetCoreAsync(name, query, true, true, e => new ScopedOriginConfigEntryNameOnly(e.Scope, new FilePath(e.Origin), e.Name), cancellationToken);

    public Task<Result<List<ConfigEntry>, IError>> ListAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, false, false, e => new ConfigEntry(e.Name, e.Value), cancellationToken);

    public Task<Result<List<ConfigEntryNameOnly>, IError>> ListNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, false, false, e => new ConfigEntryNameOnly(e.Name), cancellationToken);

    public Task<Result<List<OriginConfigEntry>, IError>> ListOriginAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, false, true, e => new OriginConfigEntry(new FilePath(e.Origin), e.Name, e.Value), cancellationToken);

    public Task<Result<List<OriginConfigEntryNameOnly>, IError>> ListOriginNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, false, true, e => new OriginConfigEntryNameOnly(new FilePath(e.Origin), e.Name), cancellationToken);

    public Task<Result<List<ScopedConfigEntry>, IError>> ListScopedAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, true, false, e => new ScopedConfigEntry(e.Scope, e.Name, e.Value), cancellationToken);

    public Task<Result<List<ScopedConfigEntryNameOnly>, IError>> ListScopedNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, true, false, e => new ScopedConfigEntryNameOnly(e.Scope, e.Name), cancellationToken);

    public Task<Result<List<ScopedOriginConfigEntry>, IError>> ListScopedOriginAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, true, true, e => new ScopedOriginConfigEntry(e.Scope, new FilePath(e.Origin), e.Name, e.Value), cancellationToken);

    public Task<Result<List<ScopedOriginConfigEntryNameOnly>, IError>> ListScopedOriginNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default)
        => ListCoreAsync(query, true, true, e => new ScopedOriginConfigEntryNameOnly(e.Scope, new FilePath(e.Origin), e.Name), cancellationToken);

    public Task<Result<Nothing, IError>> RemoveSection(Section toRemove, RenameSectionQueryParameters query, CancellationToken cancellationToken = default)
    {
        if (query is null)
            return InvalidOptions<Nothing>();

        var section = toRemove.ToString();
        return RunAsync(Const.Config.Commands.REMOVE_SECTION, query.FileOption, args =>
        {
            args.AddArgument(Const.Config.Flags.REMOVE_SECTION, true);
            args.AddArgument(section, true);
        }, cancellationToken);
    }

    public Task<Result<Nothing, IError>> RenameSectionAsync(Section oldName, Section newName, RenameSectionQueryParameters query, CancellationToken cancellationToken = default)
    {
        if (query is null)
            return InvalidOptions<Nothing>();

        var oldSection = oldName.ToString();
        var newSection = newName.ToString();
        return RunAsync(Const.Config.Commands.RENAME_SECTION, query.FileOption, args =>
        {
            args.AddArgument(Const.Config.Flags.RENAME_SECTION, true);
            args.AddArgument(oldSection, true);
            args.AddArgument(newSection, true);
        }, cancellationToken);
    }

    public Task<Result<Nothing, IError>> SetAsync(ConfigKey name, ConfigValue value, ConfigSetQueryParameters query, CancellationToken cancellationToken = default)
    {
        if (query is null || !name.IsValid)
            return InvalidOptions<Nothing>();

        return RunAsync(Const.Config.Commands.SET, query.FileOption, args =>
        {
            var typeArgument = value.Type.GetTypeArgument();
            if (typeArgument is not null)
                args.AddArgument(string.Format(System.Globalization.CultureInfo.InvariantCulture, Const.Config.Flags.TYPE_FORMAT, typeArgument), true);

            var pattern = query.ValuePattern?.RegexPattern;
            if (pattern is not null && query.ValuePattern!.Value.DoFixedValue)
                args.AddArgument(Const.Config.Flags.FIXED_VALUE, true);

            if (query.DoAllValues)
                args.AddArgument(Const.Config.Flags.REPLACE_ALL, true);

            args.AddArgument(name.Value, true);
            args.AddArgument(value.Value, true);

            if (pattern is not null)
                args.AddArgument(pattern.Value.ToString(), true);
        }, cancellationToken);
    }

    public Task<Result<Nothing, IError>> UnsetAsync(ConfigKey name, ConfigUnsetQueryParameters query, CancellationToken cancellationToken = default)
    {
        if (query is null || !name.IsValid)
            return InvalidOptions<Nothing>();

        return RunAsync(Const.Config.Commands.UNSET, query.FileOption, args =>
        {
            var pattern = query.ValuePattern?.RegexPattern;
            if (pattern is not null && query.ValuePattern!.Value.DoFixedValue)
                args.AddArgument(Const.Config.Flags.FIXED_VALUE, true);

            args.AddArgument(query.DoAllValues ? Const.Config.Flags.UNSET_ALL : Const.Config.Flags.UNSET, true);
            args.AddArgument(name.Value, true);

            if (pattern is not null)
                args.AddArgument(pattern.Value.ToString(), true);
        }, cancellationToken);
    }

    private static Task<Result<T, IError>> InvalidOptions<T>()
        => Task.FromResult(Result<T, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidOptions)));

    private static Task<Result<List<T>, IError>> ListCoreAsync<T>(
        ConfigListQueryParameters query,
        bool showScope,
        bool showOrigin,
        Func<ParsedConfigEntry, T> selector,
        CancellationToken cancellationToken)
    {
        if (query is null || query.FileOption is null)
            return InvalidOptions<List<T>>();

        return QueryAsync(
            Const.Config.Commands.LIST,
            query.FileOption,
            args =>
            {
                if (query.DoIncludes)
                    args.AddArgument(Const.Config.Flags.INCLUDES, true);

                args.AddArgument(Const.Config.Flags.LIST, true);
                args.AddArgument(Const.Config.Flags.NULL_TERMINATED, true);
                AddDisplayFlags(args, showScope, showOrigin);
            },
            showScope,
            showOrigin,
            entries =>
            {
                var regex = query.RegexPattern is null ? null : new System.Text.RegularExpressions.Regex(query.RegexPattern.Value.ToString());
                return Select(entries, selector, regex is null ? null : e => regex.IsMatch(e.Name));
            },
            cancellationToken);
    }

    private static Task<Result<List<T>, IError>> GetCoreAsync<T>(
        ConfigKey name,
        ConfigGetQueryParameters query,
        bool showScope,
        bool showOrigin,
        Func<ParsedConfigEntry, T> selector,
        CancellationToken cancellationToken)
    {
        if (query is null || query.FileOption is null)
            return InvalidOptions<List<T>>();

        string? keyPattern = query.RegexPattern is { } regex
            ? regex.ToString()
            : name.IsValid ? string.Concat("^", ConfigOutputParser.EscapeRegex(name.Value), "$") : null;

        if (keyPattern is null && query.Url is null)
            return InvalidOptions<List<T>>();

        return QueryAsync(
            Const.Config.Commands.GET,
            query.FileOption,
            args =>
            {
                if (query.DoIncludes)
                    args.AddArgument(Const.Config.Flags.INCLUDES, true);

                args.AddArgument(Const.Config.Flags.NULL_TERMINATED, true);
                AddDisplayFlags(args, showScope, showOrigin);

                var valuePattern = query.ValuePattern?.RegexPattern;
                if (valuePattern is not null && query.ValuePattern!.Value.DoFixedValue)
                    args.AddArgument(Const.Config.Flags.FIXED_VALUE, true);

                if (query.Url is { } url)
                {
                    args.AddArgument(Const.Config.Flags.GET_URLMATCH, true);
                    args.AddArgument(name.Value, true);
                    args.AddArgument(url.ToString(), true);
                    return;
                }

                args.AddArgument(Const.Config.Flags.GET_REGEXP, true);
                args.AddArgument(keyPattern!, true);

                if (valuePattern is not null)
                    args.AddArgument(valuePattern.Value.ToString(), true);
            },
            showScope,
            showOrigin,
            entries =>
            {
                var results = Select(entries, selector, null);
                if (results.Count == 0 && query.Default is not null)
                    results.Add(selector(new ParsedConfigEntry(ConfigScope.Default, string.Empty, name.Value, query.Default)));
                else if (!query.DoAllValues && results.Count > 1)
                    results.RemoveRange(0, results.Count - 1);

                return results;
            },
            cancellationToken);
    }

    private static List<T> Select<T>(List<ParsedConfigEntry> entries, Func<ParsedConfigEntry, T> selector, Func<ParsedConfigEntry, bool>? filter)
    {
        var results = new List<T>(entries.Count);
        foreach (var entry in entries)
        {
            if (filter is null || filter(entry))
                results.Add(selector(entry));
        }

        return results;
    }

    private static void AddDisplayFlags(CliWrapArgumentBuilder args, bool showScope, bool showOrigin)
    {
        if (showScope)
            args.AddArgument(Const.Config.Flags.SHOW_SCOPE, true);
        if (showOrigin)
            args.AddArgument(Const.Config.Flags.SHOW_ORIGIN, true);
    }

    private static async Task<Result<List<T>, IError>> QueryAsync<T>(
        string commandName,
        FileOption fileOption,
        Action<CliWrapArgumentBuilder> configure,
        bool showScope,
        bool showOrigin,
        Func<List<ParsedConfigEntry>, List<T>> project,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = Cli.Wrap(Const.GIT).WithArguments(argsI =>
            {
                var args = new CliWrapArgumentBuilder(argsI);
                args.AddArgument(Const.CONFIG, true);
                fileOption.AddArguments(args);
                configure(args);
            });

            var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);

            // git config exits with 1 when the key/pattern matched nothing.
            if (result.ExitCode != 0 && !(result.ExitCode == 1 && string.IsNullOrWhiteSpace(result.StandardError)))
                return Result<List<T>, IError>.None(CreateProcessError(commandName, result));

            var entries = ConfigOutputParser.Parse(result.ExitCode == 0 ? result.StandardOutput : null, showScope, showOrigin);
            return Result<List<T>, IError>.Some(project(entries));
        }
        catch (Exception)
        {
            return Result<List<T>, IError>.None(new ConfigExecutionError());
        }
    }

    private static async Task<Result<Nothing, IError>> RunAsync(
        string commandName,
        FileOption fileOption,
        Action<CliWrapArgumentBuilder> configure,
        CancellationToken cancellationToken)
    {
        if (fileOption is null)
            return Result<Nothing, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidOptions));

        try
        {
            var command = Cli.Wrap(Const.GIT).WithArguments(argsI =>
            {
                var args = new CliWrapArgumentBuilder(argsI);
                args.AddArgument(Const.CONFIG, true);
                fileOption.AddArguments(args);
                configure(args);
            });

            var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            return result.ExitCode == 0
                ? Result<Nothing, IError>.Some(new Nothing())
                : Result<Nothing, IError>.None(CreateProcessError(commandName, result));
        }
        catch (Exception)
        {
            return Result<Nothing, IError>.None(new ConfigExecutionError());
        }
    }

    private static ConfigProcessError CreateProcessError(string commandName, ProcessResult result)
        => new(string.Format(System.Globalization.CultureInfo.InvariantCulture, Const.Config.Errors.COMMAND_FAILED_FORMAT, commandName, result.ExitCode), result.ExitCode, result.StandardError);
}
