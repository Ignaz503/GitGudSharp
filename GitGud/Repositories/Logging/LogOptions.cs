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

using System.Globalization;

using LogConst = GitGud.Constants.Const.Repository.Logging;

namespace GitGud.Repositories.Logging;

/// <summary>
/// Description of a <c>git log</c> call. Only the options that are explicitly set end up as arguments.
/// </summary>
public sealed class LogOptions : IOperationOptions
{
    const int PatchIndex = 0;
    const int FileChangesIndex = 1;
    const int MergeDiffsIndex = 2;

    const int MaxCountIndex = 3;
    const int SkipIndex = 4;
    const int MinParentsIndex = 5;
    const int MaxParentsIndex = 6;
    const int SinceIndex = 7;
    const int UntilIndex = 8;
    const int AuthorIndex = 9;
    const int CommitterIndex = 10;
    const int GrepIndex = 11;
    const int AllMatchIndex = 12;
    const int InvertGrepIndex = 13;
    const int IgnoreCaseIndex = 14;
    const int ExtendedRegexpIndex = 15;
    const int FixedStringsIndex = 16;
    const int PerlRegexpIndex = 17;
    const int OnlyMergesIndex = 18;
    const int NoMergesIndex = 19;
    const int FirstParentIndex = 20;
    const int AllIndex = 21;
    const int BranchesIndex = 22;
    const int TagsIndex = 23;
    const int RemotesIndex = 24;
    const int DateOrderIndex = 25;
    const int AuthorDateOrderIndex = 26;
    const int TopoOrderIndex = 27;
    const int ReverseIndex = 28;
    const int FullHistoryIndex = 29;
    const int SimplifyByDecorationIndex = 30;
    const int AncestryPathIndex = 31;
    const int PickaxeStringIndex = 32;
    const int PickaxeRegexIndex = 33;
    const int FollowIndex = 34;

    // Emitted after --not.
    const int NotBranchesIndex = 35;
    const int NotTagsIndex = 36;
    const int NotRemotesIndex = 37;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[38];
    private DateTimeOffset? _since;
    private DateTimeOffset? _until;

    /// <summary>Revisions or ranges (e.g. main, origin..HEAD, A...B, ^topic).</summary>
    public IReadOnlyList<Revision>? Revisions { get; init; }

    /// <summary>Excludes the history of these revisions (--not rev).</summary>
    public IReadOnlyList<Revision>? NotRevisions { get; init; }

    /// <summary>Limits history to commits touching these paths.</summary>
    public IReadOnlyList<FilePath>? Paths { get; init; }

    /// <summary>Line or function history (-L). Implies a patch and cannot be combined with paths.</summary>
    public IReadOnlyList<(LineRange Range, FilePath Path)>? LineRanges { get; init; }

    /// <summary>Only commits with these kinds of file changes (--diff-filter).</summary>
    public IReadOnlyList<FileChangeKind>? DiffFilter { get; init; }

    /// <summary>True when the output contains diff text (-p or -L).</summary>
    internal bool HasPatch => Patch || HasLineRanges;

    private bool HasLineRanges => LineRanges is not null && LineRanges.Count > 0;

    /// <summary>Populates <see cref="LogEntry.Patch"/> (-p).</summary>
    public bool Patch
    {
        get => Has(PatchIndex);
        init => SetFlag(PatchIndex, LogConst.PATCH, value);
    }

    /// <summary>Populates <see cref="LogEntry.Files"/> (--name-status).</summary>
    public bool FileChanges
    {
        get => Has(FileChangesIndex);
        init => SetFlag(FileChangesIndex, LogConst.NAME_STATUS, value);
    }

    /// <summary>Shows full diffs for merge commits (-m). Requires a patch.</summary>
    public bool MergeDiffs
    {
        get => Has(MergeDiffsIndex);
        init => SetFlag(MergeDiffsIndex, LogConst.MERGE_DIFFS, value);
    }

    public int? MaxCount
    {
        get => GetNumber(MaxCountIndex);
        init => SetNumber(MaxCountIndex, LogConst.MAX_COUNT, value);
    }

    public int? Skip
    {
        get => GetNumber(SkipIndex);
        init => SetNumber(SkipIndex, LogConst.SKIP, value);
    }

    public int? MinParents
    {
        get => GetNumber(MinParentsIndex);
        init => SetNumber(MinParentsIndex, LogConst.MIN_PARENTS, value);
    }

    public int? MaxParents
    {
        get => GetNumber(MaxParentsIndex);
        init => SetNumber(MaxParentsIndex, LogConst.MAX_PARENTS, value);
    }

    public DateTimeOffset? Since
    {
        get => _since;
        init
        {
            _since = value;
            SetValue(SinceIndex, LogConst.SINCE, FormatDate(value));
        }
    }

