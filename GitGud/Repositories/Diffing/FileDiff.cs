// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Diffing;

public sealed class FileDiff
{
    public FileDiff(
        DiffFileStatus status,
        string? oldPath,
        string? newPath,
        string? oldMode,
        string? newMode,
        string? oldHash,
        string? newHash,
        int? similarity,
        bool isBinary,
        IReadOnlyList<DiffHunk> hunks)
    {
        Status = status;
        OldPath = oldPath;
        NewPath = newPath;
        OldMode = oldMode;
        NewMode = newMode;
        OldHash = oldHash;
        NewHash = newHash;
        Similarity = similarity;
        IsBinary = isBinary;
        Hunks = hunks;

        for (var i = 0; i < hunks.Count; i++)
        {
            Additions += hunks[i].Additions;
            Deletions += hunks[i].Deletions;
        }
    }

    public DiffFileStatus Status { get; }

    /// <summary>Path before the change; null for added files.</summary>
    public string? OldPath { get; }

    /// <summary>Path after the change; null for deleted files.</summary>
    public string? NewPath { get; }

    /// <summary>The path after the change, or the old path for deleted files.</summary>
    public string? Path => NewPath ?? OldPath;

    public string? OldMode { get; }

    public string? NewMode { get; }

    public string? OldHash { get; }

    public string? NewHash { get; }

    /// <summary>Similarity percentage for renames and copies.</summary>
    public int? Similarity { get; }

    public bool IsBinary { get; }

    public IReadOnlyList<DiffHunk> Hunks { get; }

    public int Additions { get; }

    public int Deletions { get; }
}
