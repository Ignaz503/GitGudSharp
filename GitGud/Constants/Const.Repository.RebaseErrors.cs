// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{

    public static partial class Repository
    {
        public static class RebaseErrors
        {
            public const string MODE_EXCLUSIVE = "A rebase mode (continue, skip, abort, quit, edit-todo, show-current-patch) cannot be combined with any other option.";
            public const string APPLY_INCOMPATIBLE = "Apply, Whitespace and ContextLines are incompatible with Merge, Strategy, StrategyOptions, Autosquash, RebaseMerges, Interactive, Exec, NoKeepEmpty, Empty, (No)ReapplyCherryPicks without KeepBase, UpdateRefs, Root without Onto and Trailers.";
            public const string KEEP_BASE_ONTO = "KeepBase and Onto cannot be combined.";
            public const string KEEP_BASE_ROOT = "KeepBase and Root cannot be combined.";
            public const string FORK_POINT_ROOT = "ForkPoint and Root cannot be combined.";
            public const string CONTRADICTORY = "Contradictory options were specified (an option and its negation).";
            public const string ROOT_UPSTREAM = "An upstream cannot be specified together with Root.";
            public const string BRANCH_WITHOUT_UPSTREAM = "A branch requires an upstream unless Root is specified.";
            public const string VALUE_INVALID = "Rebase values (upstream, branch, onto, exec, trailers, strategy options) cannot be empty.";
            public const string CONTEXT_INVALID = "The context line count cannot be negative.";
            public const string REBASE_FAILED = "Git failed to rebase.";
            public const string QUIET_VERBOSE = "Quiet and Verbose cannot be combined.";
            public const string GPG_KEY_WITHOUT_SIGN = "GpgKeyId cannot be combined with GpgSign = false.";
        }
    }

}
