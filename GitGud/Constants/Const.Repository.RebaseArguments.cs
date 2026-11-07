// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string REBASE = "rebase";

    public static partial class Repository
    {
        public const string GIT_REBASE = $"{GIT} {REBASE}";
        public static class RebaseArguments
        {
            // Modes
            public const string CONTINUE = "--continue";
            public const string SKIP = "--skip";
            public const string ABORT = "--abort";
            public const string QUIT = "--quit";
            public const string EDIT_TODO = "--edit-todo";
            public const string SHOW_CURRENT_PATCH = "--show-current-patch";

            // Options
            public const string ONTO = "--onto";
            public const string KEEP_BASE = "--keep-base";
            public const string APPLY = "--apply";
            public const string EMPTY = "--empty=";
            public const string KEEP_EMPTY = "--keep-empty";
            public const string NO_KEEP_EMPTY = "--no-keep-empty";
            public const string REAPPLY_CHERRY_PICKS = "--reapply-cherry-picks";
            public const string NO_REAPPLY_CHERRY_PICKS = "--no-reapply-cherry-picks";
            public const string ALLOW_EMPTY_MESSAGE = "--allow-empty-message";
            public const string MERGE = "--merge";
            public const string STRATEGY = "--strategy=";
            public const string STRATEGY_OPTION = "--strategy-option=";
            public const string RERERE_AUTOUPDATE = "--rerere-autoupdate";
            public const string NO_RERERE_AUTOUPDATE = "--no-rerere-autoupdate";
            public const string GPG_SIGN = "-S";
            public const string NO_GPG_SIGN = "--no-gpg-sign";
            public const string QUIET = "--quiet";
            public const string VERBOSE = "--verbose";
            public const string STAT = "--stat";
            public const string NO_STAT = "--no-stat";
            public const string VERIFY = "--verify";
            public const string NO_VERIFY = "--no-verify";
            public const string CONTEXT = "-C";
            public const string FORCE_REBASE = "--force-rebase";
            public const string FORK_POINT = "--fork-point";
            public const string NO_FORK_POINT = "--no-fork-point";
            public const string IGNORE_WHITESPACE = "--ignore-whitespace";
            public const string WHITESPACE = "--whitespace=";
            public const string COMMITTER_DATE_IS_AUTHOR_DATE = "--committer-date-is-author-date";
            public const string IGNORE_DATE = "--ignore-date";
            public const string RESET_AUTHOR_DATE = "--reset-author-date";
            public const string SIGNOFF = "--signoff";
            public const string TRAILER = "--trailer=";
            public const string INTERACTIVE = "--interactive";
            public const string REBASE_MERGES = "--rebase-merges";
            public const string REBASE_MERGES_VALUE = "--rebase-merges=";
            public const string NO_REBASE_MERGES = "--no-rebase-merges";
            public const string EXEC = "--exec";
            public const string ROOT = "--root";
            public const string AUTOSQUASH = "--autosquash";
            public const string NO_AUTOSQUASH = "--no-autosquash";
            public const string AUTOSTASH = "--autostash";
            public const string NO_AUTOSTASH = "--no-autostash";
            public const string RESCHEDULE_FAILED_EXEC = "--reschedule-failed-exec";
            public const string NO_RESCHEDULE_FAILED_EXEC = "--no-reschedule-failed-exec";
            public const string UPDATE_REFS = "--update-refs";
            public const string NO_UPDATE_REFS = "--no-update-refs";

            // Values
            public const string EMPTY_DROP = "drop";
            public const string EMPTY_KEEP = "keep";
            public const string EMPTY_STOP = "stop";
            public const string EMPTY_ASK = "ask";
            public const string REBASE_COUSINS = "rebase-cousins";
            public const string NO_REBASE_COUSINS = "no-rebase-cousins";

        }
    }

}
