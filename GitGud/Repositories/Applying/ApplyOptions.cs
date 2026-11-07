// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using ApplyConst = GitGud.Constants.Const.Repository.Applying;

namespace GitGud.Repositories.Applying;

public sealed class ApplyOptions : IOperationOptions
{
    const int StatIndex = 0;
    const int NumStatIndex = 1;
    const int SummaryIndex = 2;
    const int CheckIndex = 3;
    const int IndexIndex = 4;
    const int CachedIndex = 5;
    const int IntentToAddIndex = 6;
    const int ThreeWayIndex = 7;
    const int OursIndex = 8;
    const int TheirsIndex = 9;
    const int UnionIndex = 10;
    const int ApplyChangesIndex = 11;
    const int NoAddIndex = 12;
    const int ReverseIndex = 13;
    const int RejectIndex = 14;
    const int NulTerminatedIndex = 15;
    const int UnidiffZeroIndex = 16;
    const int IgnoreWhitespaceIndex = 17;
    const int InaccurateEofIndex = 18;
    const int RecountIndex = 19;
    const int VerboseIndex = 20;
    const int QuietIndex = 21;
    const int UnsafePathsIndex = 22;
    const int AllowEmptyIndex = 23;
    const int WhitespaceIndex = 24;
    const int PathStripIndex = 25;
    const int ContextIndex = 26;
    const int DirectoryIndex = 27; 
    const int BuildFakeAncestorIndex = 28;
    const int IgnoreSpaceChangeIndex = 29;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[30];

    private ApplyWhitespaceMode _whitespace;
    private int? _pathStrip;
    private int? _context;
    private string? _directory;

    /// <summary>Patch files to read.</summary>
    public IReadOnlyList<FilePath> Patches { get; init; } = Array.Empty<FilePath>();

    /// <summary>Outputs a diffstat instead of applying (--stat).</summary>
    public bool Stat
    {
        get => arguments[StatIndex].HasValue;
        init => SetFlag(StatIndex, value, ApplyConst.STAT);
    }

    /// <summary>Outputs a machine friendly diffstat instead of applying (--numstat).</summary>
    public bool NumStat
    {
        get => arguments[NumStatIndex].HasValue;
        init => SetFlag(NumStatIndex, value, ApplyConst.NUMSTAT);
    }

    /// <summary>Outputs a condensed summary instead of applying (--summary).</summary>
    public bool Summary
    {
        get => arguments[SummaryIndex].HasValue;
        init => SetFlag(SummaryIndex, value, ApplyConst.SUMMARY);
    }

    /// <summary>Only checks that the patch applies (--check).</summary>
    public bool Check
    {
        get => arguments[CheckIndex].HasValue;
        init => SetFlag(CheckIndex, value, ApplyConst.CHECK);
    }

    /// <summary>Applies to the index and working tree (--index).</summary>
    public bool Index
    {
        get => arguments[IndexIndex].HasValue;
        init => SetFlag(IndexIndex, value, ApplyConst.INDEX);
    }

    /// <summary>Applies to the index only (--cached).</summary>
    public bool Cached
    {
        get => arguments[CachedIndex].HasValue;
        init => SetFlag(CachedIndex, value, ApplyConst.CACHED);
    }

    /// <summary>Marks new files as intent-to-add (-N).</summary>
    public bool IntentToAdd
    {
        get => arguments[IntentToAddIndex].HasValue;
        init => SetFlag(IntentToAddIndex, value, ApplyConst.INTENT_TO_ADD);
    }

    /// <summary>Attempts a three-way merge (--3way).</summary>
    public bool ThreeWay
    {
        get => arguments[ThreeWayIndex].HasValue;
        init => SetFlag(ThreeWayIndex, value, ApplyConst.THREE_WAY);
    }

    /// <summary>Resolves conflicts favoring our side (--ours). Requires ThreeWay.</summary>
    public bool Ours
    {
        get => arguments[OursIndex].HasValue;
        init => SetFlag(OursIndex, value, ApplyConst.OURS);
    }

    /// <summary>Resolves conflicts favoring their side (--theirs). Requires ThreeWay.</summary>
    public bool Theirs
    {
        get => arguments[TheirsIndex].HasValue;
        init => SetFlag(TheirsIndex, value, ApplyConst.THEIRS);
    }

    /// <summary>Resolves conflicts keeping both sides (--union). Requires ThreeWay.</summary>
    public bool Union
    {
        get => arguments[UnionIndex].HasValue;
        init => SetFlag(UnionIndex, value, ApplyConst.UNION);
    }

