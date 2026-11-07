// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Logging;

/// <summary>A file touched by a commit.</summary>
public class LogFileChange
{
    public LogFileChange(string path, FileChangeKind kind)
    {
        Path = path;
        Kind = kind;
    }

    public string Path { get; }

    public FileChangeKind Kind { get; }
}

/// <summary>A renamed or copied file.</summary>
public sealed class RenamedFileChange : LogFileChange
{
    public RenamedFileChange(string path, FileChangeKind kind, string oldPath, int similarity)
        : base(path, kind)
    {
        OldPath = oldPath;
        Similarity = similarity;
    }

    public string OldPath { get; }

    /// <summary>Similarity percentage (0-100).</summary>
    public int Similarity { get; }
}
