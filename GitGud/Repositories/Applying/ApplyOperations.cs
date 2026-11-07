// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using ApplyConst = GitGud.Constants.Const.Repository.Applying;

namespace GitGud.Repositories.Applying;

public sealed class ApplyOperations : IApplyOperations
{
    private readonly ICommandExecutor _executor;

    public ApplyOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<string?, IError>> ApplyPatch(ApplyOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Result<string?, IError>.None(new RepositoryValidationError(ApplyConst.GIT_APPLY, Const.Repository.OperationErrors.OPTIONS_REQUIRED));

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<string?, IError>.None(validation.Handle());

        return await _executor.ExecuteAsync(
            ApplyConst.GIT_APPLY, ApplyConst.Errors.APPLY_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);
    }
}