    public DateTimeOffset? Until
    {
        get => _until;
        init
        {
            _until = value;
            SetValue(UntilIndex, LogConst.UNTIL, FormatDate(value));
        }
    }

    public RegexPattern? Author
    {
        get => GetRegex(AuthorIndex);
        init => SetRegex(AuthorIndex, LogConst.AUTHOR, value);
    }

    public RegexPattern? Committer
    {
        get => GetRegex(CommitterIndex);
        init => SetRegex(CommitterIndex, LogConst.COMMITTER, value);
    }

    /// <summary>Message pattern (--grep).</summary>
    public RegexPattern? Grep
    {
        get => GetRegex(GrepIndex);
        init => SetRegex(GrepIndex, LogConst.GREP, value);
    }

    public bool AllMatch { get => Has(AllMatchIndex); init => SetFlag(AllMatchIndex, LogConst.ALL_MATCH, value); }

    public bool InvertGrep { get => Has(InvertGrepIndex); init => SetFlag(InvertGrepIndex, LogConst.INVERT_GREP, value); }

    public bool IgnoreCase { get => Has(IgnoreCaseIndex); init => SetFlag(IgnoreCaseIndex, LogConst.IGNORE_CASE, value); }

    public bool ExtendedRegexp { get => Has(ExtendedRegexpIndex); init => SetFlag(ExtendedRegexpIndex, LogConst.EXTENDED_REGEXP, value); }

    public bool FixedStrings { get => Has(FixedStringsIndex); init => SetFlag(FixedStringsIndex, LogConst.FIXED_STRINGS, value); }

    public bool PerlRegexp { get => Has(PerlRegexpIndex); init => SetFlag(PerlRegexpIndex, LogConst.PERL_REGEXP, value); }

    public bool OnlyMerges { get => Has(OnlyMergesIndex); init => SetFlag(OnlyMergesIndex, LogConst.MERGES, value); }

    public bool NoMerges { get => Has(NoMergesIndex); init => SetFlag(NoMergesIndex, LogConst.NO_MERGES, value); }

    public bool FirstParent { get => Has(FirstParentIndex); init => SetFlag(FirstParentIndex, LogConst.FIRST_PARENT, value); }

    /// <summary>All refs (--all).</summary>
    public bool All { get => Has(AllIndex); init => SetFlag(AllIndex, LogConst.ALL, value); }

    /// <summary>All branches (--branches). Combine with <see cref="BranchesPattern"/> to limit them.</summary>
    public bool Branches { get => Has(BranchesIndex); init => SetRefFlag(BranchesIndex, LogConst.BRANCHES, value); }

    public GlobPattern? BranchesPattern { init => SetRefPattern(BranchesIndex, LogConst.BRANCHES, value); }

    /// <summary>All tags (--tags). Combine with <see cref="TagsPattern"/> to limit them.</summary>
    public bool Tags { get => Has(TagsIndex); init => SetRefFlag(TagsIndex, LogConst.TAGS, value); }

    public GlobPattern? TagsPattern { init => SetRefPattern(TagsIndex, LogConst.TAGS, value); }

    /// <summary>All remote branches (--remotes). Combine with <see cref="RemotesPattern"/> (e.g. origin or */master).</summary>
    public bool Remotes { get => Has(RemotesIndex); init => SetRefFlag(RemotesIndex, LogConst.REMOTES, value); }

    public GlobPattern? RemotesPattern { init => SetRefPattern(RemotesIndex, LogConst.REMOTES, value); }

    public bool NotBranches { get => Has(NotBranchesIndex); init => SetRefFlag(NotBranchesIndex, LogConst.BRANCHES, value); }

    public GlobPattern? NotBranchesPattern { init => SetRefPattern(NotBranchesIndex, LogConst.BRANCHES, value); }

    public bool NotTags { get => Has(NotTagsIndex); init => SetRefFlag(NotTagsIndex, LogConst.TAGS, value); }

    public GlobPattern? NotTagsPattern { init => SetRefPattern(NotTagsIndex, LogConst.TAGS, value); }

    public bool NotRemotes { get => Has(NotRemotesIndex); init => SetRefFlag(NotRemotesIndex, LogConst.REMOTES, value); }

    public GlobPattern? NotRemotesPattern { init => SetRefPattern(NotRemotesIndex, LogConst.REMOTES, value); }

    public bool DateOrder { get => Has(DateOrderIndex); init => SetFlag(DateOrderIndex, LogConst.DATE_ORDER, value); }

    public bool AuthorDateOrder { get => Has(AuthorDateOrderIndex); init => SetFlag(AuthorDateOrderIndex, LogConst.AUTHOR_DATE_ORDER, value); }

