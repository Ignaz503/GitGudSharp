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

public sealed class UnsetUpstreamOptions : IOperationOptions
{
    const int NameIndex = 0;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[1];

    /// <summary>Branch to configure; defaults to the current branch when left empty.</summary>
    public Branch Name
    {
        get => new(arguments[NameIndex].Transform(static v => v.First, static () => default!) ?? string.Empty);
        init
        {
            string? name = value;
            arguments[NameIndex] = !string.IsNullOrWhiteSpace(name)
                ? Optional<Pair>.Some(new Pair(name!))
                : Optional<Pair>.None;
        }
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);
        argumentBuilder.Flag(BranchConst.UNSET_UPSTREAM);

        if (arguments[NameIndex].HasValue)
            argumentBuilder.AddArgument(arguments[NameIndex].Value.First!, true);
    }

    public Result<Nothing, IError> Validate()
    {
        if (arguments[NameIndex].HasValue)
        {
            var error = BranchNameRules.Validate(arguments[NameIndex].Value.First, BranchConst.Errors.NAME_REQUIRED);
            if (error is not null)
                return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));
        }

        return Result<Nothing, IError>.Some(new());
    }
}