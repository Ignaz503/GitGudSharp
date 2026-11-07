// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{

    public static partial class Repository
    {
        public static class Applying
        {
            public const string GIT_APPLY = $"{GIT} apply";

            public const string STAT = "--stat";
            public const string NUMSTAT = "--numstat";
            public const string SUMMARY = "--summary";
            public const string CHECK = "--check";
            public const string INDEX = "--index";
            public const string CACHED = "--cached";
            public const string INTENT_TO_ADD = "--intent-to-add";
            public const string THREE_WAY = "--3way";
            public const string OURS = "--ours";
            public const string THEIRS = "--theirs";
            public const string UNION = "--union";
            public const string APPLY = "--apply";
            public const string NO_ADD = "--no-add";
            public const string REVERSE = "--reverse";
            public const string REJECT = "--reject";
            public const string NUL_TERMINATED = "-z";
            public const string UNIDIFF_ZERO = "--unidiff-zero";
            public const string IGNORE_WHITESPACE = "--ignore-whitespace";
            public const string INACCURATE_EOF = "--inaccurate-eof";
            public const string RECOUNT = "--recount";
            public const string VERBOSE = "--verbose";
            public const string QUIET = "--quiet";
            public const string UNSAFE_PATHS = "--unsafe-paths";
            public const string ALLOW_EMPTY = "--allow-empty";
            public const string IGNORE_SPACE_CHANGE = "--ignore-space-change";
            public const string BUILD_FAKE_ANCESTOR = "build-fake-ancestor";
            public const string ARGUMENT_SEPARATOR = "--";

            public const string WHITESPACE = "whitespace";
            public const string WHITESPACE_NOWARN = "nowarn";
            public const string WHITESPACE_WARN = "warn";
            public const string WHITESPACE_FIX = "fix";
            public const string WHITESPACE_ERROR = "error";
            public const string WHITESPACE_ERROR_ALL = "error-all";
            public const string DIRECTORY = "directory";
            public const string EXCLUDE = "exclude";
            public const string INCLUDE = "include";

            public const string FORMAT_PATH_STRIP = "-p{0}";
            public const string FORMAT_CONTEXT = "-C{0}";

            public static class Errors
            {
                public const string APPLY_FAILED = "Failed to apply patch.";
                public const string PATCH_REQUIRED = "At least one patch file is required.";
                public const string PATCH_INVALID = "Patch file path is invalid.";
                public const string VALUE_INVALID = "An apply option value is invalid.";
                public const string NUMBER_INVALID = "Path strip and context values cannot be negative.";
                public const string TARGET_CONFLICT = "Index, cached and intent-to-add options conflict.";
                public const string RESOLUTION_CONFLICT = "Only one of ours, theirs or union may be specified.";
                public const string RESOLUTION_REQUIRES_THREE_WAY = "Ours, theirs and union require a three-way merge.";
                public const string THREE_WAY_REJECT_CONFLICT = "Three-way and reject cannot both be specified.";
                public const string VERBOSITY_CONFLICT = "Verbose and quiet cannot both be specified.";
            }
        }
    }

}
