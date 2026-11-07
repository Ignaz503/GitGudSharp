// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Utility;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule update</summary>
public sealed class SubmoduleUpdateOptions : SubmodulePathOptionsBase
{
    const int InitIndex = 1;
    const int RemoteIndex = 2;
    const int NoFetchIndex = 3;
    const int ForceIndex = 4;
    const int ModeIndex = 5;
    const int ReferenceIndex = 6;
    const int DepthIndex = 7;
    const int RecursiveIndex = 8;
    const int JobsIndex = 9;

    public SubmoduleUpdateOptions() : base(10) { }

    private protected override string SubCommand => SubConst.UPDATE;

    /// <summary>Initializes uninitialized submodules (--init).</summary>
    public bool Init
    {
        get => arguments[InitIndex].HasValue;
        init => SetFlag(InitIndex, value, SubConst.INIT);
    }

    /// <summary>Uses the remote tracking branch status (--remote).</summary>
    public bool Remote
    {
        get => arguments[RemoteIndex].HasValue;
        init => SetFlag(RemoteIndex, value, SubConst.REMOTE);
    }

    /// <summary>Does not fetch new objects (--no-fetch).</summary>
    public bool NoFetch
    {
        get => arguments[NoFetchIndex].HasValue;
        init => SetFlag(NoFetchIndex, value, SubConst.NO_FETCH);
    }

    /// <summary>Forces checkout (--force).</summary>
    public bool Force
    {
        get => arguments[ForceIndex].HasValue;
        init => SetFlag(ForceIndex, value, SubConst.FORCE);
    }

    /// <summary>Update procedure (--checkout, --rebase or --merge).</summary>
    public SubmoduleUpdateMode Mode
    {
        get => arguments[ModeIndex].Transform(
                static v => v.First?.GetSubmoduleUpdateMode() ?? SubmoduleUpdateMode.Default, 
                static () => SubmoduleUpdateMode.Default);
        init => arguments[ModeIndex] = value switch
        {
            SubmoduleUpdateMode.Checkout => Optional<Pair>.Some(new Pair(SubConst.CHECKOUT)),
            SubmoduleUpdateMode.Rebase => Optional<Pair>.Some(new Pair(SubConst.REBASE)),
            SubmoduleUpdateMode.Merge => Optional<Pair>.Some(new Pair(SubConst.MERGE)),
            _ => Optional<Pair>.None
        };
    }

    /// <summary>Reference repository (--reference).</summary>
    public string? Reference
    {
        get => GetValue(ReferenceIndex);
        init => SetValue(ReferenceIndex, value, SubConst.REFERENCE);
    }

    /// <summary>Shallow clone depth (--depth).</summary>
    public int? Depth
    {
        get => GetNumber(DepthIndex);
        init => SetNumber(DepthIndex, value, SubConst.DEPTH);
    }

    /// <summary>Recurses into nested submodules (--recursive).</summary>
    public bool Recursive
    {
        get => arguments[RecursiveIndex].HasValue;
        init => SetFlag(RecursiveIndex, value, SubConst.RECURSIVE);
    }

    /// <summary>Number of parallel jobs (--jobs).</summary>
    public int? Jobs
    {
        get => GetNumber(JobsIndex);
        init => SetNumber(JobsIndex, value, SubConst.JOBS);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, InitIndex, arguments.Length);
        AddPaths(argumentBuilder, Paths);
    }

    private protected override string? ValidateBody()
    {
        if (arguments[ReferenceIndex].HasValue && string.IsNullOrWhiteSpace(Reference))
            return SubConst.Errors.VALUE_INVALID;
        if ((Depth is int depth && depth <= 0) || (Jobs is int jobs && jobs <= 0))
            return SubConst.Errors.NUMBER_INVALID;

        return ValidatePaths(Paths);
    }
}
