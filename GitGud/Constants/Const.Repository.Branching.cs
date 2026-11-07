// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string BRANCH = "branch";

    public static partial class Repository
    {
        public const string GIT_BRANCH = $"{GIT} {BRANCH}";

        public static class Branching
        {
            public const string LIST = "list";
            public const string REMOTES = "remotes";
            public const string FORMAT = "format";
            public const string LIST_FORMAT = "%(HEAD)%09%(refname)%09%(objectname)%09%(upstream:short)";
            public const string DELETE_SHORT = "-d";
            public const string DELETE_FORCE_SHORT = "-D";
            public const string MOVE_SHORT = "-m";
            public const string MOVE_FORCE_SHORT = "-M";
            public const string COPY_SHORT = "-c";
            public const string COPY_FORCE_SHORT = "-C";
            public const string FORCE_SHORT = "-f";
            public const string SHOW_CURRENT = "show-current";
            public const string CONTAINS = "contains";
            public const string NO_CONTAINS = "no-contains";
            public const string MERGED = "merged";
            public const string NO_MERGED = "no-merged";
            public const string POINTS_AT = "points-at";
            public const string FORKED = "forked";
            public const string SORT = "sort";
            public const string IGNORE_CASE = "ignore-case";
            public const string TRACK = "track";
            public const string NO_TRACK = "no-track";
            public const string TRACK_DIRECT = "direct";
            public const string TRACK_INHERIT = "inherit";
            public const string RECURSE_SUBMODULES = "recurse-submodules";
            public const string SET_UPSTREAM_TO = "set-upstream-to";
            public const string UNSET_UPSTREAM = "unset-upstream";
            public const string DELETE_MERGED = "delete-merged";
            public const string CHECKOUT = "checkout";
            public const string SWITCH = "switch";
            public const string GIT_SWITCH = $"{GIT} {SWITCH}";
            public const string SWITCH_CREATE_SHORT = "-c";
            public const string SWITCH_FORCE_CREATE_SHORT = "-C";
            public const string DISCARD_CHANGES = "discard-changes";
            public const string GIT_CHECKOUT = $"{GIT} {CHECKOUT}";
            public const string CREATE_SHORT = "-b";
            public const string RESET_SHORT = "-B";
            public const string CREATE_REFLOG_SHORT = "-l";
            public const string MERGE_SHORT = "-m";
            public const string ORPHAN = "orphan";
            public const string DETACH = "detach";
            public const string CONFLICT = "conflict";
            public const string CONFLICT_MERGE = "merge";
            public const string CONFLICT_DIFF3 = "diff3";
            public const string CONFLICT_ZDIFF3 = "zdiff3";
            public const string GUESS = "guess";
            public const string NO_GUESS = "no-guess";
            public const string IGNORE_OTHER_WORKTREES = "ignore-other-worktrees";
            public const string OVERWRITE_IGNORE = "overwrite-ignore";
            public const string NO_OVERWRITE_IGNORE = "no-overwrite-ignore";
            public const string NO_RECURSE_SUBMODULES = "no-recurse-submodules";
            public const string PREVIOUS_BRANCH = "-";
            public const string DRY_RUN = "dry-run";
            public const string DELETED_PREFIX = "Deleted branch ";
            public const string WOULD_DELETE_PREFIX = "Would delete branch ";
            public const string ALL_SHORT = "-a";
            public const string REMOTES_SHORT = "-r";
            public const string HEADS_PREFIX = "refs/heads/";
            public const string REMOTES_PREFIX = "refs/remotes/";
            public const string HEAD_SUFFIX = "/HEAD";
            public const string CURRENT_MARKER = "*";
            public const string FORMAT_REFSPEC = "{0}:{1}";
            public const string FORMAT_HEADS_REF = "refs/heads/{0}";
            public const string FORMAT_REMOTE_TRACKING_REF = "refs/remotes/{0}/{1}";
            public const char FIELD_SEPARATOR = '\t';
            public const char LINE_SEPARATOR = '\n';
            public const char CARRIAGE_RETURN = '\r';
            public const char OPTION_PREFIX = '-';
            public const int FIELD_COUNT = 4;

            public static class Errors
            {
                public const string NAME_REQUIRED = "A branch name is required.";
                public const string NAME_INVALID = "A branch name cannot start with a dash.";
                public const string NEW_NAME_REQUIRED = "A new branch name is required.";
                public const string START_POINT_INVALID = "The start point cannot start with a dash.";
                public const string REMOTE_REQUIRED = "A remote is required.";
                public const string SAME_NAME = "The new branch name must differ from the current name.";
                public const string LIST_FAILED = "Git failed to list the branches.";
                public const string CREATE_FAILED = "Git failed to create the branch.";
                public const string DELETE_FAILED = "Git failed to delete the branch.";
                public const string RENAME_FAILED = "Git failed to rename the branch.";
                public const string COPY_FAILED = "Git failed to copy the branch.";
                public const string UPSTREAM_FAILED = "Git failed to change the branch upstream.";
                public const string CURRENT_FAILED = "Git failed to determine the current branch.";
                public const string DELETE_MERGED_FAILED = "Git failed to delete the merged branches.";
                public const string UPSTREAM_REQUIRED = "An upstream is required.";
                public const string UPSTREAM_PATTERNS_REQUIRED = "At least one upstream pattern is required.";
                public const string PATTERN_INVALID = "A pattern or ref cannot be empty or start with a dash.";
                public const string COMMIT_INVALID = "A commit cannot be empty or start with a dash.";
                public const string SORT_INVALID = "A sort key cannot be empty.";
                public const string CHECKOUT_FAILED = "Git failed to check out.";
                public const string SWITCH_FAILED = "Git failed to switch.";
                public const string ORPHAN_WITH_TARGET = "An orphan branch cannot have a start point.";
                public const string TARGET_REQUIRED = "A branch or commit to check out is required.";
                public const string TARGET_INVALID = "The checkout target cannot start with a dash.";
                public const string DETACH_WITH_CREATE = "Detach cannot be combined with creating a branch.";
                public const string FORCE_WITH_MERGE = "Force and Merge cannot be used together.";
            }
        }
    }
}
