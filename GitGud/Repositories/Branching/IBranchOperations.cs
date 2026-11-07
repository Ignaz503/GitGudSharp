// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Repositories.Merging;
using GitGud.Repositories.Remotes;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Branching;

public interface IBranchOperations
{
    Task<Result<IReadOnlyList<BranchInfo>, IError>> ListBranches(ListBranchesOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> CreateBranch(CreateBranchOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> DeleteBranch(DeleteBranchOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> DeleteRemoteBranch(DeleteRemoteBranchOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> RenameBranch(RenameBranchOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> RenameRemoteBranch(RenameRemoteBranchOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> CopyBranch(CopyBranchOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> SetUpstream(SetUpstreamOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> UnsetUpstream(UnsetUpstreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the current branch; the result is null when HEAD is detached.</summary>
    Task<Result<Branch?, IError>> GetCurrentBranch(CancellationToken cancellationToken = default);

    /// <summary>Deletes local branches already merged into their upstream; with DryRun only reports them. Returns the affected branches.</summary>
    Task<Result<IReadOnlyList<Branch>, IError>> DeleteMergedBranches(DeleteMergedOptions options, CancellationToken cancellationToken = default);

    /// <summary>Switches to a branch or commit, optionally creating the branch first (git checkout).</summary>
    Task<Result<Nothing, IError>> CheckoutBranch(CheckoutBranchOptions options, CancellationToken cancellationToken = default);

    /// <summary>Switches to a branch or commit, optionally creating the branch first (git switch).</summary>
    Task<Result<Nothing, IError>> SwitchBranch(SwitchBranchOptions options, CancellationToken cancellationToken = default);
}
