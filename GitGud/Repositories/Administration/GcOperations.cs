// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using System.Threading;

using GcConst = GitGud.Constants.Const.Repository.Gc;

namespace GitGud.Repositories.Administration;

public sealed class GcOperations : IGcOperations
{
    private readonly ICommandExecutor _executor;

    public GcOperations(ICommandExecutor executor)
        => _executor = executor;

    public Task<Result<Nothing, IError>> Gc(GcOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new GcOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_GC, GcConst.Errors.GC_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }
}

