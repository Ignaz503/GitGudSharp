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

public sealed class RenameRemoteBranchOptions : IOperationOptions
{
    const int RemoteIndex = 0;
    const int NameIndex = 1;
    const int NewNameIndex = 2;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[3];

    public Remote Remote
    {
        get => new(Get(RemoteIndex));
        init => Set(RemoteIndex, value);
    }

    public Branch Name
    {
        get => new(Get(NameIndex));
        init => Set(NameIndex, value);
    }

    public Branch NewName
    {
        get => new(Get(NewNameIndex));
        init => Set(NewNameIndex, value);
    }

    private string Get(int index)
        => arguments[index].Transform(static v => v.First, static () => default!) ?? string.Empty;

    private void Set(int index, string? value)
        => arguments[index] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(value!))
            : Optional<Pair>.None;

    /// <summary>Writes <c>git push &lt;remote&gt; &lt;remote&gt;/&lt;old&gt;:refs/heads/&lt;new&gt; :refs/heads/&lt;old&gt;</c>.</summary>
    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        var remote = Get(RemoteIndex);
        var oldName = Get(NameIndex);
        var newName = Get(NewNameIndex);

        argumentBuilder.AddArgument(Const.PUSH, true);
        argumentBuilder.AddArgument(remote, true);
        argumentBuilder.AddArgument(string.Format(
            BranchConst.FORMAT_REFSPEC,
            string.Format(BranchConst.FORMAT_REMOTE_TRACKING_REF, remote, oldName),
            string.Format(BranchConst.FORMAT_HEADS_REF, newName)), true);
        argumentBuilder.AddArgument(string.Format(
            BranchConst.FORMAT_REFSPEC,
            string.Empty,
            string.Format(BranchConst.FORMAT_HEADS_REF, oldName)), true);
    }

    public Result<Nothing, IError> Validate()
    {
        var error = BranchNameRules.Validate(arguments[RemoteIndex].Transform(static v => v.First, static () => default!),
                BranchConst.Errors.REMOTE_REQUIRED)
            ?? BranchNameRules.Validate(arguments[NameIndex].Transform(static v => v.First, static () => default!),
                BranchConst.Errors.NAME_REQUIRED)
            ?? BranchNameRules.Validate(arguments[NewNameIndex].Transform(static v => v.First, static () => default!),
                BranchConst.Errors.NEW_NAME_REQUIRED);

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_PUSH, error));

        return Result<Nothing, IError>.Some(new());
    }
}
