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

public sealed class DeleteBranchOptions : IOperationOptions
{
    const int RemoteTrackingIndex = 0;
    const int NameIndex = 1;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[2];
    private bool _force;

    /// <summary>Branch to delete. Blank values are ignored.</summary>
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

    /// <summary>Delete even if the branch is not fully merged.</summary>
    public bool Force
    {
        get => _force;
        init => _force = value;
    }

    /// <summary>Additional branches to delete in the same call.</summary>
    public IReadOnlyList<Branch>? Names { get; init; }

    /// <summary>Deletes remote-tracking branches instead of local ones (-r).</summary>
    public bool RemoteTracking
    {
        get => arguments[RemoteTrackingIndex].HasValue;
        init => arguments[RemoteTrackingIndex] = value
            ? Optional<Pair>.Some(new Pair(BranchConst.REMOTES_SHORT))
            : Optional<Pair>.None;
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.BRANCH, true);
        argumentBuilder.AddArgument(_force ? BranchConst.DELETE_FORCE_SHORT : BranchConst.DELETE_SHORT, true);

        if (arguments[RemoteTrackingIndex].HasValue)
            argumentBuilder.AddArgument(arguments[RemoteTrackingIndex].Value.First!, true);

        if (arguments[NameIndex].HasValue)
            argumentBuilder.AddArgument(arguments[NameIndex].Value.First!, true);

        var names = Names;
        if (names is null)
            return;

        for (var i = 0; i < names.Count; i++)
            argumentBuilder.AddArgument(names[i].ToString(), true);
    }

    public Result<Nothing, IError> Validate()
    {
        var names = Names;
        var hasName = arguments[NameIndex].HasValue;
        string? error = null;

        if (!hasName && (names is null || names.Count == 0))
            error = BranchConst.Errors.NAME_REQUIRED;

        if (error is null && hasName)
            error = BranchNameRules.Validate(arguments[NameIndex].Value.First, BranchConst.Errors.NAME_REQUIRED);

        if (error is null && names is not null)
        {
            for (var i = 0; i < names.Count && error is null; i++)
                error = BranchNameRules.Validate(names[i].ToString(), BranchConst.Errors.NAME_REQUIRED);
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_BRANCH, error));

        return Result<Nothing, IError>.Some(new());
    }
}