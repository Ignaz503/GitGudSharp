// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Operations;
using GitGud.Utility;

namespace GitGud.Repositories.Remotes;

public sealed class FetchOptions : IOperationOptions
{
    const int PruneIndex = 0;
    const int TagsIndex = 1;
    const int DepthIndex = 2;
    const int AllIndex = 3;
    const int RemoteIndex = 4;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[5];
    private int? _depth;

    /// <summary>Remote to fetch from. Null uses git's default remote.</summary>
    public Remote? Remote
    {
        get => arguments[RemoteIndex].Transform(static v => new Remote(v.First!), static () => default);
        init => arguments[RemoteIndex] = value is Remote remote && !string.IsNullOrWhiteSpace(remote.ToString())
            ? Optional<Pair>.Some(new Pair(remote.ToString()))
            : Optional<Pair>.None;
    }

    /// <summary>Fetches all remotes. Overrides <see cref="Remote"/>.</summary>
    public bool All
    {
        get => arguments[AllIndex].HasValue;
        init => arguments[AllIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.ALL))
            : Optional<Pair>.None;
    }

    public bool Prune
    {
        get => arguments[PruneIndex].HasValue;
        init => arguments[PruneIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.PRUNE))
            : Optional<Pair>.None;
    }

    public bool Tags
    {
        get => arguments[TagsIndex].HasValue;
        init => arguments[TagsIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.TAGS))
            : Optional<Pair>.None;
    }

    public int? Depth
    {
        get => _depth;
        init
        {
            _depth = value;
            arguments[DepthIndex] = value is int depth
                ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.DEPTH, depth.ToString()))
                : Optional<Pair>.None;
        }
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.FETCH, true);

        var all = arguments[AllIndex].HasValue;
        for (var i = 0; i < arguments.Length; i++)
        {
            if (all && i == RemoteIndex)
                continue;

            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (i == RemoteIndex)
                argumentBuilder.AddArgument(pair.First!, true);
            else if (pair.TwoValues)
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
            else
                argumentBuilder.Flag(pair.First!);
        }
    }

    public Result<Nothing, IError> Validate()
        => Result<Nothing, IError>.Some(new());
}