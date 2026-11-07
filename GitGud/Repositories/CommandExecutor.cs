// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap;
using CliWrap.Builders;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Processes;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories;

public sealed class CommandExecutor : ICommandExecutor
{
    private readonly DirectoryPath _path;

    public CommandExecutor(DirectoryPath path)
        => _path = path;

    public async Task<Result<string?, IError>> ExecuteAsync(
        string operation,
        string failureMessage,
        Action<CliWrapArgumentBuilder> buildArguments,
        string? validationError,
        CancellationToken cancellationToken)
    {
        if (validationError is not null)
            return Fail(new RepositoryValidationError(operation, validationError));

        string workingDirectory;
        try
        {
            workingDirectory = System.IO.Path.GetFullPath(_path.ToString());
        }
        catch (Exception)
        {
            return Fail(new RepositoryValidationError(operation, Const.Repository.OperationErrors.REPOSITORY_PATH_INVALID));
        }

        try
        {
            var command = Cli.Wrap(Const.GIT)
                .WithWorkingDirectory(workingDirectory)
                .WithArguments(builder => buildArguments(new CliWrapArgumentBuilder(builder)));

            var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0)
                return Fail(RepositoryErrorParser.Create(operation, failureMessage, result.ExitCode, result.StandardError));

            return Result<string?, IError>.Some(result.StandardOutput);
        }
        catch (OperationCanceledException)
        {
            return Fail(new RepositoryCancelledError(operation, Const.Repository.Errors.CANCELLED));
        }
        catch (Exception)
        {
            return Fail(new RepositoryExecutionError(operation, Const.Repository.OperationErrors.EXECUTION));
        }
    }

    private static Result<string?, IError> Fail(RepositoryError error)
        => Result<string?, IError>.None(error);
}
