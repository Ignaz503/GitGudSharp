// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string CLONE = "clone";
    public const string REV_PARSE = "rev-parse";

    public static partial class Repository
    {
        public const string GIT_CLONE = $"{GIT} {CLONE}";
        public const string GIT_OPEN = $"{GIT} {REV_PARSE}";
        public const string BARE_ARGUMENT = "bare";
        public const string BRANCH_ARGUMENT = "branch";
        public const string DEPTH_ARGUMENT = "depth";
        public const string QUIET_ARGUMENT = "quiet";
        public const string RECURSE_SUBMODULES_ARGUMENT = "recurse-submodules";
        public const string SHALLOW_SUBMODULES_ARGUMENT = "shallow-submodules";
        public const string IS_BARE_ARGUMENT = "is-bare-repository";
        public const string WORKING_DIRECTORY_ARGUMENT = "-C";
        public const string END_OF_OPTIONS = "--";
        public const string TRUE = "true";

        public static class Errors
        {
            public const string OPTIONS_REQUIRED = "Clone options cannot be null.";
            public const string SOURCE_REQUIRED = "A repository source is required.";
            public const string TARGET_PATH_REQUIRED = "A target path is required.";
            public const string TARGET_PATH_INVALID = "The target path is invalid.";
            public const string DEPTH_INVALID = "The clone depth must be greater than zero.";
            public const string SUBMODULE_INVALID = "A submodule path cannot be empty.";
            public const string CLONE_FAILED = "Git failed to clone the repository.";
            public const string CLONE_EXECUTION = "Git clone could not be executed.";
            public const string OPEN_PATH_NOT_FOUND = "The repository directory does not exist.";
            public const string OPEN_NOT_A_REPOSITORY = "The directory is not a git repository.";
            public const string OPEN_EXECUTION = "The repository could not be opened.";
            public const string CANCELLED = "The operation was cancelled.";
        }

    }
}
