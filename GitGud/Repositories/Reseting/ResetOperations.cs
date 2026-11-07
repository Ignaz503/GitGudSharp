// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Reseting;

public sealed class ResetOperations : IResetOperations
{
    private readonly ICommandExecutor _executor;

    public ResetOperations(ICommandExecutor executor)
        => _executor = executor;

    public Task<Result<Nothing, IError>> Reset(ResetOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ResetOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_RESET, Const.Repository.Resetting.Errors.RESET_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }
}
