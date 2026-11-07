// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string STASH = "stash";

    public static partial class Repository
    {
        public const string GIT_STASH = $"{GIT} {STASH}";

        public static class Stashing
        {
            public const string PUSH = "push";
            public const string LIST = "list";
            public const string APPLY = "apply";
            public const string POP = "pop";
            public const string DROP = "drop";
            public const string CLEAR = "clear";
            public const string BRANCH = "branch";
            public const string INCLUDE_UNTRACKED = "include-untracked";
            public const string ALL = "all";
            public const string KEEP_INDEX = "keep-index";
            public const string INDEX = "index";
            public const string FORMAT = "format";
            public const string LIST_FORMAT = "%gd%09%H%09%gs";
            public const string FORMAT_STASH_REF = "stash@{{{0}}}";
            public const string REF_PREFIX = "stash@{";
            public const char REF_SUFFIX = '}';
            public const char FIELD_SEPARATOR = '\t';
            public const char LINE_SEPARATOR = '\n';
            public const char CARRIAGE_RETURN = '\r';
            public const int FIELD_COUNT = 3;

            public static class Errors
            {
                public const string INDEX_INVALID = "The stash index cannot be negative.";
                public const string PATH_INVALID = "A path cannot be empty.";
                public const string LIST_FAILED = "Git failed to list the stashes.";
                public const string PUSH_FAILED = "Git failed to stash the changes.";
                public const string APPLY_FAILED = "Git failed to apply the stash.";
                public const string POP_FAILED = "Git failed to pop the stash.";
                public const string DROP_FAILED = "Git failed to drop the stash.";
                public const string CLEAR_FAILED = "Git failed to clear the stashes.";
                public const string BRANCH_FAILED = "Git failed to create a branch from the stash.";
            }
        }
    }
}
