// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Merging;

/// <summary>
/// A conflicted file. Version files are temporary and deleted once the resolver returns, so copy them if they are needed later.
/// A version is <see langword="null"/> when that side does not have the file (added or deleted on one side).
/// </summary>
public sealed class MergeConflict
{
    public MergeConflict(string path, string workingFilePath, string? basePath, string? localPath, string? incomingPath)
    {
        Path = path;
        WorkingFilePath = workingFilePath;
        BasePath = basePath;
        LocalPath = localPath;
        IncomingPath = incomingPath;
    }

    /// <summary>Path relative to the repository root.</summary>
    public string Path { get; }

    /// <summary>Full path of the file in the working tree, containing conflict markers.</summary>
    public string WorkingFilePath { get; }

    /// <summary>Common ancestor version.</summary>
    public string? BasePath { get; }

    /// <summary>Version from the current branch.</summary>
    public string? LocalPath { get; }

    /// <summary>Version from the branch being merged in.</summary>
    public string? IncomingPath { get; }
}
