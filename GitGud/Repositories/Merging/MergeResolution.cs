// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Merging;

public enum MergeResolutionKind
{
    /// <summary>The merged result is an existing file; it is copied over the working file.</summary>
    Path,

    /// <summary>The merged result is provided as text.</summary>
    Content,

    /// <summary>Keep the version from the current branch.</summary>
    Local,

    /// <summary>Take the version from the incoming branch.</summary>
    Incoming,

    /// <summary>Abort the whole merge.</summary>
    Abort,
}

/// <summary>The answer a resolver gives for one conflicted file.</summary>
public sealed class MergeResolution
{
    private MergeResolution(MergeResolutionKind kind, string? path, string? content)
    {
        Kind = kind;
        Path = path;
        Content = content;
    }

    public MergeResolutionKind Kind { get; }

    public string? Path { get; }

    public string? Content { get; }

    /// <summary>Use an existing file as the result. Preferred for large or binary files since it is copied, not loaded.</summary>
    public static MergeResolution FromPath(string path) => new(MergeResolutionKind.Path, path, null);

    /// <summary>Use text as the result, written as UTF-8 without a byte order mark.</summary>
    public static MergeResolution FromContent(string content) => new(MergeResolutionKind.Content, null, content);

    public static MergeResolution UseLocal() => new(MergeResolutionKind.Local, null, null);

    public static MergeResolution UseIncoming() => new(MergeResolutionKind.Incoming, null, null);

    public static MergeResolution Abort() => new(MergeResolutionKind.Abort, null, null);
}
