// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Repository
    {
        public const string GIT_MERGE = $"{GIT} {Merging.MERGE}";
        public const string GIT_MERGE_CONFLICTS = $"{GIT} {Merging.DIFF}";
        public const string GIT_MERGE_STAGES = $"{GIT} {Merging.CHECKOUT_INDEX}";
        public const string GIT_MERGE_REMOVE = $"{GIT} {Merging.REMOVE}";

        public static class Merging
        {
            public const string MERGE = "merge";
            public const string DIFF = "diff";
            public const string CHECKOUT_INDEX = "checkout-index";
            public const string RESET = "reset";
            public const string REMOVE = "rm";
            public const string NO_FF = "no-ff";
            public const string SQUASH = "squash";
            public const string NO_COMMIT = "no-commit";
            public const string ABORT = "abort";
            public const string NAME_ONLY = "name-only";
            public const string RELATIVE = "relative";
            public const string DIFF_FILTER_UNMERGED = "diff-filter=U";
            public const string STAGE_ALL = "stage=all";
            public const string TEMP = "temp";
            public const string NULL_SEPARATOR_SHORT = "-z";
            public const string FORCE_SHORT = "-f";
            public const string MISSING_STAGE = ".";
            public const char NULL_SEPARATOR = '\0';
            public const char FIELD_SEPARATOR = '\t';
            public const char STAGE_SEPARATOR = ' ';
            public const char OPTION_PREFIX = '-';
            public const int STAGE_COUNT = 3;

            public static class Errors
            {
                public const string BRANCH_REQUIRED = "A branch to merge is required.";
                public const string BRANCH_INVALID = "A branch name cannot start with a dash.";
                public const string MERGE_FAILED = "Git failed to merge.";
                public const string ABORT_FAILED = "Git failed to abort the merge.";
                public const string CONFLICTS_FAILED = "Git failed to list the merge conflicts.";
                public const string STAGES_FAILED = "Git failed to extract the conflicting versions.";
                public const string STAGE_PARSE_FAILED = "The conflicting versions could not be read.";
                public const string RESOLVE_FAILED = "Git failed to stage the resolved file.";
                public const string CONFLICTS = "The merge stopped because of conflicts.";
                public const string MERGE_ABORTED = "The merge was aborted during conflict resolution.";
                public const string RESOLUTION_PATH_INVALID = "The resolved file does not exist.";
                public const string RESOLUTION_IO = "The resolved content could not be written.";
                public const string RESOLVER_FAILED = "The conflict resolver failed.";
            }
        }
    }
}
