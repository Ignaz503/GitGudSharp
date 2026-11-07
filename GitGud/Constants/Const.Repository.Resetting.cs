// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string RESET = "reset";

    public static partial class Repository
    {
        public const string GIT_RESET = $"{GIT} {RESET}";

        public static class Resetting
        {
            public const string SOFT = "soft";
            public const string MIXED = "mixed";
            public const string HARD = "hard";
            public const string MERGE = "merge";
            public const string KEEP = "keep";
            public const string REFRESH = "refresh";
            public const string NO_REFRESH = "no-refresh";
            public const string RECURSE_SUBMODULES = "recurse-submodules";
            public const string NO_RECURSE_SUBMODULES = "no-recurse-submodules";
            public const string INTENT_TO_ADD_SHORT = "-N";

            public static class Errors
            {
                public const string PATH_INVALID = "A path cannot be empty.";
                public const string COMMIT_INVALID = "The commit cannot start with a dash.";
                public const string MODE_WITH_PATHS = "Only the default or mixed mode can be combined with paths.";
                public const string INTENT_TO_ADD_INVALID = "Intent-to-add requires a mixed reset without paths.";
                public const string MODE_INVALID = "The reset mode is not valid.";
                public const string RESET_FAILED = "Git failed to reset.";
            }
        }
    }
}
