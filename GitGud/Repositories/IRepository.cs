// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Repositories.Branching;
using GitGud.Repositories.Administration;
using GitGud.Repositories.Commiting;
using GitGud.Repositories.Diffing;
using GitGud.Repositories.Logging;
using GitGud.Repositories.Merging;
using GitGud.Repositories.Operations;
using GitGud.Repositories.Remotes;
using GitGud.Repositories.Reseting;
using GitGud.Repositories.Restoring;
using GitGud.Repositories.Stashing;
using GitGud.Repositories.Tagging;
using GitGud.Utility;
using GitGud.Repositories.Applying;
using GitGud.Repositories.Submodules;

namespace GitGud.Repositories;

public interface IRepository : IRemoteOperations, ICommitOperations, IBranchOperations, IStashOperations, IMergeOperations, IRestoreOperations, IResetOperations, IDiffOperations, ITagOperations, ILogOperations, ICleanOperations, IGcOperations, IFsCheckOperations, IApplyOperations, ISubmoduleOperations
{
    DirectoryPath Path { get; }

    bool IsBare { get; }
}

