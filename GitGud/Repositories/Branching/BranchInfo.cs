// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Branching;

public sealed class BranchInfo
{
    public BranchInfo(Branch name, Remote? remote, bool isCurrent, string commit, string? upstream)
    {
        Name = name;
        Remote = remote;
        IsCurrent = isCurrent;
        Commit = commit;
        Upstream = upstream;
    }

    /// <summary>Branch name without remote prefix.</summary>
    public Branch Name { get; }

    /// <summary>Set for remote-tracking branches.</summary>
    public Remote? Remote { get; }

    public bool IsRemote => Remote is not null;

    public bool IsCurrent { get; }

    public string Commit { get; }

    public string? Upstream { get; }
}
