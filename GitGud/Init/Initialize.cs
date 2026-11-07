// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap;

using GitGud;
using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Init.Errors;
using GitGud.Processes;
using GitGud.Repositories;
using GitGud.Utility;

namespace GitGud.Init;

public interface IInit 
{
    Task<Result<IRepository, IError>> Init(IInitOptions options);
}

public class Initialize : IInit
{
    public async Task<Result<IRepository, IError>> Init(IInitOptions options) 
    {
        if (options is null)
            return Result<IRepository, IError>.None(new InitValidationError(InitValidationCode.OptionsRequired));

        string targetPath;
        try
        {
            targetPath = options.TargetPath.ToString();
            if (string.IsNullOrWhiteSpace(targetPath))
                return Result<IRepository, IError>.None(new InitValidationError(InitValidationCode.TargetPathRequired));

            targetPath = System.IO.Path.GetFullPath(targetPath);
        }
        catch (Exception)
        {
            return Result<IRepository, IError>.None(new InitValidationError(InitValidationCode.InvalidTargetPath));
        }

        try
        {
            IError? optionsError = null;
            var command = Cli.Wrap(Const.GIT)
                .WithArguments(argsI =>
                {
                    var args = new CliWrapArgumentBuilder(argsI);
                    args.AddArgument(Const.INIT, true);
                    var argumentResult = options.AddCliArguments(args);
                    if (argumentResult.HasNone)
                    {
                        optionsError = argumentResult.Handle();
                        return;
                    }
                    args.AddArgument(targetPath, true);
                });

            if (optionsError is not null)
                return Result<IRepository, IError>.None(optionsError);

            var result = await ProcessRunner.ExecuteAsync(command).ConfigureAwait(false);
            if (result.ExitCode != 0)
                return Result<IRepository, IError>.None(new InitProcessError(Const.Init.Errors.FAILED, result.ExitCode, result.StandardError));

            return Result<IRepository, IError>.Some(new Repository(new DirectoryPath(targetPath), options.Bare));
        }
        catch (Exception)
        {
            return Result<IRepository, IError>.None(new InitExecutionError());
        }
    }
}

