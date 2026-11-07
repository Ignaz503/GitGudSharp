// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories;

/// <summary>A ref pointing at a logged commit. Switch on the concrete type.</summary>
public abstract class RefDecoration
{
    protected RefDecoration(string name)
        => Name = name;

    /// <summary>Short name of the ref.</summary>
    public string Name { get; }
}

/// <summary>HEAD, optionally attached to a local branch.</summary>
public sealed class HeadDecoration : RefDecoration
{
    public HeadDecoration(string name, Branch? target)
        : base(name)
        => Target = target;

    public Branch? Target { get; }
}

public sealed class LocalBranchDecoration : RefDecoration
{
    public LocalBranchDecoration(Branch branch)
        : base(branch)
        => Branch = branch;

    public Branch Branch { get; }
}

public sealed class RemoteBranchDecoration : RefDecoration
{
    public RemoteBranchDecoration(Branch branch, string remote)
        : base(branch)
    {
        Branch = branch;
        Remote = remote;
    }

    /// <summary>Remote-tracking name including the remote, e.g. origin/main.</summary>
    public Branch Branch { get; }

    public string Remote { get; }
}

public sealed class TagDecoration : RefDecoration
{
    public TagDecoration(Tag tag)
        : base(tag)
        => Tag = tag;

    public Tag Tag { get; }
}

/// <summary>Any other ref, e.g. refs/stash.</summary>
public sealed class OtherDecoration : RefDecoration
{
    public OtherDecoration(string name)
        : base(name)
    {
    }
}
