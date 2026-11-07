// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule foreach</summary>
public sealed class SubmoduleForeachOptions : SubmoduleOptionsBase
{
    const int RecursiveIndex = 1;
    const int CommandIndex = 2;

    public SubmoduleForeachOptions() : base(3) { }

    private protected override string SubCommand => SubConst.FOREACH;

    /// <summary>Recurses into nested submodules (--recursive).</summary>
    public bool Recursive
    {
        get => arguments[RecursiveIndex].HasValue;
        init => SetFlag(RecursiveIndex, value, SubConst.RECURSIVE);
    }

    /// <summary>Shell command evaluated in each checked out submodule (required).</summary>
    public string? Command
    {
        get => GetValue(CommandIndex);
        init => SetValue(CommandIndex, value);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
        => AddRange(argumentBuilder, RecursiveIndex, arguments.Length);

    private protected override string? ValidateBody()
        => string.IsNullOrWhiteSpace(Command) ? SubConst.Errors.COMMAND_REQUIRED : null;
}
