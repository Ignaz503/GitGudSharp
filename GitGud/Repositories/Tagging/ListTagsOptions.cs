// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using TagConst = GitGud.Constants.Const.Repository.Tagging;

namespace GitGud.Repositories.Tagging;

public sealed class ListTagsOptions : IOperationOptions
{
    const int IgnoreCaseIndex = 0;
    const int ContainsIndex = 1;
    const int NoContainsIndex = 2;
    const int MergedIndex = 3;
    const int NoMergedIndex = 4;
    const int PointsAtIndex = 5;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[6];

    /// <summary>Shell wildcard patterns; a tag is listed if any matches.</summary>
    public IReadOnlyList<string>? Patterns { get; init; }

    /// <summary>Sort keys, e.g. "version:refname" or "-creatordate". The last key is primary.</summary>
    public IReadOnlyList<string>? Sort { get; init; }

    public bool IgnoreCase
    {
        get => arguments[IgnoreCaseIndex].HasValue;
        init => arguments[IgnoreCaseIndex] = value
            ? Optional<Pair>.Some(new Pair(TagConst.IGNORE_CASE))
            : Optional<Pair>.None;
    }

    public string? Contains
    {
        get => GetValue(ContainsIndex);
        init => SetValue(ContainsIndex, TagConst.CONTAINS, value);
    }

    public string? NoContains
    {
        get => GetValue(NoContainsIndex);
        init => SetValue(NoContainsIndex, TagConst.NO_CONTAINS, value);
    }

    public string? Merged
    {
        get => GetValue(MergedIndex);
        init => SetValue(MergedIndex, TagConst.MERGED, value);
    }

    public string? NoMerged
    {
        get => GetValue(NoMergedIndex);
        init => SetValue(NoMergedIndex, TagConst.NO_MERGED, value);
    }

    public string? PointsAt
    {
        get => GetValue(PointsAtIndex);
        init => SetValue(PointsAtIndex, TagConst.POINTS_AT, value);
    }

    private string? GetValue(int index)
        => arguments[index].Transform(static v => v.Second, static () => default!);

    private void SetValue(int index, string name, string? value)
        => arguments[index] = value is string val
            ? Optional<Pair>.Some(new Pair(name, val))
            : Optional<Pair>.None;

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(TagConst.LIST, true);
        argumentBuilder.AddArgument(
            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, TagConst.FORMAT, TagConst.LIST_FORMAT), true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (pair.TwoValues)
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
            else
                argumentBuilder.AddArgument(pair.First!, true);
        }

        var sort = Sort;
        if (sort is not null)
        {
            for (var i = 0; i < sort.Count; i++)
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, TagConst.SORT, sort[i]), true);
        }

        var patterns = Patterns;
        if (patterns is not null)
        {
            for (var i = 0; i < patterns.Count; i++)
                argumentBuilder.AddArgument(patterns[i], true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var error = ValidateValues(Patterns, true) ?? ValidateValues(Sort, false);

        for (var i = ContainsIndex; error is null && i < arguments.Length; i++)
        {
            var value = GetValue(i);
            if (value is not null && string.IsNullOrWhiteSpace(value))
                error = TagConst.Errors.VALUE_INVALID;
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_TAG, error));

        return Result<Nothing, IError>.Some(new());
    }

    private static string? ValidateValues(IReadOnlyList<string>? values, bool rejectDash)
    {
        if (values is null)
            return null;

        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (string.IsNullOrWhiteSpace(value) || (rejectDash && value[0] == Const.Repository.Branching.OPTION_PREFIX))
                return TagConst.Errors.VALUE_INVALID;
        }

        return null;
    }
}