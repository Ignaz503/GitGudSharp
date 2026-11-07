// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using CleanConst = GitGud.Constants.Const.Repository.Cleaning;

namespace GitGud.Repositories.Administration;

public sealed class CleanOperations : ICleanOperations
{
    private readonly ICommandExecutor _executor;

    public CleanOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<IReadOnlyList<string>, IError>> Clean(CleanOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Result<IReadOnlyList<string>, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_CLEAN, Const.Repository.OperationErrors.OPTIONS_REQUIRED));

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<IReadOnlyList<string>, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_CLEAN, CleanConst.Errors.CLEAN_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<string>, IError>.None(output.Handle());

        return Result<IReadOnlyList<string>, IError>.Some(ParseLines(output.Unwrap()));
    }

    private static IReadOnlyList<string> ParseLines(string? output)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(output))
            return lines;

        var parts = output!.Split('\n');
        for (var i = 0; i < parts.Length; i++)
        {
            var line = parts[i].TrimEnd('\r');
            if (line.Length > 0)
                lines.Add(line);
        }

        return lines;
    }
}
