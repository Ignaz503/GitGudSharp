// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string CLEAN = "clean";
    public const string GC = "gc";
    public const string FSCK = "fsck";

    public static partial class Repository
    {
        public const string GIT_CLEAN = $"{GIT} {CLEAN}";
        public const string GIT_GC = $"{GIT} {GC}";
        public const string GIT_FSCK = $"{GIT} {FSCK}";

        public static class FsCheck
        {
            public const string TAGS = "tags";
            public const string ROOT = "root";
            public const string UNREACHABLE = "unreachable";
            public const string CACHE = "cache";
            public const string NO_REFLOGS = "no-reflogs";
            public const string FULL = "full";
            public const string NO_FULL = "no-full";
            public const string STRICT = "strict";
            public const string LOST_FOUND = "lost-found";
            public const string DANGLING = "dangling";
            public const string NO_DANGLING = "no-dangling";
            public const string NO_PROGRESS = "no-progress";
            public const string CONNECTIVITY_ONLY = "connectivity-only";
            public const string NAME_OBJECTS = "name-objects";
            public const string NO_NAME_OBJECTS = "no-name-objects";
            public const string REFERENCES = "references";
            public const string NO_REFERENCES = "no-references";

            public const string PREFIX_DANGLING = "dangling";
            public const string PREFIX_UNREACHABLE = "unreachable";
            public const string PREFIX_ROOT = "root ";
            public const string PREFIX_TAGGED = "tagged ";
            public const string PREFIX_MISSING = "missing ";
            public const string PREFIX_BROKEN_LINK = "broken link from ";
            public const string PREFIX_BROKEN_LINK_TARGET = "to ";
            public const string PREFIX_ERROR_IN = "error in ";
            public const string PREFIX_WARNING_IN = "warning in ";
            public const string KIND_BLOB = "blob";
            public const string KIND_TREE = "tree";
            public const string KIND_COMMIT = "commit";
            public const string KIND_TAG = "tag";
            public const string MESSAGE_SEPARATOR = ": ";
            public const string IN_SEPARATOR = ") in ";

            public static class Errors
            {
                public const string OBJECT_INVALID = "An object to check cannot be empty or start with '-'.";
                public const string FULL_CONFLICT = "Full and NoFull cannot be used together.";
                public const string DANGLING_CONFLICT = "Dangling and NoDangling cannot be used together.";
                public const string NAME_OBJECTS_CONFLICT = "NameObjects and NoNameObjects cannot be used together.";
                public const string REFERENCES_CONFLICT = "References and NoReferences cannot be used together.";
                public const string FSCK_FAILED = "Git failed to verify the object database.";
            }
        }

        public static class Gc
        {
            public const string AGGRESSIVE = "aggressive";
            public const string AUTO = "auto";
            public const string DETACH = "detach";
            public const string NO_DETACH = "no-detach";
            public const string CRUFT = "cruft";
            public const string NO_CRUFT = "no-cruft";
            public const string MAX_CRUFT_SIZE = "max-cruft-size";
            public const string EXPIRE_TO = "expire-to";
            public const string PRUNE = "prune";
            public const string PRUNE_NOW = "now";
            public const string NO_PRUNE = "no-prune";
            public const string QUIET = "quiet";
            public const string FORCE = "force";
            public const string KEEP_LARGEST_PACK = "keep-largest-pack";
            public const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:sszzz";

            public static class Errors
            {
                public const string VALUE_INVALID = "A gc option value cannot be empty.";
                public const string NUMBER_INVALID = "A gc number cannot be negative.";
                public const string PRUNE_CONFLICT = "Prune and NoPrune cannot be used together.";
                public const string CRUFT_CONFLICT = "Cruft and NoCruft cannot be used together.";
                public const string EXPIRE_TO_REQUIRES_CRUFT = "ExpireTo cannot be used together with NoCruft.";
                public const string GC_FAILED = "Git failed to clean up and optimize the repository.";
            }
        }

        public static class Cleaning
        {
            public const string DIRECTORIES = "-d";
            public const string FORCE = "-f";
            public const string DRY_RUN = "-n";
            public const string QUIET = "-q";
            public const string EXCLUDE = "-e";
            public const string INCLUDE_IGNORED = "-x";
            public const string ONLY_IGNORED = "-X";

            public static class Errors
            {
                public const string FORCE_REQUIRED = "Either Force or DryRun must be set to clean.";
                public const string IGNORED_CONFLICT = "IncludeIgnored and OnlyIgnored cannot be used together.";
                public const string EXCLUDE_INVALID = "An exclude pattern cannot be empty.";
                public const string PATH_INVALID = "A path cannot be empty.";
                public const string CLEAN_FAILED = "Git failed to clean the working tree.";
            }
        }
    }
}
