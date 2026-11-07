// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Repository
    {
        public static class ErrorPatterns
        {
            public const string LOCAL_CHANGES_OVERWRITTEN = "local changes to the following files would be overwritten";
            public const string COMMIT_OR_STASH = "commit your changes or stash them";
            public const string UNSTAGED_CHANGES = "you have unstaged changes";
            public const string UNCOMMITTED_CHANGES = "you have uncommitted changes";
            public const string UNTRACKED_OVERWRITTEN = "untracked working tree files would be overwritten";
            public const string MERGE_CONFLICT = "automatic merge failed";
            public const string CONFLICT = "conflict (";
            public const string DIVERGENT = "need to specify how to reconcile divergent branches";
            public const string NOT_POSSIBLE_FF = "not possible to fast-forward";
            public const string NON_FAST_FORWARD = "non-fast-forward";
            public const string FETCH_FIRST = "fetch first";
            public const string UNRELATED_HISTORIES = "refusing to merge unrelated histories";
            public const string CANNOT_LOCK_REF = "cannot lock ref";
            public const string UNABLE_TO_UPDATE_LOCAL_REF = "unable to update local ref";
            public const string REF_EXISTS_CANNOT_CREATE = "exists; cannot create";
            public const string LOCAL_REFS_NOT_UPDATED = "some local refs could not be updated";
            public const string NOT_A_REPOSITORY = "not a git repository";
            public const string NO_SUCH_REMOTE = "no such remote";
            public const string REMOTE_NOT_FOUND_FATAL = "does not appear to be a git repository";
            public const string REPOSITORY_NOT_FOUND = "repository not found";
            public const string AUTH_FAILED = "authentication failed";
            public const string PERMISSION_DENIED = "permission denied";
            public const string COULD_NOT_READ_USERNAME = "could not read username";
            public const string COULD_NOT_RESOLVE_HOST = "could not resolve host";
            public const string UNABLE_TO_ACCESS = "unable to access";
            public const string CONNECTION_TIMED_OUT = "connection timed out";
            public const string NO_UPSTREAM = "has no upstream branch";
            public const string NO_TRACKING_INFO = "no tracking information";
            public const string REJECTED = "[rejected]";
            public const string NOTHING_TO_COMMIT = "nothing to commit";
            public const string NO_CHANGES_ADDED = "no changes added to commit";
            public const string AUTHOR_IDENTITY_UNKNOWN = "author identity unknown";
            public const string EMPTY_IDENT = "empty ident name";
            public const string INDEX_LOCK = "index.lock";
        }
    }
}
