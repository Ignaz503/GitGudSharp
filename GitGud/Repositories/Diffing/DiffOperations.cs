// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using System.Threading;

using DifConst = GitGud.Constants.Const.Repository.Diffing;

namespace GitGud.Repositories.Diffing;

public sealed class DiffOperations : IDiffOperations
{
    private readonly ICommandExecutor _executor;

    public DiffOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<DiffResult, IError>> Diff(DiffOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new DiffOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<DiffResult, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_DIFF, DifConst.Errors.DIFF_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<DiffResult, IError>.None(output.Handle());

        return Result<DiffResult, IError>.Some(DiffParser.Parse(output.Unwrap()));
    }
}
