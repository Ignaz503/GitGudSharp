// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule absorbgitdirs</summary>
public sealed class SubmoduleAbsorbGitDirsOptions : SubmodulePathOptionsBase
{
    public SubmoduleAbsorbGitDirsOptions() : base(1) { }

    private protected override string SubCommand => SubConst.ABSORB_GIT_DIRS;

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
        => AddPaths(argumentBuilder, Paths);

    private protected override string? ValidateBody() => ValidatePaths(Paths);
}