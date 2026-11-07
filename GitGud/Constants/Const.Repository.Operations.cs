// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string FETCH = "fetch";
    public const string PULL = "pull";
    public const string ADD = "add";
    public const string COMMIT = "commit";
    public const string PUSH = "push";

    public static partial class Repository
    {
        public const string GIT_FETCH = $"{GIT} {FETCH}";
        public const string GIT_PULL = $"{GIT} {PULL}";
        public const string GIT_ADD = $"{GIT} {ADD}";
        public const string GIT_COMMIT = $"{GIT} {COMMIT}";
        public const string GIT_PUSH = $"{GIT} {PUSH}";

        public static class Arguments
        {
            public const string ALL = "all";
            public const string PRUNE = "prune";
            public const string TAGS = "tags";
            public const string DEPTH = "depth";
            public const string REBASE = "rebase";
            public const string FF_ONLY = "ff-only";
            public const string UPDATE = "update";
            public const string AMEND = "amend";
            public const string ALLOW_EMPTY = "allow-empty";
            public const string NO_EDIT = "no-edit";
            public const string SET_UPSTREAM = "set-upstream";
            public const string FORCE_WITH_LEASE = "force-with-lease";
            public const string DELETE = "delete";
            public const string MESSAGE_SHORT = "-m";
            public const string ALL_SHORT = "-a";
            public const string ADD_ALL_SHORT = "-A";
            public const string UPDATE_SHORT = "-u";
        }

        public static class OperationErrors
        {
            public const string OPTIONS_REQUIRED = "Options cannot be null.";
            public const string REMOTE_REQUIRED = "A remote is required when a branch is specified.";
            public const string PATHS_REQUIRED = "At least one path, or All/Update, is required.";
            public const string PATH_INVALID = "A path cannot be empty.";
            public const string MESSAGE_REQUIRED = "A commit message is required.";
            public const string REPOSITORY_PATH_INVALID = "The repository path is invalid.";
            public const string FETCH_FAILED = "Git failed to fetch.";
            public const string PULL_FAILED = "Git failed to pull.";
            public const string ADD_FAILED = "Git failed to add the files.";
            public const string COMMIT_FAILED = "Git failed to commit.";
            public const string PUSH_FAILED = "Git failed to push.";
            public const string EXECUTION = "The git operation could not be executed.";
        }
    }
}
