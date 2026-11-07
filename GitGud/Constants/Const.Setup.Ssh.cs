// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Setup
    {
        public static class Ssh
        {
            public const string ED25519 = "ed25519";
            public const string PUBLIC_KEY_EXTENSION = ".pub";
            public const string WINDOWS_SSH_AGENT_SERVICE = "ssh-agent";
            public const string SSH_AGENT_OUTPUT_FORMAT = "-s";
            public const string SSH_AUTH_SOCK = "SSH_AUTH_SOCK";
            public const string SSH_AGENT_PID = "SSH_AGENT_PID";
            public const string SSH_ASKPASS = "SSH_ASKPASS";
            public const string SSH_ASKPASS_REQUIRE = "SSH_ASKPASS_REQUIRE";
            public const string SSH_ASKPASS_FORCE = "force";
            public const string SSH_AGENT_RUNNING = "RUNNING";
            public const string MAC_SSH_ASKPASS_PATH = "/usr/libexec/ssh-askpass";
            public const string SSH_COMMAND_FORMAT = "ssh -i '{0}' -o IdentitiesOnly=yes";
            public const string SHELL_SINGLE_QUOTE = "'";
            public const string SHELL_ESCAPED_SINGLE_QUOTE = "'\"'\"'";

            public static class Arguments
            {
                public const string TYPE = "t";
                public const string PASSPHRASE = "N";
                public const string OUTPUT = "f";
                public const string QUIET = "q";
                public const string SERVICE_START = "start";
                public const string SERVICE_CONFIG = "config";
                public const string SERVICE_START_SETTING = "start=";
                public const string SERVICE_DEMAND_SETTING = "demand";
                public const string SERVICE_QUERY = "query";
            }
        }
    }
}
