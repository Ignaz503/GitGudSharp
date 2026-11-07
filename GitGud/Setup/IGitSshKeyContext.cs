// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Utility;

namespace GitGud.Setup;

public interface IGitSshKeyContext : IOperationContext
{
    FilePath PrivateKeyPath { get; }

    /// <summary>
    /// The passphrase is supplied to ssh-keygen as a process argument and may be visible to local process inspection.
    /// Loading the protected key into ssh-agent prompts through the discovered askpass program.
    /// </summary>
    string? Passphrase { get; }

    /// <summary>
    /// Invoked with the existing global core.sshCommand value when one is already configured.
    /// Return true to overwrite it. If no callback is provided, an existing value causes the operation to fail.
    /// </summary>
    Func<string, Task<bool>>? ShouldOverwriteSshCommand { get; }
}

public sealed record GitSshKeyContext(
    FilePath PrivateKeyPath,
    string? Passphrase = null,
    CancellationToken CancellationToken = default,
    Func<string, Task<bool>>? ShouldOverwriteSshCommand = null) : IGitSshKeyContext;
