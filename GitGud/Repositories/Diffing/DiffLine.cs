// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Diffing;

public readonly struct DiffLine
{
    public DiffLine(DiffLineKind kind, string content, int? oldLineNumber, int? newLineNumber, bool noNewlineAtEnd = false)
    {
        Kind = kind;
        Content = content;
        OldLineNumber = oldLineNumber;
        NewLineNumber = newLineNumber;
        NoNewlineAtEnd = noNewlineAtEnd;
    }

    public DiffLineKind Kind { get; }

    /// <summary>The line text without the leading diff marker.</summary>
    public string Content { get; }

    /// <summary>Line number in the old file; null for added lines.</summary>
    public int? OldLineNumber { get; }

    /// <summary>Line number in the new file; null for removed lines.</summary>
    public int? NewLineNumber { get; }

    /// <summary>True when the file has no trailing newline after this line.</summary>
    public bool NoNewlineAtEnd { get; }

    internal DiffLine WithNoNewlineAtEnd()
        => new DiffLine(Kind, Content, OldLineNumber, NewLineNumber, true);
}
