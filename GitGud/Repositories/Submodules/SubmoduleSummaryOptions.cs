// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule summary</summary>
public sealed class SubmoduleSummaryOptions : SubmodulePathOptionsBase
{
    const int CachedIndex = 1;
    const int FilesIndex = 2;
    const int LimitIndex = 3;
    const int CommitIndex = 4;

    public SubmoduleSummaryOptions() : base(5) { }

    private protected override string SubCommand => SubConst.SUMMARY;

    /// <summary>Compares against the index (--cached).</summary>
    public bool Cached
    {
        get => arguments[CachedIndex].HasValue;
        init => SetFlag(CachedIndex, value, SubConst.CACHED);
    }

    /// <summary>Compares the index with the submodule work tree (--files).</summary>
    public bool Files
    {
        get => arguments[FilesIndex].HasValue;
        init => SetFlag(FilesIndex, value, SubConst.FILES);
    }

    /// <summary>Limits the number of commits shown (--summary-limit).</summary>
    public int? SummaryLimit
    {
        get => GetNumber(LimitIndex);
        init => SetNumber(LimitIndex, value, SubConst.SUMMARY_LIMIT);
    }

    /// <summary>Superproject commit to compare with. Defaults to HEAD.</summary>
    public string? Commit
    {
        get => GetValue(CommitIndex);
        init => SetValue(CommitIndex, value);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, CachedIndex, arguments.Length);
        AddPaths(argumentBuilder, Paths);
    }

    private protected override string? ValidateBody()
    {
        if (Cached && Files)
            return SubConst.Errors.MODE_CONFLICT;
        if (Files && arguments[CommitIndex].HasValue)
            return SubConst.Errors.MODE_CONFLICT;
        if (arguments[CommitIndex].HasValue && IsInvalid(Commit))
            return SubConst.Errors.VALUE_INVALID;
        if (SummaryLimit is int limit && limit <= 0)
            return SubConst.Errors.NUMBER_INVALID;

        return ValidatePaths(Paths);
    }
}
