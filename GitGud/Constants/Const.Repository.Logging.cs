// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string LOG = "log";

    public static partial class Repository
    {
        public const string GIT_LOG = $"{GIT} {LOG}";

        public static class Logging
        {
            public const string CONFIG_OPTION = "-c";
            public const string QUOTE_PATH_OFF = "core.quotepath=false";
            public const string FORMAT = "format";
            public const string DECORATE_FULL = "decorate=full";
            public const string NO_COLOR = "no-color";
            public const string NO_EXT_DIFF = "no-ext-diff";
            public const string NO_PATCH = "no-patch";
            public const string NAME_STATUS = "name-status";
            public const string FOLLOW = "follow";
            public const string MAX_COUNT = "max-count";
            public const string SKIP = "skip";
            public const string SINCE = "since";
            public const string UNTIL = "until";
            public const string AUTHOR = "author";
            public const string COMMITTER = "committer";
            public const string GREP = "grep";
            public const string ALL_MATCH = "all-match";
            public const string INVERT_GREP = "invert-grep";
            public const string IGNORE_CASE = "regexp-ignore-case";
            public const string EXTENDED_REGEXP = "extended-regexp";
            public const string FIXED_STRINGS = "fixed-strings";
            public const string PERL_REGEXP = "perl-regexp";
            public const string MERGES = "merges";
            public const string NO_MERGES = "no-merges";
            public const string MIN_PARENTS = "min-parents";
            public const string MAX_PARENTS = "max-parents";
            public const string FIRST_PARENT = "first-parent";
            public const string ALL = "all";
            public const string BRANCHES = "branches";
            public const string TAGS = "tags";
            public const string REMOTES = "remotes";
            public const string DATE_ORDER = "date-order";
            public const string AUTHOR_DATE_ORDER = "author-date-order";
            public const string TOPO_ORDER = "topo-order";
            public const string REVERSE = "reverse";
            public const string FULL_HISTORY = "full-history";
            public const string SIMPLIFY_BY_DECORATION = "simplify-by-decoration";
            public const string ANCESTRY_PATH = "ancestry-path";
            public const string PICKAXE_STRING = "-S";
            public const string PICKAXE_REGEX = "-G";
            public const string DIFF_FILTER = "diff-filter";
            public const string PATCH = "patch";
            public const string MERGE_DIFFS = "-m";
            public const string NOT = "--not";
            public const string RANGE_FORMAT = "{0}..{1}";
            public const string SYMMETRIC_RANGE_FORMAT = "{0}...{1}";
            public const string EXCLUDE_FORMAT = "^{0}";
            public const string LINE_RANGE_FORMAT = "-L{0}:{1}";
            public const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";

            public const string LOG_FORMAT = "%x1e%H%x1f%P%x1f%an%x1f%ae%x1f%aI%x1f%cn%x1f%ce%x1f%cI%x1f%D%x1f%s%x1f%b%x1f";
            public const char RECORD_SEPARATOR = '\u001e';
            public const char FIELD_SEPARATOR = '\u001f';
            public const char LINE_SEPARATOR = '\n';
            public const char STATUS_SEPARATOR = '\t';
            public const char PARENT_SEPARATOR = ' ';
            public const int FIELD_COUNT = 12;
            public const string DECORATION_SEPARATOR = ", ";
            public const string HEAD = "HEAD";
            public const string HEAD_ARROW = "HEAD -> ";
            public const string TAG_PREFIX = "tag: ";
            public const string REFS_HEADS = "refs/heads/";
            public const string REFS_REMOTES = "refs/remotes/";
            public const string REFS_TAGS = "refs/tags/";
            public const char REMOTE_SEPARATOR = '/';

            public static class Errors
            {
                public const string LOG_FAILED = "Failed to read the log.";
                public const string VALUE_INVALID = "A log option value is invalid.";
                public const string NUMBER_INVALID = "Log numeric limits must not be negative.";
                public const string FOLLOW_SINGLE_PATH = "Follow requires exactly one path.";
                public const string PATCH_CONFLICT = "Patch output cannot be combined with file changes.";
                public const string MERGE_DIFFS_REQUIRES_PATCH = "Merge diffs require a patch.";
                public const string LINE_RANGE_INVALID = "Line ranges must have a range and a path and cannot be combined with paths.";
                public const string DIFF_FILTER_INVALID = "Diff filter contains an unknown change kind.";
            }
        }
    }
}
