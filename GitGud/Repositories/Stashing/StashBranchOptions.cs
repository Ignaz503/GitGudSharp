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

namespace GitGud.Repositories.Stashing;

public sealed class StashBranchOptions : IOperationOptions
{
    const int NameIndex = 0;
    const int StashIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];
    private int? _index;

    /// <summary>Name of the branch to create.</summary>
    public Branch Name
    {
        get => new(arguments[NameIndex].Transform(static v => v.First, static () => default!) ?? string.Empty);
        init => arguments[NameIndex] = Optional<Pair>.Some(new Pair(value));
    }

    /// <summary>Stash to base the branch on; <see langword="null"/> means the most recent.</summary>
    public int? Index
    {
        get => _index;
        init
        {
            _index = value;
            arguments[StashIndex] = value is int entry
                ? Optional<Pair>.Some(new Pair(string.Format(Const.Repository.Stashing.FORMAT_STASH_REF, entry)))
                : Optional<Pair>.None;
        }
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.STASH, true);
        argumentBuilder.AddArgument(Const.Repository.Stashing.BRANCH, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (optional.HasValue)
                argumentBuilder.AddArgument(optional.Value.First!, true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        string? error;
        var name = arguments[NameIndex].Transform(static v => v.First, static () => default!);

        if (string.IsNullOrWhiteSpace(name))
            error = Const.Repository.Branching.Errors.NAME_REQUIRED;
        else if (name![0] == Const.Repository.Branching.OPTION_PREFIX)
            error = Const.Repository.Branching.Errors.NAME_INVALID;
        else if (_index is int value && value < 0)
            error = Const.Repository.Stashing.Errors.INDEX_INVALID;
        else
            error = null;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_STASH, error));

        return Result<Nothing, IError>.Some(new());
    }
}