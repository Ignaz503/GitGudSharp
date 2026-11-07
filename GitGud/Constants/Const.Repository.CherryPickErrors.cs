// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{

    public static partial class Repository
    {
        public static class CherryPickErrors
        {
            public const string COMMITS_REQUIRED = "At least one commit is required unless a sequencer action is specified.";
            public const string COMMIT_INVALID = "A commit reference cannot be empty.";
            public const string MAINLINE_INVALID = "The mainline parent number must be greater than zero.";
            public const string SEQUENCER_CONFLICT = "Sequencer actions cannot be combined with commits.";
            public const string STRATEGY_OPTION_INVALID = "A strategy option cannot be empty.";
            public const string CHERRY_PICK_FAILED = "Git failed to cherry-pick.";

            public const string GPG_KEY_WITHOUT_SIGN = "GpgKeyId cannot be combined with GpgSign = false.";

        }

    }

}