    public bool TopoOrder { get => Has(TopoOrderIndex); init => SetFlag(TopoOrderIndex, LogConst.TOPO_ORDER, value); }

    public bool Reverse { get => Has(ReverseIndex); init => SetFlag(ReverseIndex, LogConst.REVERSE, value); }

    public bool FullHistory { get => Has(FullHistoryIndex); init => SetFlag(FullHistoryIndex, LogConst.FULL_HISTORY, value); }

    public bool SimplifyByDecoration { get => Has(SimplifyByDecorationIndex); init => SetFlag(SimplifyByDecorationIndex, LogConst.SIMPLIFY_BY_DECORATION, value); }

    public bool AncestryPath { get => Has(AncestryPathIndex); init => SetFlag(AncestryPathIndex, LogConst.ANCESTRY_PATH, value); }

    /// <summary>Only commits that add or remove this string (-S).</summary>
    public string? PickaxeString
    {
        get => GetValue(PickaxeStringIndex);
        init => SetValue(PickaxeStringIndex, LogConst.PICKAXE_STRING, value);
    }

    /// <summary>Only commits whose diff matches this regex (-G).</summary>
    public RegexPattern? PickaxeRegex
    {
        get => GetRegex(PickaxeRegexIndex);
        init => SetRegex(PickaxeRegexIndex, LogConst.PICKAXE_REGEX, value);
    }

    /// <summary>Follows renames; requires exactly one path.</summary>
    public bool Follow
    {
        get => Has(FollowIndex);
        init => SetFlag(FollowIndex, LogConst.FOLLOW, value);
    }

    private bool Has(int index)
        => arguments[index].HasValue;

    private string? GetValue(int index)
        => arguments[index].Transform(static v => v.Second, static () => default!);

