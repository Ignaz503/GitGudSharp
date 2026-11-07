// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Branching;

public enum BranchCreateMode
{
    /// <summary>Switches to an existing branch or commit.</summary>
    None,

    /// <summary>Creates the new branch and switches to it (-b).</summary>
    Create,

    /// <summary>Creates the new branch, or resets it to the start point if it exists, and switches to it (-B).</summary>
    Reset,

    /// <summary>Creates a new unborn branch with no history and switches to it (--orphan).</summary>
    Orphan
}
