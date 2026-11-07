// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Stashing;

public interface IStashOperations
{
    Task<Result<IReadOnlyList<StashInfo>, IError>> ListStashes(CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> Stash(StashPushOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> ApplyStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> PopStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> DropStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> ClearStashes(CancellationToken cancellationToken = default);

    /// <summary>Creates a branch from the commit the stash was made on, applies the stash and drops it on success.</summary>
    Task<Result<Nothing, IError>> CreateBranchFromStash(StashBranchOptions options, CancellationToken cancellationToken = default);
}
