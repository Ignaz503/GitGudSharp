#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using BranchConst = GitGud.Constants.Const.Repository.Branching;

namespace GitGud.Repositories.Branching;

public sealed class CheckoutBranchOptions : IOperationOptions
{
    const int ForceIndex = 0;
    const int MergeIndex = 1;
    const int ConflictStyleIndex = 2;
    const int TrackIndex = 3;
    const int GuessIndex = 4;
    const int CreateReflogIndex = 5;
    const int IgnoreOtherWorktreesIndex = 6;
    const int OverwriteIgnoreIndex = 7;
    const int RecurseSubmodulesIndex = 8;
    const int DetachIndex = 9;
    const int CreateModeIndex = 10;
    const int NewBranchIndex = 11;
    const int TargetIndex = 12;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[13];
    private BranchCreateMode _createMode;
    private BranchConflictStyle _conflictStyle;
    private BranchTrackMode _track;

    /// <summary>Branch or commit to switch to ("-" is the previously checked out branch). When creating a branch it is the start point and defaults to the current commit. Optional with Detach, which then detaches at the current branch tip. Blank values are ignored.</summary>
    public string? Target
    {
        get => arguments[TargetIndex].Transform(static v => v.First, static () => default!);
        init => arguments[TargetIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    /// <summary>Whether and how a new branch is created.</summary>
    public BranchCreateMode CreateMode
    {
        get => _createMode;
        init
        {
            _createMode = value;
            arguments[CreateModeIndex] = value switch
            {
                BranchCreateMode.Create => Optional<Pair>.Some(new Pair(BranchConst.CREATE_SHORT)),
                BranchCreateMode.Reset => Optional<Pair>.Some(new Pair(BranchConst.RESET_SHORT)),
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

    /// <summary>Checks out the commit with a detached HEAD (--detach). Cannot be combined with branch creation.</summary>
    public bool Detach
    {
        get => arguments[DetachIndex].HasValue;
        init => SetFlag(DetachIndex, BranchConst.DETACH, value);
    }

    /// <summary>Discards local changes and untracked files in the way (-f). Cannot be combined with Merge.</summary>
    public bool Force
    {
        get => arguments[ForceIndex].HasValue;
        init => SetFlag(ForceIndex, BranchConst.FORCE_SHORT, value);
    }

    /// <summary>Carries local changes over by stashing and reapplying them (-m). Cannot be combined with Force.</summary>
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

    /// <summary>Creates the reflog of the new branch (-l).</summary>
    public bool CreateReflog
    {
        get => arguments[CreateReflogIndex].HasValue;
        init => SetFlag(CreateReflogIndex, BranchConst.CREATE_REFLOG_SHORT, value);
    }

    /// <summary>Checks the branch out even if another worktree uses it (--ignore-other-worktrees).</summary>
    public bool IgnoreOtherWorktrees
    {
        get => arguments[IgnoreOtherWorktreesIndex].HasValue;
        init => SetFlag(IgnoreOtherWorktreesIndex, BranchConst.IGNORE_OTHER_WORKTREES, value);
    }

    /// <summary>When true silently overwrites ignored files (--overwrite-ignore), when false aborts instead (--no-overwrite-ignore); null uses the git default.</summary>
    public bool? OverwriteIgnore
    {
        get => GetTriState(OverwriteIgnoreIndex, BranchConst.OVERWRITE_IGNORE);
        init => SetTriState(OverwriteIgnoreIndex, value, BranchConst.OVERWRITE_IGNORE, BranchConst.NO_OVERWRITE_IGNORE);
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
        argumentBuilder.AddArgument(BranchConst.CHECKOUT, true);

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
                case ForceIndex:
                case MergeIndex:
                case CreateReflogIndex:
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
        }
        else if (!hasTarget && !Detach)
        {
            error = BranchConst.Errors.TARGET_REQUIRED;
        }

        if (error is null && Force && Merge)
            error = BranchConst.Errors.FORCE_WITH_MERGE;
        else if (error is null && hasTarget && target![0] == BranchConst.OPTION_PREFIX && target != BranchConst.PREVIOUS_BRANCH)
            error = BranchConst.Errors.TARGET_INVALID;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(BranchConst.GIT_CHECKOUT, error));

        return Result<Nothing, IError>.Some(new());
    }
}