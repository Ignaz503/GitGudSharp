#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string RESTORE = "restore";

    public static partial class Repository
    {
        public const string GIT_RESTORE = $"{GIT} {RESTORE}";

        public static class Restoring
        {
            public const string SOURCE = "source";
            public const string STAGED = "staged";
            public const string WORKTREE = "worktree";
            public const string OURS = "ours";
            public const string THEIRS = "theirs";
            public const string MERGE = "merge";
            public const string IGNORE_UNMERGED = "ignore-unmerged";
            public const string IGNORE_SKIP_WORKTREE_BITS = "ignore-skip-worktree-bits";
            public const string OVERLAY = "overlay";
            public const string NO_OVERLAY = "no-overlay";
            public const string RECURSE_SUBMODULES = "recurse-submodules";
            public const string NO_RECURSE_SUBMODULES = "no-recurse-submodules";

            public static class Errors
            {
                public const string PATHS_REQUIRED = "At least one path is required to restore.";
                public const string PATH_INVALID = "A path cannot be empty.";
                public const string SOURCE_INVALID = "The restore source cannot start with a dash.";
                public const string OURS_THEIRS_CONFLICT = "Ours and Theirs cannot be used together.";
                public const string SOURCE_CONFLICT = "Ours, Theirs and Merge cannot be used together with a source.";
                public const string RESTORE_FAILED = "Git failed to restore the files.";
            }
        }
    }
}
