// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Diffing;

public sealed class DiffResult
{
    public DiffResult(IReadOnlyList<FileDiff> files)
    {
        Files = files;
        for (var i = 0; i < files.Count; i++)
        {
            TotalAdditions += files[i].Additions;
            TotalDeletions += files[i].Deletions;
        }
    }

    public IReadOnlyList<FileDiff> Files { get; }

    public bool HasChanges => Files.Count > 0;

    public int TotalAdditions { get; }

    public int TotalDeletions { get; }
}
