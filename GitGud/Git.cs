// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Init;
using GitGud.Repositories;
using GitGud.Setup;
using GitGud.Utility;

namespace GitGud;

public partial class Git : IGit
{
    private readonly Configuration _globalConfiguration = new();

    private readonly Initialize _init = new();

    private readonly GitSetup _setup;

    private readonly RepositoryProvider _repositories = new();

    public Git()
        => _setup = new GitSetup(_globalConfiguration);

    public Task<Result<IRepository, IError>> CloneRepository(ICloneOptions options, System.Threading.CancellationToken cancellationToken = default)
        => _repositories.Clone(options, cancellationToken);

    public Task<Result<IConfiguration, IError>> GetGlobalConfiguration()
        => Task.FromResult(Result<IConfiguration, IError>.Some(_globalConfiguration));

    public Task<Result<IGitGlobStatusResult, IError>> GetStatus(bool forceRefresh = false, System.Threading.CancellationToken cancellationToken = default)
        => _setup.GetStatus(forceRefresh, cancellationToken);

    public async Task<Result<IRepository, IError>> Init(IInitOptions options)
        => await _init.Init(options).ConfigureAwait(false);

    public Task<Result<IRepository, IError>> OpenRepository(DirectoryPath path, System.Threading.CancellationToken cancellationToken = default)
        => _repositories.Open(path, cancellationToken);

    public async Task<Result<Nothing, IError>> SetupGit(IGitSetupContext setupContext) 
        => await _setup.Setup(setupContext).ConfigureAwait(false);

    public async Task<Result<Nothing, IError>> UninstallGit(IGitUninstallContext uninstallContext) 
        => await _setup.Uninstall(uninstallContext).ConfigureAwait(false);

    public async Task<Result<Nothing, IError>> UpdateGit(IGitUpdateContext updateContext) 
        => await _setup.Update(updateContext).ConfigureAwait(false);

    public Task<Result<SshKeyResult, IError>> GenerateSshKeyPair(IGitSshKeyContext context)
        => _setup.GenerateSshKeyPair(context);

}


