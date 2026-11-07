// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule deinit</summary>
public sealed class SubmoduleDeinitOptions : SubmodulePathOptionsBase
{
    const int ForceIndex = 1;
    const int AllIndex = 2;

    public SubmoduleDeinitOptions() : base(3) { }

    private protected override string SubCommand => SubConst.DEINIT;

    /// <summary>Removes the work tree even with local modifications (--force).</summary>
    public bool Force
    {
        get => arguments[ForceIndex].HasValue;
        init => SetFlag(ForceIndex, value, SubConst.FORCE);
    }

    /// <summary>Deinitializes all submodules (--all).</summary>
    public bool All
    {
        get => arguments[AllIndex].HasValue;
        init => SetFlag(AllIndex, value, SubConst.ALL);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, ForceIndex, arguments.Length);
        AddPaths(argumentBuilder, Paths);
    }

    private protected override string? ValidateBody()
    {
        if (All && Paths.Count > 0)
            return SubConst.Errors.PATHS_CONFLICT;
        if (!All && Paths.Count == 0)
            return SubConst.Errors.PATHS_REQUIRED;

        return ValidatePaths(Paths);
    }
}
