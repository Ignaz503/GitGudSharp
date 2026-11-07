// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Commiting;

public interface ICommitOperations
{
    Task<Result<Nothing, IError>> Add(AddOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> Commit(CommitOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>Applies the changes introduced by existing commits, or continues/skips/aborts/quits an in-progress cherry-pick.</summary>
    Task<Result<Nothing, IError>> CherryPick(CherryPickOptions options, CancellationToken cancellationToken = default);
    /// <summary>Reapplies commits on top of another base tip, or continues/skips/aborts/quits an in-progress rebase.</summary>
    Task<Result<Nothing, IError>> Rebase(RebaseOptions options, CancellationToken cancellationToken = default);

}
