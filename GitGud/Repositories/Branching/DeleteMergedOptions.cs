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

public sealed class DeleteMergedOptions : IOperationOptions
{
    const int DryRunIndex = 0;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[1];

    /// <summary>Upstream patterns (a ref, a remote or a glob) whose merged local branches are deleted (--delete-merged). At least one is required.</summary>
    public IReadOnlyList<string>? Upstreams { get; init; }

    /// <summary>Limits which local branches are considered.</summary>
    public IReadOnlyList<string>? BranchPatterns { get; init; }

    /// <summary>Only reports which branches would be deleted (--dry-run).</summary>
    public bool DryRun
    {
        get => arguments[DryRunIndex].HasValue;
        init => arguments[DryRunIndex] = value
            ? Optional<Pair>.Some(new Pair(BranchConst.DRY_RUN))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);

        if (arguments[DryRunIndex].HasValue)
            argumentBuilder.Flag(arguments[DryRunIndex].Value.First!);

        var upstreams = Upstreams;
        if (upstreams is not null)
        {
            for (var i = 0; i < upstreams.Count; i++)
            {
                argumentBuilder.Flag(BranchConst.DELETE_MERGED);
                argumentBuilder.AddArgument(upstreams[i], true);
            }
        }

        var patterns = BranchPatterns;
        if (patterns is null)
            return;

        for (var i = 0; i < patterns.Count; i++)
            argumentBuilder.AddArgument(patterns[i], true);
    }

    public Result<Nothing, IError> Validate()
    {
        var upstreams = Upstreams;
        string? error;

        if (upstreams is null || upstreams.Count == 0)
            error = BranchConst.Errors.UPSTREAM_PATTERNS_REQUIRED;
        else
            error = ValidateValues(upstreams) ?? ValidateValues(BranchPatterns);

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));

        return Result<Nothing, IError>.Some(new());
    }

    private static string? ValidateValues(IReadOnlyList<string>? values)
    {
        if (values is null)
            return null;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (string.IsNullOrWhiteSpace(value) || value[0] == BranchConst.OPTION_PREFIX)
                return BranchConst.Errors.PATTERN_INVALID;
        }

        return null;
    }
}