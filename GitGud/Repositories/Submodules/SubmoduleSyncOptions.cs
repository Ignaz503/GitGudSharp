// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule sync</summary>
public sealed class SubmoduleSyncOptions : SubmodulePathOptionsBase
{
    const int RecursiveIndex = 1;

    public SubmoduleSyncOptions() : base(2) { }

    public SubmoduleSyncOptions(int size) : base(size)
    {
    }

    private protected override string SubCommand => SubConst.SYNC;

    /// <summary>Recurses into nested submodules (--recursive).</summary>
    public bool Recursive
    {
        get => arguments[RecursiveIndex].HasValue;
        init => SetFlag(RecursiveIndex, value, SubConst.RECURSIVE);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, RecursiveIndex, arguments.Length);
        AddPaths(argumentBuilder, Paths);
    }

    private protected override string? ValidateBody() => ValidatePaths(Paths);
}
