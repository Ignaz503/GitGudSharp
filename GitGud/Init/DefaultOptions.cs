// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable
using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Init.Errors;
using GitGud.Utility;

namespace GitGud.Init;

public class DefaultOptions : IInitOptions
{
    public DirectoryPath TargetPath { get; init; }

    public bool Quiet { get; init; }
    public bool Bare { get; init; }
    public ObjectFormat ObjectFormat { get; init; }
    public RefStorageFormat RefStorageFormat { get; init; }
    public DirectoryPath? TemplateDirectory { get; init; }
    public DirectoryPath? SeperateGitDir { get; init; }
    public Branch? BranchName { get; init; }
    public RepositorySharing? Sharing { get; init; }

    public Result<Nothing, IError> AddCliArguments<T>(T argBuilder) where T : ICliArgumentBuilder
    {
        if (Quiet)
            argBuilder.AddArgument(new DoubleDashedArgument(Const.Init.QUIET_ARGUMENT), true);
        if (Bare)
            argBuilder.AddArgument(new DoubleDashedArgument(Const.Init.BARE_ARGUMENT), true);

        switch (ObjectFormat)
        {
            case ObjectFormat.SHA1:
                break;
            case ObjectFormat.SHA256:
                argBuilder.AddArgument(new DoubleDashedArgument<string>(Const.Init.OBJECT_FORMAT_ARGUMENT, Const.Init.OBJECT_FORMAT_SHA256), true);
                break;
            default:
                return Result<Nothing, IError>.None(new InitValidationError(InitValidationCode.InvalidObjectFormat));
        }

        switch (RefStorageFormat)
        {
            case RefStorageFormat.Files:
                break;
            case RefStorageFormat.RefTable:
                argBuilder.AddArgument(new DoubleDashedArgument<string>(Const.Init.REF_FORMAT_ARGUMENT, Const.Init.REF_FORMAT_REFTABLE), true);
                break;
            default:
                return Result<Nothing, IError>.None(new InitValidationError(InitValidationCode.InvalidRefStorageFormat));
        }

        if (TemplateDirectory.HasValue)
            argBuilder.AddArgument(new DoubleDashedArgument<DirectoryPath>(Const.Init.TEMPLATE_ARGUMENT, TemplateDirectory.Value), true);
        if (SeperateGitDir.HasValue)
            argBuilder.AddArgument(new DoubleDashedArgument<DirectoryPath>(Const.Init.SEPARATE_GIT_DIR_ARGUMENT, SeperateGitDir.Value), true);
        if (BranchName.HasValue)
            argBuilder.AddArgument(new DoubleDashedArgument<Branch>(Const.Init.INITIAL_BRANCH_ARGUMENT, BranchName.Value), true);
        if (Sharing.HasValue)
            argBuilder.AddArgument(new DoubleDashedArgument<RepositorySharing>(Const.Init.SHARED_ARGUMENT, Sharing.Value), true);

        return Result<Nothing, IError>.Some(new Nothing());
    }
}



