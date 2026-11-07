// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule set-branch</summary>
public sealed class SubmoduleSetBranchOptions : SubmoduleOptionsBase
{
    const int BranchIndex = 1;
    const int DefaultIndex = 2;
    const int PathIndex = 3;

    public SubmoduleSetBranchOptions() : base(4) { }

    private protected override string SubCommand => SubConst.SET_BRANCH;

    /// <summary>Submodule path (required).</summary>
    public string? Path
    {
        get => GetValue(PathIndex);
        init => SetValue(PathIndex, value);
    }

    /// <summary>Remote branch to track (--branch).</summary>
    public string? Branch
    {
        get => GetValue(BranchIndex);
        init => SetValue(BranchIndex, value, SubConst.BRANCH);
    }

    /// <summary>Removes the branch setting, tracking remote HEAD (--default).</summary>
    public bool Default
    {
        get => arguments[DefaultIndex].HasValue;
        init => SetFlag(DefaultIndex, value, SubConst.DEFAULT);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, BranchIndex, PathIndex);
        argumentBuilder.AddArgument(SubConst.PATH_SEPARATOR, true);
        AddArgument(argumentBuilder, arguments[PathIndex]);
    }

    private protected override string? ValidateBody()
    {
        if (!arguments[PathIndex].HasValue || IsInvalid(Path))
            return SubConst.Errors.PATH_INVALID;
        if (Default == arguments[BranchIndex].HasValue)
            return SubConst.Errors.BRANCH_CONFLICT;
        if (arguments[BranchIndex].HasValue && string.IsNullOrWhiteSpace(Branch))
            return SubConst.Errors.VALUE_INVALID;

        return null;
    }
}
