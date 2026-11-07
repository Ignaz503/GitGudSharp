// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Operations;
using GitGud.Utility;

namespace GitGud.Repositories.Remotes;

public sealed class RemoteOperations : IRemoteOperations
{
    private readonly ICommandExecutor _executor;

    public RemoteOperations(ICommandExecutor executor)
        => _executor = executor;

    public Task<Result<Nothing, IError>> Fetch(FetchOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FetchOptions();
        return Run(Const.Repository.GIT_FETCH, Const.Repository.OperationErrors.FETCH_FAILED, options.Validate(), options.ApplyOptionsToInvoke, cancellationToken);
    }

    public Task<Result<Nothing, IError>> Pull(PullOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PullOptions();
        return Run(Const.Repository.GIT_PULL, Const.Repository.OperationErrors.PULL_FAILED, options.Validate(), options.ApplyOptionsToInvoke, cancellationToken);
    }

    public Task<Result<Nothing, IError>> Push(PushOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PushOptions();
        return Run(Const.Repository.GIT_PUSH, Const.Repository.OperationErrors.PUSH_FAILED, options.Validate(), options.ApplyOptionsToInvoke, cancellationToken);
    }

    private Task<Result<Nothing, IError>> Run(
        string command,
        string failureMessage,
        Result<Nothing, IError> validation,
        Action<CliWrapArgumentBuilder> apply,
        CancellationToken cancellationToken)
    {
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(command, failureMessage, apply, null, cancellationToken);
    }
}
