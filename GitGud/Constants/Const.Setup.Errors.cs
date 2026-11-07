// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Setup 
    {
        public static class Errors
        {
            public const string OPERATION_CONTEXT_REQUIRED = "An operation context is required.";
            public const string NO_PACKAGE_MANAGER = "No supported package manager was detected for this operating system.";
            public const string PACKAGE_MANAGER_UNSUPPORTED = "The detected package manager is not supported.";
            public const string SETUP_EXECUTION = "The Git setup operation could not be executed.";
            public const string GIT_VERSION_QUERY_FAILED = "The Git version query failed.";
            public const string OPERATION_CANCELLED = "The operation was cancelled.";
            public const string PACKAGE_COMMAND_FAILED_FORMAT = "The {0} command failed with exit code {1}.";
            public const string SSH_KEY_PATH_REQUIRED = "A private key path is required.";
            public const string SSH_KEY_ALREADY_EXISTS = "A key file already exists at the requested path.";
            public const string SSH_KEYGEN_NOT_FOUND = "The OpenSSH key generation executable was not found.";
            public const string SSH_KEY_GENERATION_FAILED = "SSH key generation failed.";
            public const string SSH_AGENT_START_FAILED = "The OpenSSH authentication agent could not be started.";
            public const string SSH_AGENT_ADD_FAILED = "The SSH key could not be added to the authentication agent.";
            public const string SSH_ASKPASS_NOT_FOUND = "No supported OpenSSH askpass program was found.";
            public const string SSH_COMMAND_EXISTS = "A global core.sshCommand is already configured and overwriting it was not confirmed.";
            public const string SSH_GIT_CONFIG_FAILED = "Git could not be configured to use the generated SSH key.";
        }

    }
}

            