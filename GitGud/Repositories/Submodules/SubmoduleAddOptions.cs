// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule add</summary>
public sealed class SubmoduleAddOptions : SubmoduleOptionsBase
{
    const int BranchIndex = 1;
    const int ForceIndex = 2;
    const int NameIndex = 3;
    const int ReferenceIndex = 4;
    const int DepthIndex = 5;
    const int RepositoryIndex = 6;
    const int PathIndex = 7;

    public SubmoduleAddOptions() : base(8) { }

    private protected override string SubCommand => SubConst.ADD;

    /// <summary>Repository URL to add (required).</summary>
    public string? Repository
    {
        get => GetValue(RepositoryIndex);
        init => SetValue(RepositoryIndex, value);
    }

    /// <summary>Relative location for the submodule.</summary>
    public string? Path
    {
        get => GetValue(PathIndex);
        init => SetValue(PathIndex, value);
    }

    /// <summary>Branch to track (-b).</summary>
    public string? Branch
    {
        get => GetValue(BranchIndex);
        init => SetValue(BranchIndex, value, "-b");
    }

    /// <summary>Allows adding an otherwise ignored path (--force).</summary>
    public bool Force
    {
        get => arguments[ForceIndex].HasValue;
        init => SetFlag(ForceIndex, value, SubConst.FORCE);
    }

    /// <summary>Logical name of the submodule (--name).</summary>
    public string? Name
    {
        get => GetValue(NameIndex);
        init => SetValue(NameIndex, value, SubConst.NAME);
    }

    /// <summary>Reference repository (--reference).</summary>
    public string? Reference
    {
        get => GetValue(ReferenceIndex);
        init => SetValue(ReferenceIndex, value, SubConst.REFERENCE);
    }

    /// <summary>Shallow clone depth (--depth).</summary>
    public int? Depth
    {
        get => GetNumber(DepthIndex);
        init => SetNumber(DepthIndex, value, SubConst.DEPTH);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        AddRange(argumentBuilder, BranchIndex, RepositoryIndex);
        argumentBuilder.AddArgument(SubConst.PATH_SEPARATOR, true);
        AddRange(argumentBuilder, RepositoryIndex, arguments.Length);
    }

    private protected override string? ValidateBody()
    {
        if (!arguments[RepositoryIndex].HasValue || IsInvalid(Repository))
            return SubConst.Errors.REPOSITORY_INVALID;
        if (arguments[PathIndex].HasValue && IsInvalid(Path))
            return SubConst.Errors.PATH_INVALID;
        if ((arguments[BranchIndex].HasValue && string.IsNullOrWhiteSpace(Branch))
            || (arguments[NameIndex].HasValue && string.IsNullOrWhiteSpace(Name))
            || (arguments[ReferenceIndex].HasValue && string.IsNullOrWhiteSpace(Reference)))
            return SubConst.Errors.VALUE_INVALID;
        if (Depth is int depth && depth <= 0)
            return SubConst.Errors.NUMBER_INVALID;

        return null;
    }
}
