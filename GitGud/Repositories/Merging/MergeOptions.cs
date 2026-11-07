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

namespace GitGud.Repositories.Merging;

public sealed class MergeOptions : IOperationOptions
{
    const int StrategyIndex = 0;
    const int NoCommitIndex = 1;
    const int MessageIndex = 2;
    const int BranchIndex = 3;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[4];
    private MergeStrategy _strategy;

    /// <summary>Branch to merge into the current branch.</summary>
    public Branch Branch
    {
        get => new(arguments[BranchIndex].Transform(static v => v.First, static () => default!) ?? string.Empty);
        init => arguments[BranchIndex] = Optional<Pair>.Some(new Pair(value));
    }

    public MergeStrategy Strategy
    {
        get => _strategy;
        init
        {
            _strategy = value;
            arguments[StrategyIndex] = value switch
            {
                MergeStrategy.NoFastForward => Optional<Pair>.Some(new Pair(Const.Repository.Merging.NO_FF)),
                MergeStrategy.FastForwardOnly => Optional<Pair>.Some(new Pair(Const.Repository.Arguments.FF_ONLY)),
                MergeStrategy.Squash => Optional<Pair>.Some(new Pair(Const.Repository.Merging.SQUASH)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>Stop before creating the merge commit. Ignored for <see cref="MergeStrategy.Squash"/>, which never commits.</summary>
    public bool NoCommit
    {
        get => arguments[NoCommitIndex].HasValue;
        init => arguments[NoCommitIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Merging.NO_COMMIT))
            : Optional<Pair>.None;
    }

    /// <summary>Merge commit message. Blank values are ignored.</summary>
    public string? Message
    {
        get => arguments[MessageIndex].Transform(static v => v.Second, static () => default!);
        init => arguments[MessageIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.MESSAGE_SHORT, value!))
            : Optional<Pair>.None;
    }

    /// <summary>
    /// Resolves conflicts. When <see langword="null"/>, the merge is left in its conflicted state and a merge conflict error is returned.
    /// </summary>
    public MergeConflictResolver? ConflictResolver { get; init; }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.Repository.Merging.MERGE, true);
        argumentBuilder.Flag(Const.Repository.Arguments.NO_EDIT);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (i == StrategyIndex || i == NoCommitIndex)
            {
                argumentBuilder.Flag(pair.First!);
                continue;
            }

            argumentBuilder.AddArgument(pair.First!, true);
            if (pair.TwoValues)
                argumentBuilder.AddArgument(pair.Second!, true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var name = arguments[BranchIndex].Transform(static v => v.First, static () => default!);
        string? error = null;

        if (string.IsNullOrWhiteSpace(name))
            error = Const.Repository.Merging.Errors.BRANCH_REQUIRED;
        else if (name![0] == Const.Repository.Merging.OPTION_PREFIX)
            error = Const.Repository.Merging.Errors.BRANCH_INVALID;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_MERGE, error));

        return Result<Nothing, IError>.Some(new());
    }
}