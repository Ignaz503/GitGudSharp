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

public sealed class SetUpstreamOptions : IOperationOptions
{
    const int UpstreamIndex = 0;
    const int NameIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];

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

    /// <summary>The upstream, e.g. "origin/main". Blank values are ignored and fail validation.</summary>
    public string? Upstream
    {
        get => arguments[UpstreamIndex].Transform(static v => v.Second, static () => default!);
        init => arguments[UpstreamIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(BranchConst.SET_UPSTREAM_TO, value!))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);

        if (arguments[UpstreamIndex].HasValue)
        {
            var pair = arguments[UpstreamIndex].Value;
            argumentBuilder.AddArgument(
                string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
        }

        if (arguments[NameIndex].HasValue)
            argumentBuilder.AddArgument(arguments[NameIndex].Value.First!, true);
    }

    public Result<Nothing, IError> Validate()
    {
        var upstream = Upstream;
        string? error = null;

        if (upstream is null)
            error = BranchConst.Errors.UPSTREAM_REQUIRED;
        else if (upstream[0] == BranchConst.OPTION_PREFIX)
            error = BranchConst.Errors.NAME_INVALID;
        else if (arguments[NameIndex].HasValue)
            error = BranchNameRules.Validate(arguments[NameIndex].Value.First, BranchConst.Errors.NAME_REQUIRED);

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));

        return Result<Nothing, IError>.Some(new());
    }
}