    /// <summary>Applies even when an information option was given (--apply).</summary>
    public bool ApplyChanges
    {
        get => arguments[ApplyChangesIndex].HasValue;
        init => SetFlag(ApplyChangesIndex, value, ApplyConst.APPLY);
    }

    /// <summary>Ignores additions made by the patch (--no-add).</summary>
    public bool NoAdd
    {
        get => arguments[NoAddIndex].HasValue;
        init => SetFlag(NoAddIndex, value, ApplyConst.NO_ADD);
    }

    /// <summary>Applies the patch in reverse (-R).</summary>
    public bool Reverse
    {
        get => arguments[ReverseIndex].HasValue;
        init => SetFlag(ReverseIndex, value, ApplyConst.REVERSE);
    }

    /// <summary>Applies the applicable hunks and leaves rejected ones in .rej files (--reject).</summary>
    public bool Reject
    {
        get => arguments[RejectIndex].HasValue;
        init => SetFlag(RejectIndex, value, ApplyConst.REJECT);
    }

    /// <summary>Uses NUL-terminated output with numstat (-z).</summary>
    public bool NulTerminated
    {
        get => arguments[NulTerminatedIndex].HasValue;
        init => SetFlag(NulTerminatedIndex, value, ApplyConst.NUL_TERMINATED);
    }

    /// <summary>Allows patches without context lines (--unidiff-zero).</summary>
    public bool UnidiffZero
    {
        get => arguments[UnidiffZeroIndex].HasValue;
        init => SetFlag(UnidiffZeroIndex, value, ApplyConst.UNIDIFF_ZERO);
    }

    /// <summary>Ignores whitespace changes in context lines (--ignore-whitespace).</summary>
    public bool IgnoreWhitespace
    {
        get => arguments[IgnoreWhitespaceIndex].HasValue;
        init => SetFlag(IgnoreWhitespaceIndex, value, ApplyConst.IGNORE_WHITESPACE);
    }

    /// <summary>Works around diffs that miss a missing end-of-file newline (--inaccurate-eof).</summary>
    public bool InaccurateEof
    {
        get => arguments[InaccurateEofIndex].HasValue;
        init => SetFlag(InaccurateEofIndex, value, ApplyConst.INACCURATE_EOF);
    }

    /// <summary>Infers hunk line counts (--recount).</summary>
    public bool Recount
    {
        get => arguments[RecountIndex].HasValue;
        init => SetFlag(RecountIndex, value, ApplyConst.RECOUNT);
    }

    /// <summary>Reports progress (--verbose).</summary>
    public bool Verbose
    {
        get => arguments[VerboseIndex].HasValue;
        init => SetFlag(VerboseIndex, value, ApplyConst.VERBOSE);
    }

    /// <summary>Suppresses stderr output (--quiet).</summary>
    public bool Quiet
    {
        get => arguments[QuietIndex].HasValue;
        init => SetFlag(QuietIndex, value, ApplyConst.QUIET);
    }

    /// <summary>Allows paths outside the working area (--unsafe-paths).</summary>
    public bool UnsafePaths
    {
        get => arguments[UnsafePathsIndex].HasValue;
        init => SetFlag(UnsafePathsIndex, value, ApplyConst.UNSAFE_PATHS);
    }

    /// <summary>Does not fail for empty patches (--allow-empty).</summary>
    public bool AllowEmpty
    {
        get => arguments[AllowEmptyIndex].HasValue;
        init => SetFlag(AllowEmptyIndex, value, ApplyConst.ALLOW_EMPTY);
    }

