// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Utility;
using GitGud.Erorrs;


namespace GitGud.Setup;

/// <summary>
/// This performs the git setup.
/// Installs, uninstalls, and updates Git using the detected platform package manager.
/// </summary>
public interface IGitSetup 
{
    /// <summary>
    /// Gets the Git installation status. Results are cached briefly; pass <paramref name="forceRefresh"/> to bypass the cache.
    /// An installed executable that cannot report its version is reported as not installed.
    /// </summary>
    Task<Result<IGitGlobStatusResult, IError>> GetStatus(bool forceRefresh = false, System.Threading.CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> Setup(IGitSetupContext setupContext);

    Task<Result<Nothing, IError>> Uninstall(IGitUninstallContext uninstallContext);

    Task<Result<Nothing, IError>> Update(IGitUpdateContext updateContext);

    /// <summary>
    /// Generates an SSH keypair, loads it into ssh-agent, and configures Git's global SSH command to use it.
    /// ssh-keygen obtains randomness from the operating system; no consumer-provided seed is required.
    /// </summary>
    Task<Result<SshKeyResult, IError>> GenerateSshKeyPair(IGitSshKeyContext context);
}
