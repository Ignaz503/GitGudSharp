// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Branching;

public enum BranchTrackMode
{
    /// <summary>Lets git decide based on branch.autoSetupMerge.</summary>
    Default,

    /// <summary>Uses the start point itself as the upstream (--track=direct).</summary>
    Direct,

    /// <summary>Copies the upstream configuration of the start point (--track=inherit).</summary>
    Inherit,

    /// <summary>Does not set up upstream tracking (--no-track).</summary>
    None
}