    /// <summary>Whitespace error handling (--whitespace).</summary>
    public ApplyWhitespaceMode Whitespace
    {
        get => _whitespace;
        init
        {
            _whitespace = value;
            var argument = value.ToArgumentValue();
            arguments[WhitespaceIndex] = argument is string val
                ? Optional<Pair>.Some(new Pair(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, ApplyConst.WHITESPACE, val)))
                : Optional<Pair>.None;
        }
    }

    /// <summary>Leading path components to remove (-p).</summary>
    public int? PathStrip
    {
        get => _pathStrip;
        init
        {
            _pathStrip = value;
            arguments[PathStripIndex] = value is int val
                ? Optional<Pair>.Some(new Pair(string.Format(ApplyConst.FORMAT_PATH_STRIP, val)))
                : Optional<Pair>.None;
        }
    }

    /// <summary>Minimum matching context lines (-C).</summary>
    public int? Context
    {
        get => _context;
        init
        {
            _context = value;
            arguments[ContextIndex] = value is int val
                ? Optional<Pair>.Some(new Pair(string.Format(ApplyConst.FORMAT_CONTEXT, val)))
                : Optional<Pair>.None;
        }
    }

    /// <summary>Root prepended to all file names (--directory).</summary>
    public string? Directory
    {
        get => _directory;
        init
        {
            _directory = value;
            arguments[DirectoryIndex] = value is string val
                ? Optional<Pair>.Some(new Pair(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, ApplyConst.DIRECTORY, val)))
                : Optional<Pair>.None;
        }
    }
    // <summary>Builds a temporary index from the blobs recorded in the patch (--build-fake-ancestor).</summary>
    public FilePath? BuildFakeAncestor
    {
        get => _buildFakeAncestor;
        init
        {
            _buildFakeAncestor = value;
            arguments[BuildFakeAncestorIndex] = value is FilePath val
                ? Optional<Pair>.Some(new Pair(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, ApplyConst.BUILD_FAKE_ANCESTOR, val.ToString())))
                : Optional<Pair>.None;
        }
    }

    /// <summary>Ignores whitespace changes in context lines (--ignore-space-change). Synonym of IgnoreWhitespace.</summary>
    public bool IgnoreSpaceChange
    {
        get => arguments[IgnoreSpaceChangeIndex].HasValue;
        init => SetFlag(IgnoreSpaceChangeIndex, value, ApplyConst.IGNORE_SPACE_CHANGE);
    }
    /// <summary>Path patterns to exclude (--exclude).</summary>
    public IEnumerable<string> Excludes { get; init; } = Enumerable.Empty<string>();

    /// <summary>Path patterns to include (--include).</summary>
    public IEnumerable<string> Includes { get; init; } = Enumerable.Empty<string>();

    private void SetFlag(int index, bool enabled, string flag)
        => arguments[index] = enabled
            ? Optional<Pair>.Some(new Pair(flag))
            : Optional<Pair>.None;

    private FilePath? _buildFakeAncestor;


    // Apply: add the separator so patch paths starting with '-' are never parsed as options
    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder args)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (optional.HasValue)
                args.AddArgument(optional.Value.First!, true);
        }

        AddValues(args, ApplyConst.EXCLUDE, Excludes);
        AddValues(args, ApplyConst.INCLUDE, Includes);

        args.AddArgument(ApplyConst.ARGUMENT_SEPARATOR, true);

        for (var i = 0; i < Patches.Count; i++)
            args.AddArgument(Patches[i].ToString(), true);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = GetError();
        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(ApplyConst.GIT_APPLY, error));

        return Result<Nothing, IError>.Some(new());
    }

    private string? GetError()
    {
        if (Patches is null || Patches.Count == 0)
            return ApplyConst.Errors.PATCH_REQUIRED;

        for (var i = 0; i < Patches.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(Patches[i].ToString()))
                return ApplyConst.Errors.PATCH_INVALID;
        }

        var resolutions = (Ours ? 1 : 0) + (Theirs ? 1 : 0) + (Union ? 1 : 0);
        if (resolutions > 1)
            return ApplyConst.Errors.RESOLUTION_CONFLICT;
        if (resolutions == 1 && !ThreeWay)
            return ApplyConst.Errors.RESOLUTION_REQUIRES_THREE_WAY;
        if (ThreeWay && Reject)
            return ApplyConst.Errors.THREE_WAY_REJECT_CONFLICT;
        if (Index && Cached)
            return ApplyConst.Errors.TARGET_CONFLICT;
        if (PathStrip < 0 || Context < 0)
            return ApplyConst.Errors.NUMBER_INVALID;
        if (Directory is not null && string.IsNullOrWhiteSpace(Directory))
            return ApplyConst.Errors.VALUE_INVALID;
        if (BuildFakeAncestor is FilePath ancestor && string.IsNullOrWhiteSpace(ancestor.ToString()))
            return ApplyConst.Errors.VALUE_INVALID;

        return ValidateValues(Excludes) ?? ValidateValues(Includes);
    }

    private static string? ValidateValues(IEnumerable<string>? values)
    {
        if (values is null)
            return null;

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ApplyConst.Errors.VALUE_INVALID;
        }

        return null;
    }

    private static void AddValues(CliWrapArgumentBuilder args, string name, IEnumerable<string>? values)
    {
        if (values is null)
            return;

        foreach (var value in values)
            args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, name, value), true);
    }
}