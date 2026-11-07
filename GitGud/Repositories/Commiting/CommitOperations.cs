// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Commiting;

public sealed class CommitOperations : ICommitOperations
{
    private readonly ICommandExecutor _executor;

    public CommitOperations(ICommandExecutor executor)
        => _executor = executor;

    public Task<Result<Nothing, IError>> Add(AddOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_ADD, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_ADD, Const.Repository.OperationErrors.ADD_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> Commit(CommitOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_COMMIT, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_COMMIT, Const.Repository.OperationErrors.COMMIT_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> CherryPick(CherryPickOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_CHERRY_PICK, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_CHERRY_PICK, Const.Repository.CherryPickErrors.CHERRY_PICK_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> Rebase(RebaseOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_REBASE, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_REBASE, Const.Repository.RebaseErrors.REBASE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }
}