// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap.Builders;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Operations;

internal static class ExecutorExtensions
{
    public static async Task<Result<Nothing, IError>> RunAsync(
        this ICommandExecutor executor,
        string operation,
        string failureMessage,
        Action<CliWrapArgumentBuilder> buildArguments,
        string? validationError,
        CancellationToken cancellationToken)
    {
        var result = await executor.ExecuteAsync(operation, failureMessage, buildArguments, validationError, cancellationToken).ConfigureAwait(false);
        if (result.HasNone)
            return Result<Nothing, IError>.None(result.Handle());

        return Result<Nothing, IError>.Some(new Nothing());
    }

    public static void Flag(this CliWrapArgumentBuilder args, string name)
        => args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, name), true);
}
