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

using BranchConst = GitGud.Constants.Const.Repository.Branching;

namespace GitGud.Repositories.Branching;

public sealed class SwitchBranchOptions : IOperationOptions
{
    const int DiscardChangesIndex = 0;
    const int MergeIndex = 1;
    const int ConflictStyleIndex = 2;
    const int TrackIndex = 3;
    const int GuessIndex = 4;
    const int IgnoreOtherWorktreesIndex = 5;
    const int RecurseSubmodulesIndex = 6;
    const int DetachIndex = 7;
    const int CreateModeIndex = 8;
    const int NewBranchIndex = 9;
    const int TargetIndex = 10;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[11];
    private BranchCreateMode _createMode;
    private BranchConflictStyle _conflictStyle;
    private BranchTrackMode _track;

    /// <summary>Branch to switch to ("-" is the previously checked out branch). When creating a branch or detaching it is the optional start point. Blank values are ignored.</summary>
    public string? Target
    {
        get => arguments[TargetIndex].Transform(static v => v.First, static () => default!);
        init => arguments[TargetIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    /// <summary>Whether and how a new branch is created: Create (-c), Reset (-C) or Orphan (--orphan).</summary>
    public BranchCreateMode CreateMode
    {
        get => _createMode;
        init
        {
            _createMode = value;
            arguments[CreateModeIndex] = value switch
            {
                BranchCreateMode.Create => Optional<Pair>.Some(new Pair(BranchConst.SWITCH_CREATE_SHORT)),
                BranchCreateMode.Reset => Optional<Pair>.Some(new Pair(BranchConst.SWITCH_FORCE_CREATE_SHORT)),
                BranchCreateMode.Orphan => Optional<Pair>.Some(new Pair(BranchConst.ORPHAN)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>The branch to create when CreateMode is not None.</summary>
    public Branch NewBranch
    {
        get => new(arguments[NewBranchIndex].Transform(static v => v.First, static () => default!) ?? string.Empty);
        init
        {
            string? name = value;
            arguments[NewBranchIndex] = Optional<Pair>.Some(new Pair(name ?? string.Empty));
        }
    }

    /// <summary>Switches to a commit with a detached HEAD (--detach). Cannot be combined with branch creation.</summary>
    public bool Detach
    {
        get => arguments[DetachIndex].HasValue;
        init => SetFlag(DetachIndex, BranchConst.DETACH, value);
    }

    /// <summary>Proceeds even if the index or working tree differs from HEAD (--discard-changes). Cannot be combined with Merge.</summary>
    public bool DiscardChanges
    {
        get => arguments[DiscardChangesIndex].HasValue;
        init => SetFlag(DiscardChangesIndex, BranchConst.DISCARD_CHANGES, value);
    }

    /// <summary>Carries local changes over by stashing and reapplying them (-m). Cannot be combined with DiscardChanges.</summary>
    public bool Merge
    {
        get => arguments[MergeIndex].HasValue;
        init => SetFlag(MergeIndex, BranchConst.MERGE_SHORT, value);
    }

    /// <summary>Conflict hunk style used with Merge (--conflict).</summary>
    public BranchConflictStyle ConflictStyle
    {
        get => _conflictStyle;
        init
        {
            _conflictStyle = value;
            arguments[ConflictStyleIndex] = value switch
            {
                BranchConflictStyle.Merge => Optional<Pair>.Some(new Pair(BranchConst.CONFLICT, BranchConst.CONFLICT_MERGE)),
                BranchConflictStyle.Diff3 => Optional<Pair>.Some(new Pair(BranchConst.CONFLICT, BranchConst.CONFLICT_DIFF3)),
                BranchConflictStyle.ZDiff3 => Optional<Pair>.Some(new Pair(BranchConst.CONFLICT, BranchConst.CONFLICT_ZDIFF3)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>Upstream tracking behavior for new branches (--track / --no-track).</summary>
    public BranchTrackMode Track
    {
        get => _track;
        init
        {
            _track = value;
            arguments[TrackIndex] = value switch
            {
                BranchTrackMode.Direct => Optional<Pair>.Some(new Pair(BranchConst.TRACK, BranchConst.TRACK_DIRECT)),
                BranchTrackMode.Inherit => Optional<Pair>.Some(new Pair(BranchConst.TRACK, BranchConst.TRACK_INHERIT)),
                BranchTrackMode.None => Optional<Pair>.Some(new Pair(BranchConst.NO_TRACK)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>When true guesses a remote-tracking branch (--guess), when false disables it (--no-guess); null uses the git default.</summary>
    public bool? Guess
    {
        get => GetTriState(GuessIndex, BranchConst.GUESS);
        init => SetTriState(GuessIndex, value, BranchConst.GUESS, BranchConst.NO_GUESS);
    }

    /// <summary>Switches even if another worktree uses the branch (--ignore-other-worktrees).</summary>
    public bool IgnoreOtherWorktrees
    {
        get => arguments[IgnoreOtherWorktreesIndex].HasValue;
        init => SetFlag(IgnoreOtherWorktreesIndex, BranchConst.IGNORE_OTHER_WORKTREES, value);
    }

    /// <summary>When true updates active submodules (--recurse-submodules), when false does not (--no-recurse-submodules); null uses the git default.</summary>
    public bool? RecurseSubmodules
    {
        get => GetTriState(RecurseSubmodulesIndex, BranchConst.RECURSE_SUBMODULES);
        init => SetTriState(RecurseSubmodulesIndex, value, BranchConst.RECURSE_SUBMODULES, BranchConst.NO_RECURSE_SUBMODULES);
    }

    private void SetFlag(int index, string name, bool value)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(name))
            : Optional<Pair>.None;

    private void SetTriState(int index, bool? value, string onName, string offName)
        => arguments[index] = value is bool enabled
            ? Optional<Pair>.Some(new Pair(enabled ? onName : offName))
            : Optional<Pair>.None;

    private bool? GetTriState(int index, string onName)
        => arguments[index].Transform(
            v => string.Equals(v.First, onName, StringComparison.Ordinal),
            static () => default);

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(BranchConst.SWITCH, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            if (i == NewBranchIndex && _createMode == BranchCreateMode.None)
                continue;

            var pair = optional.Value;
            switch (i)
            {
                case MergeIndex:
                case NewBranchIndex:
                case TargetIndex:
                    argumentBuilder.AddArgument(pair.First!, true);
                    break;
                case CreateModeIndex:
                    if (_createMode == BranchCreateMode.Orphan)
                        argumentBuilder.Flag(pair.First!);
                    else
                        argumentBuilder.AddArgument(pair.First!, true);
                    break;
                default:
                    if (pair.TwoValues)
                        argumentBuilder.AddArgument(
                            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
                    else
                        argumentBuilder.Flag(pair.First!);
                    break;
            }
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var target = Target;
        var hasTarget = target is not null;
        string? error = null;

        if (_createMode != BranchCreateMode.None)
        {
            string? name = NewBranch;
            if (string.IsNullOrWhiteSpace(name))
                error = BranchConst.Errors.NAME_REQUIRED;
            else if (name![0] == BranchConst.OPTION_PREFIX)
                error = BranchConst.Errors.NAME_INVALID;
            else if (Detach)
                error = BranchConst.Errors.DETACH_WITH_CREATE;
            else if (_createMode == BranchCreateMode.Orphan && hasTarget)
                error = BranchConst.Errors.ORPHAN_WITH_TARGET;
        }
        else if (!hasTarget && !Detach)
        {
            error = BranchConst.Errors.TARGET_REQUIRED;
        }

        if (error is null && DiscardChanges && Merge)
            error = BranchConst.Errors.FORCE_WITH_MERGE;
        else if (error is null && hasTarget && target![0] == BranchConst.OPTION_PREFIX && target != BranchConst.PREVIOUS_BRANCH)
            error = BranchConst.Errors.TARGET_INVALID;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(BranchConst.GIT_SWITCH, error));

        return Result<Nothing, IError>.Some(new());
    }
}