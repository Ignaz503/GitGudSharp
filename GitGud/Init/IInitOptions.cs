// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.CliArguments;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Init;

public interface IInitOptions : ICliParameters
{
    DirectoryPath TargetPath { get; init; }

    bool Quiet { get; init; }

    bool Bare { get; init; }

    ObjectFormat ObjectFormat { get; init; }

    RefStorageFormat RefStorageFormat { get; init; }

    DirectoryPath? TemplateDirectory {  get; init; }

    DirectoryPath? SeperateGitDir { get; init; }

    Branch? BranchName { get; init; }

    RepositorySharing? Sharing { get; init; }

}

