// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

internal sealed class SubmoduleOperations : ISubmoduleOperations
{
    private readonly ICommandExecutor _executor;

    public SubmoduleOperations(ICommandExecutor executor)
        => _executor = executor;

    public Task<Result<Nothing, IError>> Add(SubmoduleAddOptions options, CancellationToken cancellationToken = default)
        => Run(options, SubConst.Errors.ADD_FAILED, cancellationToken);

    public async Task<Result<IReadOnlyList<SubmoduleStatusInfo>, IError>> Status(SubmoduleStatusOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new SubmoduleStatusOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<IReadOnlyList<SubmoduleStatusInfo>, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            SubConst.GIT_SUBMODULE, SubConst.Errors.STATUS_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<SubmoduleStatusInfo>, IError>.None(output.Handle());

        return Result<IReadOnlyList<SubmoduleStatusInfo>, IError>.Some(ParseStatus(output.Unwrap()));
    }

    public Task<Result<Nothing, IError>> Init(SubmoduleInitOptions? options = null, CancellationToken cancellationToken = default)
        => Run(options ?? new SubmoduleInitOptions(), SubConst.Errors.INIT_FAILED, cancellationToken);

    public Task<Result<Nothing, IError>> Deinit(SubmoduleDeinitOptions options, CancellationToken cancellationToken = default)
        => Run(options, SubConst.Errors.DEINIT_FAILED, cancellationToken);

    public Task<Result<Nothing, IError>> Update(SubmoduleUpdateOptions? options = null, CancellationToken cancellationToken = default)
        => Run(options ?? new SubmoduleUpdateOptions(), SubConst.Errors.UPDATE_FAILED, cancellationToken);

    public Task<Result<Nothing, IError>> SetBranch(SubmoduleSetBranchOptions options, CancellationToken cancellationToken = default)
        => Run(options, SubConst.Errors.SET_BRANCH_FAILED, cancellationToken);

    public Task<Result<Nothing, IError>> SetUrl(SubmoduleSetUrlOptions options, CancellationToken cancellationToken = default)
        => Run(options, SubConst.Errors.SET_URL_FAILED, cancellationToken);

    public Task<Result<string?, IError>> Summary(SubmoduleSummaryOptions? options = null, CancellationToken cancellationToken = default)
        => Execute(options ?? new SubmoduleSummaryOptions(), SubConst.Errors.SUMMARY_FAILED, cancellationToken);

    public Task<Result<string?, IError>> Foreach(SubmoduleForeachOptions options, CancellationToken cancellationToken = default)
        => Execute(options, SubConst.Errors.FOREACH_FAILED, cancellationToken);

    public Task<Result<Nothing, IError>> Sync(SubmoduleSyncOptions? options = null, CancellationToken cancellationToken = default)
        => Run(options ?? new SubmoduleSyncOptions(), SubConst.Errors.SYNC_FAILED, cancellationToken);

    public Task<Result<Nothing, IError>> AbsorbGitDirs(SubmoduleAbsorbGitDirsOptions? options = null, CancellationToken cancellationToken = default)
        => Run(options ?? new SubmoduleAbsorbGitDirsOptions(), SubConst.Errors.ABSORB_FAILED, cancellationToken);

    private Task<Result<Nothing, IError>> Run(SubmoduleOptionsBase? options, string failure, CancellationToken cancellationToken)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(new RepositoryValidationError(SubConst.GIT_SUBMODULE, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(SubConst.GIT_SUBMODULE, failure, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    private async Task<Result<string?, IError>> Execute(SubmoduleOptionsBase? options, string failure, CancellationToken cancellationToken)
    {
        if (options is null)
            return Result<string?, IError>.None(new RepositoryValidationError(SubConst.GIT_SUBMODULE, Const.Repository.OperationErrors.OPTIONS_REQUIRED));

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<string?, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            SubConst.GIT_SUBMODULE, failure, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<string?, IError>.None(output.Handle());

        return Result<string?, IError>.Some(output.Unwrap());
    }

    private static IReadOnlyList<SubmoduleStatusInfo> ParseStatus(string? output)
    {
        var result = new List<SubmoduleStatusInfo>();
        if (string.IsNullOrEmpty(output))
            return result;

        var lines = output!.Split(SubConst.LINE_SEPARATOR);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (line.Length < SubConst.STATUS_MIN_LENGTH)
                continue;

            var state = line[0].MapCharacterToSubmoduleState();

            var fields = line.Substring(1).Split(new[] { SubConst.FIELD_SEPARATOR }, SubConst.STATUS_FIELD_COUNT);
            if (fields.Length < 2)
                continue;

            result.Add(new SubmoduleStatusInfo(state, fields[0], fields[1], fields.Length > 2 ? fields[2].Trim('(', ')') : null));
        }

        return result;
    }
}
