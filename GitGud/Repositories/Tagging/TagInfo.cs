// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Tagging;

public sealed class TagInfo
{
    public TagInfo(Tag name, TagKind kind, string objectId, string targetId, string subject)
    {
        Name = name;
        Kind = kind;
        ObjectId = objectId;
        TargetId = targetId;
        Subject = subject;
    }

    public Tag Name { get; }

    public TagKind Kind { get; }

    /// <summary>Object the tag ref points at (the tag object for annotated tags).</summary>
    public string ObjectId { get; }

    /// <summary>Object the tag ultimately refers to (the commit for annotated tags).</summary>
    public string TargetId { get; }

    /// <summary>First line of the tag message (annotated) or commit message (lightweight).</summary>
    public string Subject { get; }
}
