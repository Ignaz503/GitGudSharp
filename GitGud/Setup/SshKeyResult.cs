// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

using GitGud.Utility;

namespace GitGud.Setup;

/// <summary>
/// The generated SSH keypair. On Unix-like systems, apply AgentEnvironment to processes that need to use ssh-agent.
/// </summary>
public sealed record SshKeyResult(
    FilePath PrivateKeyPath,
    FilePath PublicKeyPath,
    string PublicKey,
    IReadOnlyDictionary<string, string> AgentEnvironment);
