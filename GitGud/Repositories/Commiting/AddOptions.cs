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

public sealed class AddOptions : IOperationOptions
{
    const int AllIndex = 0;
    const int UpdateIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];

    /// <summary>Paths (or pathspecs) to stage.</summary>
    public IReadOnlyList<FilePath>? Paths { get; init; }

    /// <summary>Stages all changes in the working tree, including deletions (-A). Takes precedence over <see cref="Update"/>.</summary>
    public bool All
    {
        get => arguments[AllIndex].HasValue;
        init => arguments[AllIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.ADD_ALL_SHORT))
            : Optional<Pair>.None;
    }

    /// <summary>Stages modifications and deletions of tracked files only (-u).</summary>
    public bool Update
    {
        get => arguments[UpdateIndex].HasValue;
        init => arguments[UpdateIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.UPDATE_SHORT))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.ADD, true);

        var all = arguments[AllIndex].HasValue;
        for (var i = 0; i < arguments.Length; i++)
        {
            if (all && i == UpdateIndex)
                continue;

            var optional = arguments[i];
            if (optional.HasValue)
                argumentBuilder.AddArgument(optional.Value.First!, true);
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
        var hasPaths = paths is not null && paths.Count > 0;
        string? error = null;

        if (!hasPaths && !All && !Update)
            error = Const.Repository.OperationErrors.PATHS_REQUIRED;
        else if (hasPaths)
        {
            for (var i = 0; i < paths!.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                {
                    error = Const.Repository.OperationErrors.PATH_INVALID;
                    break;
                }
            }
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_ADD, error));

        return Result<Nothing, IError>.Some(new());
    }
}