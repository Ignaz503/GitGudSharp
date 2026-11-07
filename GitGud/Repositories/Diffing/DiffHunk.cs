// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Diffing;

public sealed class DiffHunk
{
    public DiffHunk(int oldStart, int oldCount, int newStart, int newCount, string? section, IReadOnlyList<DiffLine> lines, int additions, int deletions)
    {
        OldStart = oldStart;
        OldCount = oldCount;
        NewStart = newStart;
        NewCount = newCount;
        Section = section;
        Lines = lines;
        Additions = additions;
        Deletions = deletions;
    }

    public int OldStart { get; }

    public int OldCount { get; }

    public int NewStart { get; }

    public int NewCount { get; }

    /// <summary>Text git shows after the hunk header, typically the enclosing function.</summary>
    public string? Section { get; }

    public IReadOnlyList<DiffLine> Lines { get; }

    public int Additions { get; }

    public int Deletions { get; }
}
