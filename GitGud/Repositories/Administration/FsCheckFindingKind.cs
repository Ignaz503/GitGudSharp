// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Administration;

/// <summary>Category of a finding reported by <c>git fsck</c>.</summary>
public enum FsCheckFindingKind
{
    /// <summary>Object that exists but is never directly used.</summary>
    Dangling,
    /// <summary>Object that exists but is not reachable from any reference.</summary>
    Unreachable,
    /// <summary>Root commit (--root).</summary>
    Root,
    /// <summary>Tag pointing to an object (--tags).</summary>
    Tagged,
    /// <summary>Referenced object that is missing.</summary>
    Missing,
    /// <summary>Link from one object to another that is missing.</summary>
    BrokenLink,
    /// <summary>Corrupt or invalid object.</summary>
    Error,
    /// <summary>Questionable object that is not fatal.</summary>
    Warning,
}
