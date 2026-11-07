// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Repositories.Submodules;

public interface ISubmoduleOperations
{
    Task<Result<Nothing, IError>> Add(SubmoduleAddOptions options, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SubmoduleStatusInfo>, IError>> Status(SubmoduleStatusOptions? options = null, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> Init(SubmoduleInitOptions? options = null, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> Deinit(SubmoduleDeinitOptions options, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> Update(SubmoduleUpdateOptions? options = null, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> SetBranch(SubmoduleSetBranchOptions options, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> SetUrl(SubmoduleSetUrlOptions options, CancellationToken cancellationToken = default);
    Task<Result<string?, IError>> Summary(SubmoduleSummaryOptions? options = null, CancellationToken cancellationToken = default);
    Task<Result<string?, IError>> Foreach(SubmoduleForeachOptions options, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> Sync(SubmoduleSyncOptions? options = null, CancellationToken cancellationToken = default);
    Task<Result<Nothing, IError>> AbsorbGitDirs(SubmoduleAbsorbGitDirsOptions? options = null, CancellationToken cancellationToken = default);
}
