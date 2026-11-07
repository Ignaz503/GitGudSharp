// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{

    public static partial class Repository
    {

        public const string CHERRY_PICK = "cherry-pick";
        public const string GIT_CHERRY_PICK = $"{GIT} {CHERRY_PICK}";
        public static class CherryPickArguments
        {
            public const string EDIT = "-e";
            public const string CLEANUP = "--cleanup=";
            public const string APPEND_SOURCE = "-x";
            public const string NO_OP_RELEASE = "-r";
            public const string MAINLINE = "-m";
            public const string NO_COMMIT = "-n";
            public const string SIGNOFF = "-s";
            public const string GPG_SIGN = "-S";
            public const string NO_GPG_SIGN = "--no-gpg-sign";
            public const string FF = "--ff";
            public const string ALLOW_EMPTY = "--allow-empty";
            public const string ALLOW_EMPTY_MESSAGE = "--allow-empty-message";
            public const string EMPTY = "--empty=";
            public const string KEEP_REDUNDANT_COMMITS = "--keep-redundant-commits";
            public const string STRATEGY = "--strategy=";
            public const string STRATEGY_OPTION = "-X";
            public const string RERERE_AUTOUPDATE = "--rerere-autoupdate";
            public const string NO_RERERE_AUTOUPDATE = "--no-rerere-autoupdate";
            public const string CONTINUE = "--continue";
            public const string SKIP = "--skip";
            public const string ABORT = "--abort";
            public const string QUIT = "--quit";
            public const string EMPTY_DROP = "drop";
            public const string EMPTY_KEEP = "keep";
            public const string EMPTY_STOP = "stop";
        }

    }

}
