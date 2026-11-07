// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using BranchConst = GitGud.Constants.Const.Repository.Branching;

namespace GitGud.Repositories.Branching;

public sealed class DeleteRemoteBranchOptions : IOperationOptions
{
    const int RemoteIndex = 0;
    const int NameIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];

    public Remote Remote
    {
        get => new(Get(RemoteIndex));
        init => Set(RemoteIndex, value);
    }

    public Branch Name
    {
        get => new(Get(NameIndex));
        init => Set(NameIndex, value);
    }

    private string Get(int index)
        => arguments[index].Transform(static v => v.First, static () => default!) ?? string.Empty;

    private void Set(int index, string? value)
        => arguments[index] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;

    /// <summary>Writes <c>git push &lt;remote&gt; --delete &lt;branch&gt;</c>.</summary>
    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.PUSH, true);
        argumentBuilder.AddArgument(Get(RemoteIndex), true);
        argumentBuilder.Flag(Const.Repository.Arguments.DELETE);
        argumentBuilder.AddArgument(Get(NameIndex), true);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = BranchNameRules.Validate(
                arguments[RemoteIndex].Transform(static v => v.First, static () => default!), BranchConst.Errors.REMOTE_REQUIRED)
            ?? BranchNameRules.Validate(
                arguments[NameIndex].Transform(static v => v.First, static () => default!), BranchConst.Errors.NAME_REQUIRED);

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_PUSH, error));

        return Result<Nothing, IError>.Some(new());
    }
}