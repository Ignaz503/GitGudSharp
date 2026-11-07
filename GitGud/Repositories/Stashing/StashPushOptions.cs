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

public sealed class StashPushOptions : IOperationOptions
{
    const int IncludeAllIndex = 0;
    const int IncludeUntrackedIndex = 1;
    const int KeepIndexIndex = 2;
    const int MessageIndex = 3;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[4];

    /// <summary>Message for the stash entry. Blank values are ignored.</summary>
    public string? Message
    {
        get => arguments[MessageIndex].Transform(static v => v.Second, static () => default!);
        init => arguments[MessageIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.MESSAGE_SHORT, value!))
            : Optional<Pair>.None;
    }

    /// <summary>Also stash untracked files.</summary>
    public bool IncludeUntracked
    {
        get => arguments[IncludeUntrackedIndex].HasValue;
        init => arguments[IncludeUntrackedIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Stashing.INCLUDE_UNTRACKED))
            : Optional<Pair>.None;
    }

    /// <summary>Also stash untracked and ignored files. Takes precedence over <see cref="IncludeUntracked"/>.</summary>
    public bool IncludeAll
    {
        get => arguments[IncludeAllIndex].HasValue;
        init => arguments[IncludeAllIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Stashing.ALL))
            : Optional<Pair>.None;
    }

    /// <summary>Leave staged changes in the working tree.</summary>
    public bool KeepIndex
    {
        get => arguments[KeepIndexIndex].HasValue;
        init => arguments[KeepIndexIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Stashing.KEEP_INDEX))
            : Optional<Pair>.None;
    }

    /// <summary>Limit the stash to these paths.</summary>
    public IReadOnlyList<string>? Paths { get; init; }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.STASH, true);
        argumentBuilder.AddArgument(Const.Repository.Stashing.PUSH, true);

        var includeAll = arguments[IncludeAllIndex].HasValue;
        for (var i = 0; i < arguments.Length; i++)
        {
            if (includeAll && i == IncludeUntrackedIndex)
                continue;

            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (pair.TwoValues)
            {
                argumentBuilder.AddArgument(pair.First!, true);
                argumentBuilder.AddArgument(pair.Second!, true);
            }
            else
            {
                argumentBuilder.Flag(pair.First!);
            }
        }

        var paths = Paths;
        if (paths is not null && paths.Count > 0)
        {
            argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
            for (var i = 0; i < paths.Count; i++)
                argumentBuilder.AddArgument(paths[i], true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var paths = Paths;
        if (paths is not null)
        {
            for (var i = 0; i < paths.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                    return Result<Nothing, IError>.None(
                        new RepositoryValidationError(Const.Repository.GIT_STASH, Const.Repository.Stashing.Errors.PATH_INVALID));
            }
        }

        return Result<Nothing, IError>.Some(new());
    }
}