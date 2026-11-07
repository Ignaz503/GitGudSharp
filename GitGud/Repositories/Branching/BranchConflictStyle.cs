// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Branching;

public enum BranchConflictStyle
{
    /// <summary>Uses merge.conflictStyle.</summary>
    Default,

    Merge,

    Diff3,

    ZDiff3
}
