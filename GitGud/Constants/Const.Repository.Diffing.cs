// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string DIFF = "diff";

    public static partial class Repository
    {
        public const string GIT_DIFF = $"{GIT} {DIFF}";

        public static class Diffing
        {
            public const string CONFIG_OPTION = "-c";
            public const string QUOTE_PATH_OFF = "core.quotepath=false";
            public const string NO_COLOR = "no-color";
            public const string NO_EXT_DIFF = "no-ext-diff";
            public const string NO_TEXTCONV = "no-textconv";
            public const string CACHED = "cached";
            public const string MERGE_BASE = "merge-base";
            public const string UNIFIED = "unified";
            public const string DIFF_ALGORITHM = "diff-algorithm";
            public const string SRC_PREFIX = "src-prefix";
            public const string DST_PREFIX = "dst-prefix";
            public const string SUBMODULE = "submodule";
            public const string SUBMODULE_SHORT = "short";
            public const string IGNORE_ALL_SPACE = "ignore-all-space";
            public const string IGNORE_SPACE_CHANGE = "ignore-space-change";
            public const string IGNORE_BLANK_LINES = "ignore-blank-lines";
            public const string FIND_RENAMES = "find-renames";
            public const string NO_RENAMES = "no-renames";
            public const string ALGORITHM_MYERS = "myers";
            public const string ALGORITHM_MINIMAL = "minimal";
            public const string ALGORITHM_PATIENCE = "patience";
            public const string ALGORITHM_HISTOGRAM = "histogram";

            public const string OLD_PREFIX = "a/";
            public const string NEW_PREFIX = "b/";
            public const string DEV_NULL = "/dev/null";

            public const string FILE_HEADER = "diff --git ";
            public const string HUNK_HEADER = "@@ ";
            public const string HUNK_OLD_PREFIX = "@@ -";
            public const string HUNK_NEW_PREFIX = " +";
            public const string HUNK_SUFFIX = " @@";
            public const string NEW_FILE_MODE = "new file mode ";
            public const string DELETED_FILE_MODE = "deleted file mode ";
            public const string OLD_MODE = "old mode ";
            public const string NEW_MODE = "new mode ";
            public const string RENAME_FROM = "rename from ";
            public const string RENAME_TO = "rename to ";
            public const string COPY_FROM = "copy from ";
            public const string COPY_TO = "copy to ";
            public const string SIMILARITY_INDEX = "similarity index ";
            public const string INDEX = "index ";
            public const string BINARY_FILES = "Binary files ";
            public const string OLD_FILE = "--- ";
            public const string NEW_FILE = "+++ ";
            public const string HASH_SEPARATOR = "..";

            public const char LINE_SEPARATOR = '\n';
            public const char ADDED = '+';
            public const char REMOVED = '-';
            public const char CONTEXT = ' ';
            public const char NO_NEWLINE = '\\';
            public const char RANGE_SEPARATOR = ',';
            public const char SPACE = ' ';
            public const char TAB = '\t';
            public const char PERCENT = '%';
            public const int HEADER_PATH_OVERHEAD = 5;

            public static class Errors
            {
                public const string FROM_REQUIRED = "A first commit is required when a second commit or merge base is specified.";
                public const string STAGED_WITH_TO = "Staged changes cannot be compared against a second commit.";
                public const string COMMIT_INVALID = "A commit cannot start with a dash.";
                public const string CONTEXT_INVALID = "The number of context lines cannot be negative.";
                public const string PATH_INVALID = "A path cannot be empty.";
                public const string DIFF_FAILED = "Git failed to diff.";
            }
        }
    }
}