    private void SetFlag(int index, string name, bool value)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(name))
            : Optional<Pair>.None;

    private void SetValue(int index, string name, string? value)
        => arguments[index] = value is string text
            ? Optional<Pair>.Some(new Pair(name, text))
            : Optional<Pair>.None;

    private void SetNumber(int index, string name, int? value)
        => SetValue(index, name, value?.ToString(CultureInfo.InvariantCulture));

    private int? GetNumber(int index)
        => GetValue(index) is string text
           && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : default(int?);

    private void SetRegex(int index, string name, RegexPattern? value)
        => SetValue(index, name, value is RegexPattern pattern ? pattern.ToString() ?? string.Empty : null);

    private RegexPattern? GetRegex(int index)
        => GetValue(index) is string text ? new RegexPattern(text) : default(RegexPattern?);

    private void SetRefFlag(int index, string name, bool value)
    {
        if (!value)
            arguments[index] = Optional<Pair>.None;
        else if (!arguments[index].HasValue)
            arguments[index] = Optional<Pair>.Some(new Pair(name));
    }

    private void SetRefPattern(int index, string name, GlobPattern? value)
    {
        if (value is GlobPattern pattern)
            arguments[index] = Optional<Pair>.Some(new Pair(name, pattern.ToString()));
    }

    private static string? FormatDate(DateTimeOffset? date)
        => date?.ToString(LogConst.DATE_FORMAT, CultureInfo.InvariantCulture);

    /// <summary>Writes the complete <c>git log</c> command line.</summary>
    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(LogConst.CONFIG_OPTION, true);
        argumentBuilder.AddArgument(LogConst.QUOTE_PATH_OFF, true);
        argumentBuilder.AddArgument(Const.LOG, true);
        argumentBuilder.Flag(LogConst.NO_COLOR);
        argumentBuilder.Flag(LogConst.NO_EXT_DIFF);
        argumentBuilder.AddArgument(
            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, LogConst.FORMAT, "format:" + LogConst.LOG_FORMAT), true);
        argumentBuilder.Flag(LogConst.DECORATE_FULL);

        if (!HasLineRanges)
        {
            if (Has(PatchIndex))
                argumentBuilder.Flag(LogConst.PATCH);
            else if (Has(FileChangesIndex))
                argumentBuilder.Flag(LogConst.NAME_STATUS);
            else
                argumentBuilder.Flag(LogConst.NO_PATCH);
        }

        if (Has(MergeDiffsIndex))
            argumentBuilder.AddArgument(LogConst.MERGE_DIFFS, true);

        for (var i = MaxCountIndex; i < NotBranchesIndex; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (!pair.TwoValues)
                argumentBuilder.Flag(pair.First!);
            else if (i == PickaxeStringIndex || i == PickaxeRegexIndex)
                argumentBuilder.AddArgument(pair.First + pair.Second, true);
            else
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
        }

        var kinds = DiffFilter;
        if (kinds is not null && kinds.Count > 0)
        {
            var codes = new char[kinds.Count];
            for (var i = 0; i < kinds.Count; i++)
                codes[i] = kinds[i].ToCode();

            argumentBuilder.AddArgument(
                string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, LogConst.DIFF_FILTER, new string(codes)), true);
        }

        var lines = LineRanges;
        if (lines is not null)
        {
            for (var i = 0; i < lines.Count; i++)
                argumentBuilder.AddArgument(string.Format(LogConst.LINE_RANGE_FORMAT, lines[i].Range, lines[i].Path), true);
        }

        AddAll(argumentBuilder, Revisions);

        var not = NotRevisions;
        if ((not is not null && not.Count > 0) || HasNotRef())
        {
            argumentBuilder.AddArgument(LogConst.NOT, true);
            AddAll(argumentBuilder, not);

            for (var i = NotBranchesIndex; i <= NotRemotesIndex; i++)
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
        }

        argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
        AddAll(argumentBuilder, Paths);
    }

    private bool HasNotRef()
        => Has(NotBranchesIndex) || Has(NotTagsIndex) || Has(NotRemotesIndex);

    private static void AddAll<T>(CliWrapArgumentBuilder argumentBuilder, IReadOnlyList<T>? values)
    {
        if (values is null)
            return;

        for (var i = 0; i < values.Count; i++)
            argumentBuilder.AddArgument(values[i]!.ToString(), true);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = ValidateNumbers()
            ?? ValidateBlankValues()
            ?? ValidateValues(Revisions, true)
            ?? ValidateValues(NotRevisions, true)
            ?? ValidateValues(Paths, false)
            ?? ValidatePickaxe()
            ?? ValidateDiffFilter()
            ?? ValidateLineRanges()
            ?? ValidateCombinations();

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_LOG, error));

        return Result<Nothing, IError>.Some(new());
    }

    private string? ValidateNumbers()
    {
        for (var i = MaxCountIndex; i <= MaxParentsIndex; i++)
        {
            if (GetNumber(i) is int number && number < 0)
                return LogConst.Errors.NUMBER_INVALID;
        }

        return null;
    }

    private string? ValidateBlankValues()
        => HasBlankValue(AuthorIndex, GrepIndex)
            || HasBlankValue(BranchesIndex, RemotesIndex)
            || HasBlankValue(NotBranchesIndex, NotRemotesIndex)
            ? LogConst.Errors.VALUE_INVALID
            : null;

    private bool HasBlankValue(int first, int last)
    {
        for (var i = first; i <= last; i++)
        {
            if (GetValue(i) is string text && string.IsNullOrWhiteSpace(text))
                return true;
        }

        return false;
    }

    private string? ValidatePickaxe()
        => (GetValue(PickaxeStringIndex) is string text && text.Length == 0)
            || (GetValue(PickaxeRegexIndex) is string regex && regex.Length == 0)
            ? LogConst.Errors.VALUE_INVALID
            : null;

    private string? ValidateDiffFilter()
    {
        var kinds = DiffFilter;
        if (kinds is null)
            return null;

        if (kinds.Count == 0)
            return LogConst.Errors.DIFF_FILTER_INVALID;

        for (var i = 0; i < kinds.Count; i++)
        {
            if (kinds[i] == FileChangeKind.Unknown)
                return LogConst.Errors.DIFF_FILTER_INVALID;
        }

        return null;
    }

    private string? ValidateLineRanges()
    {
        var lines = LineRanges;
        if (lines is null)
            return null;

        for (var i = 0; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i].Range) || string.IsNullOrWhiteSpace(lines[i].Path))
                return LogConst.Errors.LINE_RANGE_INVALID;
        }

        return null;
    }

    private string? ValidateCombinations()
    {
        var pathCount = Paths?.Count ?? 0;

        if (Follow && pathCount != 1)
            return LogConst.Errors.FOLLOW_SINGLE_PATH;
        if (HasLineRanges && pathCount > 0)
            return LogConst.Errors.LINE_RANGE_INVALID;
        if (Patch && FileChanges)
            return LogConst.Errors.PATCH_CONFLICT;
        if (MergeDiffs && !HasPatch)
            return LogConst.Errors.MERGE_DIFFS_REQUIRES_PATCH;

        return null;
    }

    private static string? ValidateValues<T>(IReadOnlyList<T>? values, bool rejectDash)
    {
        if (values is null)
            return null;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i]!.ToString();
            if (string.IsNullOrWhiteSpace(value) || (rejectDash && value[0] == Const.Repository.Branching.OPTION_PREFIX))
                return LogConst.Errors.VALUE_INVALID;
        }

        return null;
    }
}