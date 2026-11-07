// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Logging;

public enum LogOrder
{
    Default,
    Date,
    AuthorDate,
    Topological
}

public enum LogMergeFilter
{
    All,
    OnlyMerges,
    NoMerges
}

public enum LogPatternMode
{
    Basic,
    Extended,
    Fixed,
    Perl
}

public enum LogHistoryMode
{
    Default,
    FullHistory,
    SimplifyByDecoration
}

public enum FileChangeKind
{
    Unknown,
    Added,
    Copied,
    Deleted,
    Modified,
    Renamed,
    TypeChanged,
    Unmerged,
    Broken
}

public static class FileChangeKindExtensions
{
    public static FileChangeKind ToKind(this char code)
    => code switch
    {
        'A' => FileChangeKind.Added,
        'C' => FileChangeKind.Copied,
        'D' => FileChangeKind.Deleted,
        'M' => FileChangeKind.Modified,
        'R' => FileChangeKind.Renamed,
        'T' => FileChangeKind.TypeChanged,
        'U' => FileChangeKind.Unmerged,
        'B' => FileChangeKind.Broken,
        _ => FileChangeKind.Unknown
    };

    public static char ToCode(this FileChangeKind kind)
        => kind switch
        {
            FileChangeKind.Added => 'A',
            FileChangeKind.Copied => 'C',
            FileChangeKind.Deleted => 'D',
            FileChangeKind.Modified => 'M',
            FileChangeKind.Renamed => 'R',
            FileChangeKind.TypeChanged => 'T',
            FileChangeKind.Unmerged => 'U',
            FileChangeKind.Broken => 'B',
            _ => '\0'
        };

}
