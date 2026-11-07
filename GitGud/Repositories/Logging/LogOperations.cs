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

using LogConst = GitGud.Constants.Const.Repository.Logging;

namespace GitGud.Repositories.Logging;

public sealed class LogOperations : ILogOperations
{
    private readonly ICommandExecutor _executor;

    public LogOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<IReadOnlyList<LogEntry>, IError>> Log(LogOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new LogOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<IReadOnlyList<LogEntry>, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_LOG, LogConst.Errors.LOG_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<LogEntry>, IError>.None(output.Handle());

        return Result<IReadOnlyList<LogEntry>, IError>.Some(LogParser.Parse(output.Unwrap(), options.HasPatch));
    }
}
