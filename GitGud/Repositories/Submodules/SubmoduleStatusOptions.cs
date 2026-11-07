// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule status</summary>
public sealed class SubmoduleStatusOptions : SubmodulePathOptionsBase
{
    const int CachedIndex = 1;
    const int RecursiveIndex = 2;

    public SubmoduleStatusOptions() : base(3) { }

    public SubmoduleStatusOptions(int size) : base(size)
    {
    }

    private protected override string SubCommand => SubConst.STATUS;

    /// <summary>Prints the commit recorded in the superproject (--cached).</summary>
    public bool Cached
    {
        get => arguments[CachedIndex].HasValue;
        init => SetFlag(CachedIndex, value, SubConst.CACHED);
    }

    /// <summary>Recurses into nested submodules (--recursive).</summary>
    public bool Recursive
    {
        get => arguments[RecursiveIndex].HasValue;
        init => SetFlag(RecursiveIndex, value, SubConst.RECURSIVE);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, CachedIndex, arguments.Length);
        AddPaths(argumentBuilder, Paths);
    }

    private protected override string? ValidateBody() => ValidatePaths(Paths);
}
