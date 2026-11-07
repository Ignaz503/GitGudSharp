// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Logging;

public sealed class LogEntry
{
    public LogEntry(string id, IReadOnlyList<string> parents, GitSignature author, GitSignature committer, IReadOnlyList<RefDecoration> decorations, string subject, string body, IReadOnlyList<LogFileChange> files, string patch = "")
    {
        Patch = patch;
        Id = id;
        Parents = parents;
        Author = author;
        Committer = committer;
        Decorations = decorations;
        Subject = subject;
        Body = body;
        Files = files;
    }

    public string Id { get; }

    public IReadOnlyList<string> Parents { get; }

    public bool IsMerge => Parents.Count > 1;

    public GitSignature Author { get; }

    public GitSignature Committer { get; }

    public IReadOnlyList<RefDecoration> Decorations { get; }

    public string Subject { get; }

    public string Body { get; }

    /// <summary>Changed files. Empty unless <see cref="LogOptions.WithFileChanges"/> is set.</summary>
    public IReadOnlyList<LogFileChange> Files { get; }

    /// <summary>Unified diff text. Empty unless a patch or line range was requested.</summary>
    public string Patch { get; }
}
