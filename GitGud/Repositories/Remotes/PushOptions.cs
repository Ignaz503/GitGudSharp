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

public sealed class PushOptions : IOperationOptions
{
    const int SetUpstreamIndex = 0;
    const int ForceWithLeaseIndex = 1;
    const int TagsIndex = 2;
    const int RemoteIndex = 3;
    const int BranchIndex = 4;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[5];

    public Remote? Remote
    {
        get => arguments[RemoteIndex].Transform(static v => new Remote(v.First!), static () => default);
        init => arguments[RemoteIndex] = value is Remote remote && !string.IsNullOrWhiteSpace(remote.ToString())
            ? Optional<Pair>.Some(new Pair(remote.ToString()))
            : Optional<Pair>.None;
    }

    /// <summary>Branch (or refspec) to push. Requires <see cref="Remote"/>.</summary>
    public Branch? Branch
    {
        get => arguments[BranchIndex].Transform(static v => new Branch(v.First!), static () => default);
        init => arguments[BranchIndex] = value is Branch branch && !string.IsNullOrWhiteSpace(branch.ToString())
            ? Optional<Pair>.Some(new Pair(branch.ToString()))
            : Optional<Pair>.None;
    }

    public bool SetUpstream
    {
        get => arguments[SetUpstreamIndex].HasValue;
        init => arguments[SetUpstreamIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.SET_UPSTREAM))
            : Optional<Pair>.None;
    }

    public bool ForceWithLease
    {
        get => arguments[ForceWithLeaseIndex].HasValue;
        init => arguments[ForceWithLeaseIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.FORCE_WITH_LEASE))
            : Optional<Pair>.None;
    }

    public bool Tags
    {
        get => arguments[TagsIndex].HasValue;
        init => arguments[TagsIndex] = value
            ? Optional<Pair>.Some(new Pair(Const.Repository.Arguments.TAGS))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.PUSH, true);

        var hasRemote = arguments[RemoteIndex].HasValue;
        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var name = optional.Value.First!;
            if (i < RemoteIndex)
                argumentBuilder.Flag(name);
            else if (i == RemoteIndex || hasRemote)
                argumentBuilder.AddArgument(name, true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        if (arguments[BranchIndex].HasValue && !arguments[RemoteIndex].HasValue)
            return Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_PUSH, Const.Repository.OperationErrors.REMOTE_REQUIRED));

        return Result<Nothing, IError>.Some(new());
    }
}