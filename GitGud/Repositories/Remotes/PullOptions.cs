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

namespace GitGud.Repositories.Remotes;

public sealed class PullOptions : IOperationOptions
{
    const int StrategyIndex = 0;
    const int RemoteIndex = 1;
    const int BranchIndex = 2;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[3];
    private PullStrategy _strategy;

    public Remote? Remote
    {
        get => arguments[RemoteIndex].Transform(static v => new Remote(v.First!), static () => default);
        init => arguments[RemoteIndex] = value is Remote remote && !string.IsNullOrWhiteSpace(remote.ToString())
            ? Optional<Pair>.Some(new Pair(remote.ToString()))
            : Optional<Pair>.None;
    }

    /// <summary>Remote branch to pull. Requires <see cref="Remote"/>. Blank values are ignored.</summary>
    public string? Branch
    {
        get => arguments[BranchIndex].Transform(static v => v.First, static () => default!);
        init => arguments[BranchIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    public PullStrategy Strategy
    {
        get => _strategy;
        init
        {
            _strategy = value;
            arguments[StrategyIndex] = value switch
            {
                PullStrategy.Merge => Optional<Pair>.Some(
                    new Pair(Const.Repository.Arguments.REBASE, Const.FORMAT_REPOSITORY_SHARING_FALSE)),
                PullStrategy.Rebase => Optional<Pair>.Some(new Pair(Const.Repository.Arguments.REBASE)),
                PullStrategy.FastForwardOnly => Optional<Pair>.Some(new Pair(Const.Repository.Arguments.FF_ONLY)),
                _ => Optional<Pair>.None
            };
        }
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.PULL, true);

        var hasRemote = arguments[RemoteIndex].HasValue;
        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (i == StrategyIndex)
            {
                if (pair.TwoValues)
                    argumentBuilder.AddArgument(
                        string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
                else
                    argumentBuilder.Flag(pair.First!);
            }
            else if (i == RemoteIndex || hasRemote)
            {
                argumentBuilder.AddArgument(pair.First!, true);
            }
        }
    }

    public Result<Nothing, IError> Validate()
    {
        if (arguments[BranchIndex].HasValue && !arguments[RemoteIndex].HasValue)
            return Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_PULL, Const.Repository.OperationErrors.REMOTE_REQUIRED));

        return Result<Nothing, IError>.Some(new());
    }
}