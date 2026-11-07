// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Merging;

public enum MergeStrategy
{
    /// <summary>Fast-forward when possible, otherwise create a merge commit.</summary>
    Default,

    /// <summary>Always create a merge commit.</summary>
    NoFastForward,

    /// <summary>Fail unless the merge can fast-forward.</summary>
    FastForwardOnly,

    /// <summary>Stage the combined changes without committing or recording a merge.</summary>
    Squash,
}
