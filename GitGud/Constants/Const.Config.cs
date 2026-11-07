// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Config
    {
        public const string GIT_CONFIG = "git config";

        public static class Keys
        {
            public const string CORE_SSH_COMMAND = "core.sshCommand";
        }

        public static class Commands
        {
            public const string LIST = "list";
            public const string GET = "get";
            public const string SET = "set";
            public const string UNSET = "unset";
            public const string RENAME_SECTION = "rename-section";
            public const string REMOVE_SECTION = "remove-section";
            public const string EDIT = "edit";

        }

        public static class FileOptions
        {
            public const string LOCAL = "--local";
            public const string GLOBAL = "--global";
            public const string SYSTEM = "--system";
            public const string WORKTREE = "--worktree";
            public const string FILE = "--file";
        }

        public static class Flags
        {
            public const string LIST = "--list";
            public const string NULL_TERMINATED = "-z";
            public const string INCLUDES = "--includes";
            public const string SHOW_SCOPE = "--show-scope";
            public const string SHOW_ORIGIN = "--show-origin";
            public const string GET_REGEXP = "--get-regexp";
            public const string GET_URLMATCH = "--get-urlmatch";
            public const string FIXED_VALUE = "--fixed-value";
            public const string REPLACE_ALL = "--replace-all";
            public const string UNSET = "--unset";
            public const string UNSET_ALL = "--unset-all";
            public const string RENAME_SECTION = "--rename-section";
            public const string REMOVE_SECTION = "--remove-section";
            public const string EDIT = "--edit";
            public const string TYPE_FORMAT = "--type={0}";
            public const string SCOPE_COMMAND = "command";
            public const string ORIGIN_FILE_PREFIX = "file:";
            public const string REGEX_SPECIAL_CHARACTERS = ".^$*+?()[]{}|\\";
        }

        public static class Arguments
        {
            public const string SYSTEM = "system";
            public const string GLOBAL = "global";
            public const string LOCAL = "local";
            public const string WORKTREE = "worktree";
            public const string FILE = "file";

            public const string TYPE_STRING = "string";
            public const string TYPE_BOOLEAN = "bool";
            public const string TYPE_BOOL_OR_INT = "bool-or-int";
            public const string TYPE_INTEGER = "int";
            public const string TYPE_COLOR = "color";
            public const string TYPE_PATHNAME = "path";
            public const string TYPE_EXPIRY_DATE = "expiry-date";
 
        }



        public static class Errors
        {
            public const string INVALID_KEY = "The Git configuration key is invalid.";
            public const string VALUE_REQUIRED = "A Git configuration value is required.";
            public const string COMMAND_FAILED_FORMAT = "The git config {0} command failed with exit code {1}.";
            public const string FILE_REQUIRED = "A configuration file path is required for the file scope.";
            public const string INVALID_OPTIONS = "The Git configuration options are invalid.";
            public const string MISSING_VALUE = "The requested Git configuration value was not found.";
            public const string EXECUTION_FAILED = "The Git configuration command could not be executed.";
            public const string UNSUPPORTED_SCOPE = "The selected Git configuration scope cannot be queried by this API.";
        }
    }
}
