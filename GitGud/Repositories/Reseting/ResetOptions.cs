// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

namespace GitGud.Repositories.Reseting;

public sealed class ResetOptions : IOperationOptions
{
    const int ModeIndex = 0;
    const int IntentToAddIndex = 1;
    const int RecurseSubmodulesIndex = 2;
    const int RefreshIndex = 3;
    const int CommitIndex = 4;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[5];
    private ResetMode _mode;

    /// <summary>Reset mode; Default lets git use --mixed. Only Default or Mixed may be combined with paths.</summary>
    public ResetMode Mode
    {
        get => _mode;
        init
        {
            _mode = value;
            arguments[ModeIndex] = value switch
            {
                ResetMode.Soft => Optional<Pair>.Some(new Pair(Const.Repository.Resetting.SOFT)),
                ResetMode.Mixed => Optional<Pair>.Some(new Pair(Const.Repository.Resetting.MIXED)),
                ResetMode.Hard => Optional<Pair>.Some(new Pair(Const.Repository.Resetting.HARD)),
                ResetMode.Merge => Optional<Pair>.Some(new Pair(Const.Repository.Resetting.MERGE)),
                ResetMode.Keep => Optional<Pair>.Some(new Pair(Const.Repository.Resetting.KEEP)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>Commit or tree-ish to reset to; defaults to HEAD. Blank values are ignored.</summary>
    public string? Commit
    {
        get => arguments[CommitIndex].Transform(static v => v.First, static () => default!);
        init => arguments[CommitIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    /// <summary>Paths (or pathspecs) whose staged version is reset. When set, only the index is updated.</summary>
    public IReadOnlyList<FilePath>? Paths { get; init; }

    /// <summary>Marks removed paths as intent-to-add (-N). Only valid for a mixed reset without paths.</summary>
    public bool IntentToAdd
    {
        get => arguments[IntentToAddIndex].HasValue;
        init => arguments[IntentToAddIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Resetting.INTENT_TO_ADD_SHORT))
            : Optional<Pair>.None;
    }

    /// <summary>When true passes --recurse-submodules, when false --no-recurse-submodules; null uses the git default.</summary>
    public bool? RecurseSubmodules
    {
        get => GetTriState(RecurseSubmodulesIndex, Const.Repository.Resetting.RECURSE_SUBMODULES);
        init => SetTriState(RecurseSubmodulesIndex, value,
            Const.Repository.Resetting.RECURSE_SUBMODULES, Const.Repository.Resetting.NO_RECURSE_SUBMODULES);
    }

    /// <summary>When true passes --refresh, when false --no-refresh; null uses the git default.</summary>
    public bool? Refresh
    {
        get => GetTriState(RefreshIndex, Const.Repository.Resetting.REFRESH);
        init => SetTriState(RefreshIndex, value,
            Const.Repository.Resetting.REFRESH, Const.Repository.Resetting.NO_REFRESH);
    }

    private void SetTriState(int index, bool? value, string onName, string offName)
        => arguments[index] = value is bool enabled
            ? Optional<Pair>.Some(new Pair(enabled ? onName : offName))
            : Optional<Pair>.None;

    private bool? GetTriState(int index, string onName)
        => arguments[index].Transform(
            v => (bool?)string.Equals(v.First, onName, StringComparison.Ordinal),
            static () => default(bool?));

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.RESET, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var name = optional.Value.First!;
            if (i == IntentToAddIndex || i == CommitIndex)
                argumentBuilder.AddArgument(name, true);
            else
                argumentBuilder.Flag(name);
        }

        var paths = Paths;
        if (paths is not null && paths.Count > 0)
        {
            argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
            for (var i = 0; i < paths.Count; i++)
                argumentBuilder.AddArgument(paths[i], true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var paths = Paths;
        var hasPaths = paths is not null && paths.Count > 0;
        var mode = _mode;
        var isMixed = mode == ResetMode.Default || mode == ResetMode.Mixed;
        var commit = Commit;
        string? error = null;

        if (mode < ResetMode.Default || mode > ResetMode.Keep)
            error = Const.Repository.Resetting.Errors.MODE_INVALID;
        else if (commit is not null && commit[0] == Const.Repository.Branching.OPTION_PREFIX)
            error = Const.Repository.Resetting.Errors.COMMIT_INVALID;
        else if (hasPaths && !isMixed)
            error = Const.Repository.Resetting.Errors.MODE_WITH_PATHS;
        else if (IntentToAdd && (!isMixed || hasPaths))
            error = Const.Repository.Resetting.Errors.INTENT_TO_ADD_INVALID;
        else if (hasPaths)
        {
            for (var i = 0; i < paths!.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                {
                    error = Const.Repository.Resetting.Errors.PATH_INVALID;
                    break;
                }
            }
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_RESET, error));

        return Result<Nothing, IError>.Some(new());
    }
}