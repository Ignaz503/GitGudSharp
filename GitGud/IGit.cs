// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Config;
using GitGud.Init;
using GitGud.Repositories;
using GitGud.Setup;
using GitGud.Utility;

namespace GitGud;

public interface IGit 
{
    //this is the git basic interface
    //modify global configs
    //create repositories
    //create clones etc

    Task<Result<IGitGlobStatusResult, IError>> GetStatus(bool forceRefresh = false, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones a repository
    /// </summary>
    /// <returns></returns>
    Task<Result<IRepository, IError>> CloneRepository(ICloneOptions options, System.Threading.CancellationToken cancellationToken = default);

    
    /// <summary>
    /// Opens an already existing repository on the local machine
    /// </summary>
    /// <returns></returns>
    Task<Result<IRepository, IError>> OpenRepository(DirectoryPath path, System.Threading.CancellationToken cancellationToken = default);


    Task<Result<IRepository, IError>> Init(IInitOptions options);


    /// <summary>
    /// Retrives the global configuration.
    /// </summary>
    /// <returns></returns>
    Task<Result<IConfiguration, IError>> GetGlobalConfiguration();

    Task<Result<Nothing, IError>> SetupGit(IGitSetupContext setupContext);

    Task<Result<Nothing, IError>> UninstallGit(IGitUninstallContext uninstallContext);

    Task<Result<Nothing, IError>> UpdateGit(IGitUpdateContext updateContext);

    Task<Result<SshKeyResult, IError>> GenerateSshKeyPair(IGitSshKeyContext context);

}
