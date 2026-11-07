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

public sealed class RenameBranchOptions : IOperationOptions
{
    const int NameIndex = 0;
    const int NewNameIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];
    private bool _force;

    /// <summary>Branch to rename; defaults to the current branch when left empty.</summary>
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

    public Branch NewName
    {
        get => new(arguments[NewNameIndex].Transform(static v => v.First, static () => default!) ?? string.Empty);
        init
        {
            string? name = value;
            arguments[NewNameIndex] = !string.IsNullOrWhiteSpace(name)
                ? Optional<Pair>.Some(new Pair(name!))
                : Optional<Pair>.None;
        }
    }

    /// <summary>Overwrite an existing branch with the new name.</summary>
    public bool Force
    {
        get => _force;
        init => _force = value;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);
        argumentBuilder.AddArgument(_force ? BranchConst.MOVE_FORCE_SHORT : BranchConst.MOVE_SHORT, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (optional.HasValue)
                argumentBuilder.AddArgument(optional.Value.First!, true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var error = BranchNameRules.Validate(arguments[NewNameIndex].Transform(static v => v.First, static () => default!),
            BranchConst.Errors.NEW_NAME_REQUIRED);

        if (error is null && arguments[NameIndex].HasValue)
            error = BranchNameRules.Validate(arguments[NameIndex].Value.First, BranchConst.Errors.NAME_REQUIRED);

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));

        return Result<Nothing, IError>.Some(new());
    }
}