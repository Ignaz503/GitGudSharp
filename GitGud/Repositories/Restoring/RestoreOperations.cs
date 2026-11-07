// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

namespace GitGud.Repositories.Restoring;

public sealed class RestoreOperations : IRestoreOperations
{
    private readonly ICommandExecutor _executor;

    public RestoreOperations(ICommandExecutor executor)
        => _executor = executor;

    public Task<Result<Nothing, IError>> Restore(RestoreOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_RESTORE, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_RESTORE, Const.Repository.Restoring.Errors.RESTORE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }
}
