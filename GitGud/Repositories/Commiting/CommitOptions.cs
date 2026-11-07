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

namespace GitGud.Repositories.Commiting;

public sealed class CommitOptions : IOperationOptions
{
    const int AllIndex = 0;
    const int AmendIndex = 1;
    const int AllowEmptyIndex = 2;
    const int MessageIndex = 3;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[4];

    /// <summary>Commit message. Required; blank values are ignored and fail validation.</summary>
    public string Message
    {
        get => arguments[MessageIndex].Transform(static v => v.Second ?? string.Empty, static () => string.Empty);
        init => arguments[MessageIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.MESSAGE_SHORT, value))
            : Optional<Pair>.None;
    }

    /// <summary>Automatically stages modified and deleted tracked files (-a).</summary>
    public bool All
    {
        get => arguments[AllIndex].HasValue;
        init => arguments[AllIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.ALL_SHORT))
            : Optional<Pair>.None;
    }

    public bool Amend
    {
        get => arguments[AmendIndex].HasValue;
        init => arguments[AmendIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.AMEND))
            : Optional<Pair>.None;
    }

    public bool AllowEmpty
    {
        get => arguments[AllowEmptyIndex].HasValue;
        init => arguments[AllowEmptyIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.ALLOW_EMPTY))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.COMMIT, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (i == AllIndex)
            {
                argumentBuilder.AddArgument(pair.First!, true);
            }
            else if (pair.TwoValues)
            {
                argumentBuilder.AddArgument(pair.First!, true);
                argumentBuilder.AddArgument(pair.Second!, true);
            }
            else
            {
                argumentBuilder.Flag(pair.First!);
            }
        }
    }

    public Result<Nothing, IError> Validate()
    {
        if (!arguments[MessageIndex].HasValue)
            return Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_COMMIT, Const.Repository.OperationErrors.MESSAGE_REQUIRED));

        return Result<Nothing, IError>.Some(new());
    }
}