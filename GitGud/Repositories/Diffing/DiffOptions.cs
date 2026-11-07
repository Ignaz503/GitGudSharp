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

using System.Globalization;

using DifConst = GitGud.Constants.Const.Repository.Diffing;

namespace GitGud.Repositories.Diffing;

public sealed class DiffOptions : IOperationOptions
{
    const int StagedIndex = 0;
    const int MergeBaseIndex = 1;
    const int ContextLinesIndex = 2;
    const int AlgorithmIndex = 3;
    const int WhitespaceIndex = 4;
    const int IgnoreBlankLinesIndex = 5;
    const int DetectRenamesIndex = 6;
    const int FromIndex = 7;
    const int ToIndex = 8;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[9];
    private DiffAlgorithm _algorithm;
    private DiffWhitespaceMode _whitespace;

    /// <summary>The commit/tree-ish to compare from. Without it the working tree is compared against the index (or HEAD when Staged). Blank values are ignored.</summary>
    public string? From
    {
        get => arguments[FromIndex].Transform(static v => v.First, static () => default!);
        init => arguments[FromIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    /// <summary>The commit/tree-ish to compare to. Requires From. Blank values are ignored.</summary>
    public string? To
    {
        get => arguments[ToIndex].Transform(static v => v.First, static () => default!);
        init => arguments[ToIndex] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;
    }

    /// <summary>Compares the index against From (default HEAD) instead of the working tree (--cached).</summary>
    public bool Staged
    {
        get => arguments[StagedIndex].HasValue;
        init => arguments[StagedIndex] = value
            ? Optional<Pair>.Some(new Pair(DifConst.CACHED))
            : Optional<Pair>.None;
    }

    /// <summary>Uses the merge base of From and HEAD (or To) as the before side (--merge-base). Requires From.</summary>
    public bool MergeBase
    {
        get => arguments[MergeBaseIndex].HasValue;
        init => arguments[MergeBaseIndex] = value
            ? Optional<Pair>.Some(new Pair(DifConst.MERGE_BASE))
            : Optional<Pair>.None;
    }

    /// <summary>Limits the diff to these paths (or pathspecs).</summary>
    public IReadOnlyList<FilePath>? Paths { get; init; }

    /// <summary>Number of context lines per hunk (--unified); null uses the git default.</summary>
    public int? ContextLines
    {
        get => arguments[ContextLinesIndex].Transform(
            static v => int.TryParse(v.Second, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lines)
                ? lines
                : default,
            static () => default);
        init => arguments[ContextLinesIndex] = value is int lines
            ? Optional<Pair>.Some(new Pair(DifConst.UNIFIED, lines.ToString(CultureInfo.InvariantCulture)))
            : Optional<Pair>.None;
    }

    public DiffAlgorithm Algorithm
    {
        get => _algorithm;
        init
        {
            _algorithm = value;
            arguments[AlgorithmIndex] = value switch
            {
                DiffAlgorithm.Myers => Optional<Pair>.Some(new Pair(DifConst.DIFF_ALGORITHM, DifConst.ALGORITHM_MYERS)),
                DiffAlgorithm.Minimal => Optional<Pair>.Some(new Pair(DifConst.DIFF_ALGORITHM, DifConst.ALGORITHM_MINIMAL)),
                DiffAlgorithm.Patience => Optional<Pair>.Some(new Pair(DifConst.DIFF_ALGORITHM, DifConst.ALGORITHM_PATIENCE)),
                DiffAlgorithm.Histogram => Optional<Pair>.Some(new Pair(DifConst.DIFF_ALGORITHM, DifConst.ALGORITHM_HISTOGRAM)),
                _ => Optional<Pair>.None
            };
        }
    }

    public DiffWhitespaceMode Whitespace
    {
        get => _whitespace;
        init
        {
            _whitespace = value;
            arguments[WhitespaceIndex] = value switch
            {
                DiffWhitespaceMode.IgnoreSpaceChange => Optional<Pair>.Some(new Pair(DifConst.IGNORE_SPACE_CHANGE)),
                DiffWhitespaceMode.IgnoreAllSpace => Optional<Pair>.Some(new Pair(DifConst.IGNORE_ALL_SPACE)),
                _ => Optional<Pair>.None
            };
        }
    }

    /// <summary>Ignores changes whose lines are all blank (--ignore-blank-lines).</summary>
    public bool IgnoreBlankLines
    {
        get => arguments[IgnoreBlankLinesIndex].HasValue;
        init => arguments[IgnoreBlankLinesIndex] = value
            ? Optional<Pair>.Some(new Pair(DifConst.IGNORE_BLANK_LINES))
            : Optional<Pair>.None;
    }

    /// <summary>When true detects renames (--find-renames), when false disables detection (--no-renames); null uses the git default.</summary>
    public bool? DetectRenames
    {
        get => arguments[DetectRenamesIndex].Transform(
            static v => string.Equals(v.First, DifConst.FIND_RENAMES, StringComparison.Ordinal),
            static () => default);
        init => arguments[DetectRenamesIndex] = value is bool detect
            ? Optional<Pair>.Some(new Pair(detect ? DifConst.FIND_RENAMES : DifConst.NO_RENAMES))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(DifConst.CONFIG_OPTION, true);
        argumentBuilder.AddArgument(DifConst.QUOTE_PATH_OFF, true);
        argumentBuilder.AddArgument(Const.DIFF, true);
        argumentBuilder.Flag(DifConst.NO_COLOR);
        argumentBuilder.Flag(DifConst.NO_EXT_DIFF);
        argumentBuilder.Flag(DifConst.NO_TEXTCONV);
        argumentBuilder.AddArgument(
            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, DifConst.SRC_PREFIX, DifConst.OLD_PREFIX), true);
        argumentBuilder.AddArgument(
            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, DifConst.DST_PREFIX, DifConst.NEW_PREFIX), true);
        argumentBuilder.AddArgument(
            string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, DifConst.SUBMODULE, DifConst.SUBMODULE_SHORT), true);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (i >= FromIndex)
                argumentBuilder.AddArgument(pair.First!, true);
            else if (pair.TwoValues)
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
            else
                argumentBuilder.Flag(pair.First!);
        }

        argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);

        var paths = Paths;
        if (paths is null)
            return;

        for (var i = 0; i < paths.Count; i++)
            argumentBuilder.AddArgument(paths[i], true);
    }

    public Result<Nothing, IError> Validate()
    {
        var from = From;
        var to = To;
        var hasFrom = from is not null;
        var hasTo = to is not null;
        var paths = Paths;
        string? error = null;

        if ((hasTo || MergeBase) && !hasFrom)
            error = DifConst.Errors.FROM_REQUIRED;
        else if (Staged && hasTo)
            error = DifConst.Errors.STAGED_WITH_TO;
        else if ((hasFrom && from![0] == Const.Repository.Branching.OPTION_PREFIX)
                 || (hasTo && to![0] == Const.Repository.Branching.OPTION_PREFIX))
            error = DifConst.Errors.COMMIT_INVALID;
        else if (ContextLines is int context && context < 0)
            error = DifConst.Errors.CONTEXT_INVALID;
        else if (paths is not null)
        {
            for (var i = 0; i < paths.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                {
                    error = DifConst.Errors.PATH_INVALID;
                    break;
                }
            }
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_DIFF, error));

        return Result<Nothing, IError>.Some(new());
    }
}