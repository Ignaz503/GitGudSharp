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

/// <summary>Options for apply, pop and drop.</summary>
public sealed class StashEntryOptions : IOperationOptions
{
    const int RestoreIndexIndex = 0;
    const int StashIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];
    private int? _index;

    /// <summary>Stash to target; <see langword="null"/> means the most recent.</summary>
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

    /// <summary>Also restore staged changes. Ignored by drop.</summary>
    public bool RestoreIndex
    {
        get => arguments[RestoreIndexIndex].HasValue;
        init => arguments[RestoreIndexIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Stashing.INDEX))
            : Optional<Pair>.None;
    }

    /// <summary>Writes the options only (<c>[--index] [stash@{n}]</c>); the caller adds <c>stash &lt;subcommand&gt;</c>.</summary>
    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
        => Apply(argumentBuilder, true);

    internal void Apply(CliWrapArgumentBuilder argumentBuilder, bool supportsRestoreIndex)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            if (i == RestoreIndexIndex && !supportsRestoreIndex)
                continue;

            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var first = optional.Value.First!;
            if (i == RestoreIndexIndex)
                argumentBuilder.Flag(first);
            else
                argumentBuilder.AddArgument(first, true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        if (_index is int value && value < 0)
            return Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_STASH, Const.Repository.Stashing.Errors.INDEX_INVALID));

        return Result<Nothing, IError>.Some(new());
    }
}