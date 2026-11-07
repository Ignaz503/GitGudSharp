// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Setup 
    {
        public static class Status
        {
            public const string PATH_ENVIRONMENT_VARIABLE = "PATH";
            public const string WINDOWS_EXECUTABLE = "git.exe";
            public const string UNIX_EXECUTABLE = "git";
            public const string GIT_VERSION_PREFIX = "git version ";
            public const int CACHE_SECONDS = 30;
            public const string WINDOWS_INSTALL_DIRECTORY = "Git";
            public const string WINDOWS_COMMAND_DIRECTORY = "cmd";
            public const string WINDOWS_BIN_DIRECTORY = "bin";
            public const string USER_PROGRAMS_DIRECTORY = "Programs";
            public const string LINUX_USR_BIN_GIT = "/usr/bin/git";
            public const string LINUX_USR_LOCAL_BIN_GIT = "/usr/local/bin/git";
            public const string LINUX_BIN_GIT = "/bin/git";
            public const string MAC_USR_LOCAL_BIN_GIT = "/usr/local/bin/git";
            public const string MAC_HOMEBREW_BIN_GIT = "/opt/homebrew/bin/git";
            public const string MAC_MACPORTS_BIN_GIT = "/opt/local/bin/git";
        }

        internal static readonly string[] LinuxGitPaths =
        {
            Const.Setup.Status.LINUX_USR_BIN_GIT,
            Const.Setup.Status.LINUX_USR_LOCAL_BIN_GIT,
            Const.Setup.Status.LINUX_BIN_GIT
        };

        internal static readonly string[] MacGitPaths =
        {
            Const.Setup.Status.MAC_USR_LOCAL_BIN_GIT,
            Const.Setup.Status.MAC_HOMEBREW_BIN_GIT,
            Const.Setup.Status.MAC_MACPORTS_BIN_GIT
        };


    }
}



