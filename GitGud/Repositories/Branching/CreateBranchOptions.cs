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

public sealed class CreateBranchOptions : IOperationOptions
{
    const int TrackIndex = 0;
    const int ForceIndex = 1;
    const int RecurseSubmodulesIndex = 2;
    const int NameIndex = 3;
    const int StartPointIndex = 4;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[5];
    private BranchTrackMode _track;

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

    /// <summary>Commit, tag or branch to start from. Defaults to HEAD. Blank values are ignored.</summary>
    public string? StartPoint
    {
        get => arguments[StartPointIndex].Transform(static v => v.First, static () => default!);
        init => arguments[StartPointIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    /// <summary>Upstream tracking behavior (--track / --no-track).</summary>
    public BranchTrackMode Track
    {
        get => _track;
        init
        {
            _track = value;
            arguments[TrackIndex] = value switch
            {
                BranchTrackMode.Direct => Optional<Pair>.Some(new Pair(BranchConst.TRACK, BranchConst.TRACK_DIRECT)),
                BranchTrackMode.Inherit => Optional<Pair>.Some(new Pair(BranchConst.TRACK, BranchConst.TRACK_INHERIT)),
                BranchTrackMode.None => Optional<Pair>.Some(new Pair(BranchConst.NO_TRACK)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>Resets the branch to the start point even if it already exists (-f).</summary>
    public bool Force
    {
        get => arguments[ForceIndex].HasValue;
        init => arguments[ForceIndex] = value
            ? Optional<Pair>.Some(new Pair(BranchConst.FORCE_SHORT))
            : Optional<Pair>.None;
    }

    /// <summary>Also creates the branch in submodules (--recurse-submodules, experimental).</summary>
    public bool RecurseSubmodules
    {
        get => arguments[RecurseSubmodulesIndex].HasValue;
        init => arguments[RecurseSubmodulesIndex] = value
            ? Optional<Pair>.Some(new Pair(BranchConst.RECURSE_SUBMODULES))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            switch (i)
            {
                case ForceIndex:
                case NameIndex:
                case StartPointIndex:
                    argumentBuilder.AddArgument(pair.First!, true);
                    break;
                default:
                    if (pair.TwoValues)
                        argumentBuilder.AddArgument(
                            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
                    else
                        argumentBuilder.Flag(pair.First!);
                    break;
            }
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var error = BranchNameRules.Validate(
            arguments[NameIndex].Transform(static v => v.First, static () => default!), BranchConst.Errors.NAME_REQUIRED);

        if (error is null && StartPoint is string start && start[0] == BranchConst.OPTION_PREFIX)
            error = BranchConst.Errors.START_POINT_INVALID;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));

        return Result<Nothing, IError>.Some(new());
    }
}