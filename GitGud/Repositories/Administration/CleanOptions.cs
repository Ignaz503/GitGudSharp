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

using CleanConst = GitGud.Constants.Const.Repository.Cleaning;

namespace GitGud.Repositories.Administration;

/// <summary>Description of a <c>git clean</c> call. Only the options that are explicitly set end up as arguments.</summary>
public sealed class CleanOptions : IOperationOptions
{
    const int DirectoriesIndex = 0;
    const int ForceIndex = 1;
    const int DryRunIndex = 2;
    const int QuietIndex = 3;
    const int IncludeIgnoredIndex = 4;
    const int OnlyIgnoredIndex = 5;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[6];

    /// <summary>Recurses into untracked directories (-d).</summary>
    public bool Directories { get => Has(DirectoriesIndex); init => SetFlag(DirectoriesIndex, CleanConst.DIRECTORIES, value); }

    /// <summary>Actually removes files (-f). Required unless <see cref="DryRun"/> is used.</summary>
    public bool Force { get => Has(ForceIndex); init => SetFlag(ForceIndex, CleanConst.FORCE, value); }

    /// <summary>Only shows what would be removed (-n).</summary>
    public bool DryRun { get => Has(DryRunIndex); init => SetFlag(DryRunIndex, CleanConst.DRY_RUN, value); }

    /// <summary>Only reports errors (-q).</summary>
    public bool Quiet { get => Has(QuietIndex); init => SetFlag(QuietIndex, CleanConst.QUIET, value); }

    /// <summary>Also removes ignored files (-x). Cannot be combined with <see cref="OnlyIgnored"/>.</summary>
    public bool IncludeIgnored { get => Has(IncludeIgnoredIndex); init => SetFlag(IncludeIgnoredIndex, CleanConst.INCLUDE_IGNORED, value); }

    /// <summary>Removes only ignored files (-X). Cannot be combined with <see cref="IncludeIgnored"/>.</summary>
    public bool OnlyIgnored { get => Has(OnlyIgnoredIndex); init => SetFlag(OnlyIgnoredIndex, CleanConst.ONLY_IGNORED, value); }

    /// <summary>Exclude patterns (-e).</summary>
    public IReadOnlyList<GlobPattern>? Excludes { get; init; }

    /// <summary>Limits cleaning to these paths (pathspecs).</summary>
    public IReadOnlyList<FilePath>? Paths { get; init; }

    private bool Has(int index)
        => arguments[index].HasValue;

    private void SetFlag(int index, string name, bool value)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(name))
            : Optional<Pair>.None;

    /// <summary>Writes the options only; the executor adds the <c>clean</c> subcommand, as before.</summary>
    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (optional.HasValue)
                argumentBuilder.AddArgument(optional.Value.First!, true);
        }

        var excludes = Excludes;
        if (excludes is not null)
        {
            for (var i = 0; i < excludes.Count; i++)
            {
                argumentBuilder.AddArgument(CleanConst.EXCLUDE, true);
                argumentBuilder.AddArgument(excludes[i].ToString(), true);
            }
        }

        var paths = Paths;
        if (paths is null || paths.Count == 0)
            return;

        argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
        for (var i = 0; i < paths.Count; i++)
            argumentBuilder.AddArgument(paths[i].ToString(), true);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = ValidateExcludes() ?? ValidatePaths();

        if (error is null && !Force && !DryRun)
            error = CleanConst.Errors.FORCE_REQUIRED;
        else if (error is null && IncludeIgnored && OnlyIgnored)
            error = CleanConst.Errors.IGNORED_CONFLICT;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_CLEAN, error));

        return Result<Nothing, IError>.Some(new());
    }

    private string? ValidateExcludes()
    {
        var excludes = Excludes;
        if (excludes is null)
            return null;

        for (var i = 0; i < excludes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(excludes[i].ToString()))
                return CleanConst.Errors.EXCLUDE_INVALID;
        }

        return null;
    }

    private string? ValidatePaths()
    {
        var paths = Paths;
        if (paths is null)
            return null;

        for (var i = 0; i < paths.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(paths[i].ToString()))
                return CleanConst.Errors.PATH_INVALID;
        }

        return null;
    }
}