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

namespace GitGud.Repositories.Restoring;

public sealed class RestoreOptions : IOperationOptions
{
    const int SourceIndex = 0;
    const int StagedIndex = 1;
    const int WorktreeIndex = 2;
    const int OursIndex = 3;
    const int TheirsIndex = 4;
    const int MergeIndex = 5;
    const int IgnoreUnmergedIndex = 6;
    const int IgnoreSkipWorktreeBitsIndex = 7;
    const int OverlayIndex = 8;
    const int RecurseSubmodulesIndex = 9;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[10];

    /// <summary>Paths (or pathspecs) to restore.</summary>
    public IReadOnlyList<FilePath>? Paths { get; init; }

    /// <summary>Tree, commit, branch or tag to restore from (--source). Blank values are ignored.</summary>
    public string? Source
    {
        get => arguments[SourceIndex].Transform(static v => v.Second, static () => default!);
        init => arguments[SourceIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(Const.Repository.Restoring.SOURCE, value!))
            : Optional<Pair>.None;
    }

    /// <summary>Restores the index (--staged).</summary>
    public bool Staged
    {
        get => arguments[StagedIndex].HasValue;
        init => SetFlag(StagedIndex, value, Const.Repository.Restoring.STAGED);
    }

    /// <summary>Restores the working tree (--worktree).</summary>
    public bool Worktree
    {
        get => arguments[WorktreeIndex].HasValue;
        init => SetFlag(WorktreeIndex, value, Const.Repository.Restoring.WORKTREE);
    }

    /// <summary>Uses stage #2 for unmerged paths (--ours).</summary>
    public bool Ours
    {
        get => arguments[OursIndex].HasValue;
        init => SetFlag(OursIndex, value, Const.Repository.Restoring.OURS);
    }

    /// <summary>Uses stage #3 for unmerged paths (--theirs).</summary>
    public bool Theirs
    {
        get => arguments[TheirsIndex].HasValue;
        init => SetFlag(TheirsIndex, value, Const.Repository.Restoring.THEIRS);
    }

    /// <summary>Recreates the conflicted merge in unmerged paths (--merge).</summary>
    public bool Merge
    {
        get => arguments[MergeIndex].HasValue;
        init => SetFlag(MergeIndex, value, Const.Repository.Restoring.MERGE);
    }

    /// <summary>Leaves unmerged paths alone instead of aborting (--ignore-unmerged).</summary>
    public bool IgnoreUnmerged
    {
        get => arguments[IgnoreUnmergedIndex].HasValue;
        init => SetFlag(IgnoreUnmergedIndex, value, Const.Repository.Restoring.IGNORE_UNMERGED);
    }

    /// <summary>Ignores sparse checkout patterns (--ignore-skip-worktree-bits).</summary>
    public bool IgnoreSkipWorktreeBits
    {
        get => arguments[IgnoreSkipWorktreeBitsIndex].HasValue;
        init => SetFlag(IgnoreSkipWorktreeBitsIndex, value, Const.Repository.Restoring.IGNORE_SKIP_WORKTREE_BITS);
    }

    /// <summary>When true never removes files (--overlay); when false removes files missing from the source (--no-overlay); null uses the git default.</summary>
    public bool? Overlay
    {
        get => GetTriState(OverlayIndex, Const.Repository.Restoring.OVERLAY);
        init => SetTriState(OverlayIndex, value, Const.Repository.Restoring.OVERLAY, Const.Repository.Restoring.NO_OVERLAY);
    }

    /// <summary>When true recurses into submodules (--recurse-submodules); when false does not (--no-recurse-submodules); null uses the git default.</summary>
    public bool? RecurseSubmodules
    {
        get => GetTriState(RecurseSubmodulesIndex, Const.Repository.Restoring.RECURSE_SUBMODULES);
        init => SetTriState(RecurseSubmodulesIndex, value,
            Const.Repository.Restoring.RECURSE_SUBMODULES, Const.Repository.Restoring.NO_RECURSE_SUBMODULES);
    }

    private void SetFlag(int index, bool value, string name)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(name))
            : Optional<Pair>.None;

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
        argumentBuilder.AddArgument(Const.RESTORE, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (pair.TwoValues)
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
            else
                argumentBuilder.Flag(pair.First!);
        }

        var paths = Paths;
        if (paths is null)
            return;

        argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
        for (var i = 0; i < paths.Count; i++)
            argumentBuilder.AddArgument(paths[i], true);
    }

    public Result<Nothing, IError> Validate()
    {
        var paths = Paths;
        var source = Source;
        var hasSource = !string.IsNullOrWhiteSpace(source);
        string? error = null;

        if (paths is null || paths.Count == 0)
            error = Const.Repository.Restoring.Errors.PATHS_REQUIRED;
        else if (hasSource && source![0] == Const.Repository.Branching.OPTION_PREFIX)
            error = Const.Repository.Restoring.Errors.SOURCE_INVALID;
        else if (Ours && Theirs)
            error = Const.Repository.Restoring.Errors.OURS_THEIRS_CONFLICT;
        else if (hasSource && (Ours || Theirs || Merge))
            error = Const.Repository.Restoring.Errors.SOURCE_CONFLICT;
        else
        {
            for (var i = 0; i < paths.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                {
                    error = Const.Repository.Restoring.Errors.PATH_INVALID;
                    break;
                }
            }
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_RESTORE, error));

        return Result<Nothing, IError>.Some(new());
    }
}