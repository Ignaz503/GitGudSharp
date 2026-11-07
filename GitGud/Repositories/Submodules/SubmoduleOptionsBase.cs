// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>Shared base: holds the argument slots and emits "--quiet" and the sub command first.</summary>
public abstract class SubmoduleOptionsBase : IOperationOptions
{
    protected const int QuietIndex = 0;

    private protected readonly Optional<Pair>[] arguments;

    private protected SubmoduleOptionsBase(int size)
        => arguments = new Optional<Pair>[size];

    private protected abstract string SubCommand { get; }

    /// <summary>Only print error messages (--quiet).</summary>
    public bool Quiet
    {
        get => arguments[QuietIndex].HasValue;
        init => SetFlag(QuietIndex, value, SubConst.QUIET);
    }

    private protected void SetFlag(int index, bool value, string flag)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(flag))
            : Optional<Pair>.None;

    private protected void SetValue(int index, string? value, string? name = null)
        => arguments[index] = value is string val
            ? Optional<Pair>.Some(name is null ? new Pair(val) : new Pair(name, val))
            : Optional<Pair>.None;

    private protected string? GetValue(int index)
        => arguments[index].Transform(static v => v.Second ?? v.First, static () => default!);

    private protected void SetNumber(int index, int? value, string name)
        => arguments[index] = value is int val
            ? Optional<Pair>.Some(new Pair(name, val.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            : Optional<Pair>.None;

    private protected int? GetNumber(int index)
        => arguments[index].Transform(
            static v => int.TryParse(v.Second, out var n) ? (int?)n : null,
            static () => (int?)null);

    private protected static void AddArgument(CliWrapArgumentBuilder argumentBuilder, Optional<Pair> value)
    {
        if (value.HasValue)
        {
            var pair = value.Value;
            argumentBuilder.AddArgument(pair.First!, true);
            if (pair.TwoValues)
            {
                argumentBuilder.AddArgument(pair.Second!, true);
            }
        }
    }

    private protected void AddRange(CliWrapArgumentBuilder argumentBuilder, int from, int to)
    {
        for (int i = from; i < to; i++)
            AddArgument(argumentBuilder, arguments[i]);
    }

    private protected static void AddPaths(CliWrapArgumentBuilder argumentBuilder, IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
            return;

        argumentBuilder.AddArgument(SubConst.PATH_SEPARATOR, true);
        for (var i = 0; i < paths.Count; i++)
            argumentBuilder.AddArgument(paths[i], true);
    }

    private protected abstract void ApplyBody(CliWrapArgumentBuilder argumentBuilder);

    private protected abstract string? ValidateBody();

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        AddArgument(argumentBuilder, arguments[QuietIndex]);
        argumentBuilder.AddArgument(SubCommand, true);
        ApplyBody(argumentBuilder);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = ValidateBody();
        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(SubConst.GIT_SUBMODULE, error));

        return Result<Nothing, IError>.Some(new());
    }

    private protected static bool IsInvalid(string? value)
        => string.IsNullOrWhiteSpace(value) || value![0] == SubConst.OPTION_PREFIX;

    private protected static string? ValidatePaths(IReadOnlyList<string> paths)
    {
        for (var i = 0; i < paths.Count; i++)
        {
            if (IsInvalid(paths[i]))
                return SubConst.Errors.PATH_INVALID;
        }

        return null;
    }
}
