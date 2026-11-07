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

public sealed class ListBranchesOptions : IOperationOptions
{
    const int ScopeIndex = 0;
    const int IgnoreCaseIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];
    private BranchScope _scope = BranchScope.Local;

    public BranchScope Scope
    {
        get => _scope;
        init
        {
            _scope = value;
            arguments[ScopeIndex] = value switch
            {
                BranchScope.All => Optional<Pair>.Some(new Pair(BranchConst.ALL_SHORT)),
                BranchScope.Remote => Optional<Pair>.Some(new Pair(BranchConst.REMOTES_SHORT)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>Sorting and filtering are case insensitive (-i).</summary>
    public bool IgnoreCase
    {
        get => arguments[IgnoreCaseIndex].HasValue;
        init => arguments[IgnoreCaseIndex] = value
            ? Optional<Pair>.Some(new Pair(BranchConst.IGNORE_CASE))
            : Optional<Pair>.None;
    }

    /// <summary>Shell wildcards restricting the listed branches.</summary>
    public IReadOnlyList<string>? Patterns { get; init; }

    /// <summary>Only branches containing these commits (use "HEAD" for the current commit).</summary>
    public IReadOnlyList<string>? Contains { get; init; }

    /// <summary>Only branches not containing these commits.</summary>
    public IReadOnlyList<string>? NoContains { get; init; }

    /// <summary>Only branches whose tips are reachable from these commits.</summary>
    public IReadOnlyList<string>? Merged { get; init; }

    /// <summary>Only branches whose tips are not reachable from these commits.</summary>
    public IReadOnlyList<string>? NoMerged { get; init; }

    /// <summary>Only branches pointing at these objects.</summary>
    public IReadOnlyList<string>? PointsAt { get; init; }

    /// <summary>Only branches whose configured upstream matches these refs or patterns.</summary>
    public IReadOnlyList<string>? Forked { get; init; }

    /// <summary>Sort keys as in git for-each-ref; prefix a key with '-' for descending. The last key is the primary key.</summary>
    public IReadOnlyList<string>? Sort { get; init; }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);
        argumentBuilder.Flag(BranchConst.LIST);
        argumentBuilder.AddArgument(
            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, BranchConst.FORMAT, BranchConst.LIST_FORMAT), true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var name = optional.Value.First!;
            if (i == ScopeIndex)
                argumentBuilder.AddArgument(name, true);
            else
                argumentBuilder.Flag(name);
        }

        AddValues(argumentBuilder, BranchConst.CONTAINS, Contains);
        AddValues(argumentBuilder, BranchConst.NO_CONTAINS, NoContains);
        AddValues(argumentBuilder, BranchConst.MERGED, Merged);
        AddValues(argumentBuilder, BranchConst.NO_MERGED, NoMerged);
        AddValues(argumentBuilder, BranchConst.POINTS_AT, PointsAt);
        AddValues(argumentBuilder, BranchConst.FORKED, Forked);
        AddValues(argumentBuilder, BranchConst.SORT, Sort);

        var patterns = Patterns;
        if (patterns is null)
            return;

        for (var i = 0; i < patterns.Count; i++)
            argumentBuilder.AddArgument(patterns[i], true);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = ValidateValues(Contains, BranchConst.Errors.COMMIT_INVALID, true)
            ?? ValidateValues(NoContains, BranchConst.Errors.COMMIT_INVALID, true)
            ?? ValidateValues(Merged, BranchConst.Errors.COMMIT_INVALID, true)
            ?? ValidateValues(NoMerged, BranchConst.Errors.COMMIT_INVALID, true)
            ?? ValidateValues(PointsAt, BranchConst.Errors.COMMIT_INVALID, true)
            ?? ValidateValues(Forked, BranchConst.Errors.PATTERN_INVALID, true)
            ?? ValidateValues(Patterns, BranchConst.Errors.PATTERN_INVALID, true)
            ?? ValidateValues(Sort, BranchConst.Errors.SORT_INVALID, false);

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));

        return Result<Nothing, IError>.Some(new());
    }

    private static void AddValues(CliWrapArgumentBuilder argumentBuilder, string name, IReadOnlyList<string>? values)
    {
        if (values is null)
            return;

        for (var i = 0; i < values.Count; i++)
            argumentBuilder.AddArgument(
                string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, name, values[i]), true);
    }

    private static string? ValidateValues(IReadOnlyList<string>? values, string error, bool rejectDash)
    {
        if (values is null)
            return null;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (string.IsNullOrWhiteSpace(value) || (rejectDash && value[0] == BranchConst.OPTION_PREFIX))
                return error;
        }

        return null;
    }
}