// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{

    public static partial class Repository
    {
        public static class Submodules
        {
            public const string GIT_SUBMODULE = "submodule";

            public const string ADD = "add";
            public const string STATUS = "status";
            public const string INIT_COMMAND = "init";
            public const string DEINIT = "deinit";
            public const string UPDATE = "update";
            public const string SET_BRANCH = "set-branch";
            public const string SET_URL = "set-url";
            public const string SUMMARY = "summary";
            public const string FOREACH = "foreach";
            public const string SYNC = "sync";
            public const string ABSORB_GIT_DIRS = "absorbgitdirs";

            public const string QUIET = "--quiet";
            public const string CACHED = "--cached";
            public const string FILES = "--files";
            public const string RECURSIVE = "--recursive";
            public const string FORCE = "--force";
            public const string ALL = "--all";
            public const string INIT = "--init";
            public const string REMOTE = "--remote";
            public const string NO_FETCH = "--no-fetch";
            public const string CHECKOUT = "--checkout";
            public const string REBASE = "--rebase";
            public const string MERGE = "--merge";
            public const string BRANCH = "--branch";
            public const string DEFAULT = "--default";
            public const string NAME = "--name";
            public const string REFERENCE = "--reference";
            public const string DEPTH = "--depth";
            public const string JOBS = "--jobs";
            public const string SUMMARY_LIMIT = "--summary-limit";
            public const string PATH_SEPARATOR = "--";

            public const char OPTION_PREFIX = '-';
            public const char LINE_SEPARATOR = '\n';
            public const char FIELD_SEPARATOR = ' ';
            public const int STATUS_MIN_LENGTH = 41;
            public const int STATUS_FIELD_COUNT = 3;

            public static class Errors
            {
                public const string REPOSITORY_INVALID = "The submodule repository must not be empty or start with '-'.";
                public const string PATH_INVALID = "The submodule path must not be empty or start with '-'.";
                public const string PATH_REQUIRED = "A submodule path is required.";
                public const string PATHS_REQUIRED = "At least one submodule path or --all is required.";
                public const string PATHS_CONFLICT = "Paths cannot be combined with --all.";
                public const string VALUE_INVALID = "A submodule option value is invalid.";
                public const string URL_INVALID = "The submodule url must not be empty or start with '-'.";
                public const string BRANCH_CONFLICT = "Either a branch or default must be specified, but not both.";
                public const string COMMAND_REQUIRED = "A command is required.";
                public const string MODE_CONFLICT = "Summary cannot combine cached and files, or files with a commit.";
                public const string NUMBER_INVALID = "Numeric values must be greater than zero.";

                public const string ADD_FAILED = "Failed to add the submodule.";
                public const string STATUS_FAILED = "Failed to get the submodule status.";
                public const string INIT_FAILED = "Failed to initialize the submodules.";
                public const string DEINIT_FAILED = "Failed to deinitialize the submodules.";
                public const string UPDATE_FAILED = "Failed to update the submodules.";
                public const string SET_BRANCH_FAILED = "Failed to set the submodule branch.";
                public const string SET_URL_FAILED = "Failed to set the submodule url.";
                public const string SUMMARY_FAILED = "Failed to get the submodule summary.";
                public const string FOREACH_FAILED = "Failed to run the command in the submodules.";
                public const string SYNC_FAILED = "Failed to synchronize the submodules.";
                public const string ABSORB_FAILED = "Failed to absorb the submodule git directories.";
            }
        }

    }
}