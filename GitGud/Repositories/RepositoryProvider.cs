// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Processes;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories;

public sealed class RepositoryProvider
{
    public async Task<Result<IRepository, IError>> Clone(ICloneOptions options, CancellationToken cancellationToken = default)
    {
        const string operation = Const.Repository.GIT_CLONE;
        if (options is null)
            return Fail(operation, Const.Repository.Errors.OPTIONS_REQUIRED);

        var source = options.Source?.Location;
        if (string.IsNullOrWhiteSpace(source))
            return Fail(operation, Const.Repository.Errors.SOURCE_REQUIRED);

        if (options.Depth is <= 0)
            return Fail(operation, Const.Repository.Errors.DEPTH_INVALID);

        var submodules = options.Submodules;
        if (submodules is not null)
        {
            for (var i = 0; i < submodules.Count; i++)
            {
                if (!submodules[i].IsValid)
                    return Fail(operation, Const.Repository.Errors.SUBMODULE_INVALID);
            }
        }

        string targetPath;
        try
        {
            targetPath = options.Into.ToString();
            if (string.IsNullOrWhiteSpace(targetPath))
                return Fail(operation, Const.Repository.Errors.TARGET_PATH_REQUIRED);

            targetPath = System.IO.Path.GetFullPath(targetPath);
        }
        catch (Exception)
        {
            return Fail(operation, Const.Repository.Errors.TARGET_PATH_INVALID);
        }

        try
        {
            var command = Cli.Wrap(Const.GIT)
                .WithArguments(argsI =>
                {
                    var args = new CliWrapArgumentBuilder(argsI);
                    args.AddArgument(Const.CLONE, true);
                    if (options.Bare)
                        args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, Const.Repository.BARE_ARGUMENT), true);
                    if (!string.IsNullOrWhiteSpace(options.Branch))
                    {
                        args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, Const.Repository.BRANCH_ARGUMENT), true);
                        args.AddArgument(options.Branch!, true);
                    }
                    if (options.Depth is int depth)
                        args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, Const.Repository.DEPTH_ARGUMENT, depth), true);

                    var hasSubmodules = submodules is not null && submodules.Count > 0;
                    if (hasSubmodules)
                    {
                        for (var i = 0; i < submodules!.Count; i++)
                            args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, Const.Repository.RECURSE_SUBMODULES_ARGUMENT, submodules[i].Path), true);
                    }
                    else if (options.RecurseSubmodules)
                        args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, Const.Repository.RECURSE_SUBMODULES_ARGUMENT), true);

                    if (options.ShallowSubmodules && (hasSubmodules || options.RecurseSubmodules))
                        args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, Const.Repository.SHALLOW_SUBMODULES_ARGUMENT), true);

                    args.AddArgument(Const.Repository.END_OF_OPTIONS, true);
                    args.AddArgument(source!, true);
                    args.AddArgument(targetPath, true);
                });

            var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0)
                return Result<IRepository, IError>.None(RepositoryErrorParser.Create(operation, Const.Repository.Errors.CLONE_FAILED, result.ExitCode, result.StandardError));

            return Result<IRepository, IError>.Some(new Repository(new DirectoryPath(targetPath), options.Bare));
        }
        catch (OperationCanceledException)
        {
            return Result<IRepository, IError>.None(new RepositoryCancelledError(operation, Const.Repository.Errors.CANCELLED));
        }
        catch (Exception)
        {
            return Result<IRepository, IError>.None(new RepositoryExecutionError(operation, Const.Repository.Errors.CLONE_EXECUTION));
        }
    }

    public async Task<Result<IRepository, IError>> Open(DirectoryPath path, CancellationToken cancellationToken = default)
    {
        const string operation = Const.Repository.GIT_OPEN;

        string fullPath;
        try
        {
            fullPath = path.ToString();
            if (string.IsNullOrWhiteSpace(fullPath))
                return Fail(operation, Const.Repository.Errors.TARGET_PATH_REQUIRED);

            fullPath = System.IO.Path.GetFullPath(fullPath);
        }
        catch (Exception)
        {
            return Fail(operation, Const.Repository.Errors.TARGET_PATH_INVALID);
        }

        if (!System.IO.Directory.Exists(fullPath))
            return Fail(operation, Const.Repository.Errors.OPEN_PATH_NOT_FOUND);

        try
        {
            var command = Cli.Wrap(Const.GIT)
                .WithArguments(argsI =>
                {
                    var args = new CliWrapArgumentBuilder(argsI);
                    args.AddArgument(Const.Repository.WORKING_DIRECTORY_ARGUMENT, true);
                    args.AddArgument(fullPath, true);
                    args.AddArgument(Const.REV_PARSE, true);
                    args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, Const.Repository.IS_BARE_ARGUMENT), true);
                });

            var result = await ProcessRunner.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0)
                return Result<IRepository, IError>.None(new RepositoryNotARepositoryError(operation, Const.Repository.Errors.OPEN_NOT_A_REPOSITORY, result.ExitCode, result.StandardError));
            var isBare = string.Equals(result.StandardOutput?.Trim(), Const.Repository.TRUE, StringComparison.OrdinalIgnoreCase);
            return Result<IRepository, IError>.Some(new Repository(new DirectoryPath(fullPath), isBare));
        }
        catch (OperationCanceledException)
        {
            return Result<IRepository, IError>.None(new RepositoryCancelledError(operation, Const.Repository.Errors.CANCELLED));
        }
        catch (Exception)
        {
            return Result<IRepository, IError>.None(new RepositoryExecutionError(operation, Const.Repository.Errors.OPEN_EXECUTION));
        }
    }

    private static Result<IRepository, IError> Fail(string operation, string message)
        => Result<IRepository, IError>.None(new RepositoryValidationError(operation, message));
}
