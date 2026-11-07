// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud.Repositories.Errors;

internal delegate RepositoryError RepositoryErrorFactory(string operation, string message, int? exitCode, string? standardError);

internal static class RepositoryErrorParser
{
    private static readonly (string Pattern, RepositoryErrorFactory Create)[] Rules =
    {
        (Const.Repository.ErrorPatterns.INDEX_LOCK, static (o, m, c, s) => new RepositoryIndexLockedError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.LOCAL_CHANGES_OVERWRITTEN, static (o, m, c, s) => new RepositoryLocalChangesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.UNSTAGED_CHANGES, static (o, m, c, s) => new RepositoryLocalChangesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.UNCOMMITTED_CHANGES, static (o, m, c, s) => new RepositoryLocalChangesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.COMMIT_OR_STASH, static (o, m, c, s) => new RepositoryLocalChangesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.UNTRACKED_OVERWRITTEN, static (o, m, c, s) => new RepositoryUntrackedFilesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.UNRELATED_HISTORIES, static (o, m, c, s) => new RepositoryUnrelatedHistoriesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.MERGE_CONFLICT, static (o, m, c, s) => new RepositoryMergeConflictError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.CONFLICT, static (o, m, c, s) => new RepositoryMergeConflictError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.DIVERGENT, static (o, m, c, s) => new RepositoryDivergentBranchesError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NOT_POSSIBLE_FF, static (o, m, c, s) => new RepositoryNotFastForwardError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.CANNOT_LOCK_REF, static (o, m, c, s) => new RepositoryReferenceConflictError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.UNABLE_TO_UPDATE_LOCAL_REF, static (o, m, c, s) => new RepositoryReferenceConflictError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.REF_EXISTS_CANNOT_CREATE, static (o, m, c, s) => new RepositoryReferenceConflictError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.LOCAL_REFS_NOT_UPDATED, static (o, m, c, s) => new RepositoryReferenceConflictError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NOT_A_REPOSITORY, static (o, m, c, s) => new RepositoryNotARepositoryError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NO_SUCH_REMOTE, static (o, m, c, s) => new RepositoryRemoteNotFoundError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.REPOSITORY_NOT_FOUND, static (o, m, c, s) => new RepositoryRemoteNotFoundError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.REMOTE_NOT_FOUND_FATAL, static (o, m, c, s) => new RepositoryRemoteNotFoundError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.AUTH_FAILED, static (o, m, c, s) => new RepositoryAuthenticationError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.PERMISSION_DENIED, static (o, m, c, s) => new RepositoryAuthenticationError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.COULD_NOT_READ_USERNAME, static (o, m, c, s) => new RepositoryAuthenticationError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.COULD_NOT_RESOLVE_HOST, static (o, m, c, s) => new RepositoryNetworkError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.CONNECTION_TIMED_OUT, static (o, m, c, s) => new RepositoryNetworkError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.UNABLE_TO_ACCESS, static (o, m, c, s) => new RepositoryNetworkError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NO_UPSTREAM, static (o, m, c, s) => new RepositoryNoUpstreamError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NO_TRACKING_INFO, static (o, m, c, s) => new RepositoryNoUpstreamError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NON_FAST_FORWARD, static (o, m, c, s) => new RepositoryPushRejectedError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.FETCH_FIRST, static (o, m, c, s) => new RepositoryPushRejectedError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.REJECTED, static (o, m, c, s) => new RepositoryPushRejectedError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.AUTHOR_IDENTITY_UNKNOWN, static (o, m, c, s) => new RepositoryIdentityUnknownError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.EMPTY_IDENT, static (o, m, c, s) => new RepositoryIdentityUnknownError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NOTHING_TO_COMMIT, static (o, m, c, s) => new RepositoryNothingToCommitError(o, m, c, s)),
        (Const.Repository.ErrorPatterns.NO_CHANGES_ADDED, static (o, m, c, s) => new RepositoryNothingToCommitError(o, m, c, s)),
    };

    public static RepositoryError Create(string operation, string message, int? exitCode, string? standardError)
    {
        if (!string.IsNullOrWhiteSpace(standardError))
        {
            var text = standardError!;
            for (var i = 0; i < Rules.Length; i++)
            {
                if (text.IndexOf(Rules[i].Pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    return Rules[i].Create(operation, message, exitCode, standardError);
            }
        }

        return new RepositoryError(operation, message, exitCode, standardError);
    }
}
