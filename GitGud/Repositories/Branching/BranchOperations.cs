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

namespace GitGud.Repositories.Branching;

public sealed class BranchOperations : IBranchOperations
{
    private readonly ICommandExecutor _executor;

    public BranchOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<IReadOnlyList<BranchInfo>, IError>> ListBranches(ListBranchesOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ListBranchesOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<IReadOnlyList<BranchInfo>, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.LIST_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<BranchInfo>, IError>.None(output.Handle());

        return Result<IReadOnlyList<BranchInfo>, IError>.Some(ParseBranches(output.Unwrap()));
    }
    public Task<Result<Nothing, IError>> CreateBranch(CreateBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_BRANCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.CREATE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> DeleteBranch(DeleteBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_BRANCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.DELETE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> DeleteRemoteBranch(DeleteRemoteBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_PUSH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_PUSH, Const.Repository.Branching.Errors.DELETE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }
    public Task<Result<Nothing, IError>> RenameBranch(RenameBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_BRANCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.RENAME_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> RenameRemoteBranch(RenameRemoteBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_PUSH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_PUSH, Const.Repository.Branching.Errors.RENAME_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> CopyBranch(CopyBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_BRANCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.COPY_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> SetUpstream(SetUpstreamOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_BRANCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.UPSTREAM_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> UnsetUpstream(UnsetUpstreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new UnsetUpstreamOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.UPSTREAM_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public async Task<Result<Branch?, IError>> GetCurrentBranch(CancellationToken cancellationToken = default)
    {
        var output = await _executor.ExecuteAsync(Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.CURRENT_FAILED, args =>
        {
            args.AddArgument(Const.BRANCH, true);
            args.Flag(Const.Repository.Branching.SHOW_CURRENT);
        }, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<Branch?, IError>.None(output.Handle());

        var name = output.Unwrap()?.Trim();
        if (string.IsNullOrEmpty(name))
            return Result<Branch?, IError>.Some(null);

        return Result<Branch?, IError>.Some(new Branch(name!));
    }

    public async Task<Result<IReadOnlyList<Branch>, IError>> DeleteMergedBranches(DeleteMergedOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Result<IReadOnlyList<Branch>, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_BRANCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED));

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<IReadOnlyList<Branch>, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_BRANCH, Const.Repository.Branching.Errors.DELETE_MERGED_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<Branch>, IError>.None(output.Handle());

        return Result<IReadOnlyList<Branch>, IError>.Some(ParseDeletedBranches(output.Unwrap()));
    }

    public Task<Result<Nothing, IError>> CheckoutBranch(CheckoutBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.Branching.GIT_CHECKOUT, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.Branching.GIT_CHECKOUT, Const.Repository.Branching.Errors.CHECKOUT_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> SwitchBranch(SwitchBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.Branching.GIT_SWITCH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.Branching.GIT_SWITCH, Const.Repository.Branching.Errors.SWITCH_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    internal static IReadOnlyList<Branch> ParseDeletedBranches(string? output)
    {
        var branches = new List<Branch>();
        if (string.IsNullOrEmpty(output))
            return branches;

        var lines = output!.Split(Const.Repository.Branching.LINE_SEPARATOR);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            string? rest = null;
            if (line.StartsWith(Const.Repository.Branching.DELETED_PREFIX, StringComparison.Ordinal))
                rest = line.Substring(Const.Repository.Branching.DELETED_PREFIX.Length);
            else if (line.StartsWith(Const.Repository.Branching.WOULD_DELETE_PREFIX, StringComparison.Ordinal))
                rest = line.Substring(Const.Repository.Branching.WOULD_DELETE_PREFIX.Length);

            if (rest is null)
                continue;

            var end = rest.IndexOf(' ');
            var name = end < 0 ? rest : rest.Substring(0, end);
            if (name.Length > 0)
                branches.Add(new Branch(name));
        }

        return branches;
    }

    internal static IReadOnlyList<BranchInfo> ParseBranches(string? output)
    {
        var branches = new List<BranchInfo>();
        if (string.IsNullOrEmpty(output))
            return branches;

        var lines = output!.Split(Const.Repository.Branching.LINE_SEPARATOR);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd(Const.Repository.Branching.CARRIAGE_RETURN);
            if (line.Length == 0)
                continue;

            var fields = line.Split(Const.Repository.Branching.FIELD_SEPARATOR);
            if (fields.Length < Const.Repository.Branching.FIELD_COUNT)
                continue;

            var isCurrent = fields[0] == Const.Repository.Branching.CURRENT_MARKER;
            var refName = fields[1];
            var commit = fields[2];
            var upstream = fields[3].Length == 0 ? null : fields[3];

            if (refName.StartsWith(Const.Repository.Branching.HEADS_PREFIX, StringComparison.Ordinal))
            {
                var name = refName.Substring(Const.Repository.Branching.HEADS_PREFIX.Length);
                branches.Add(new BranchInfo(new Branch(name), null, isCurrent, commit, upstream));
            }
            else if (refName.StartsWith(Const.Repository.Branching.REMOTES_PREFIX, StringComparison.Ordinal)
                && !refName.EndsWith(Const.Repository.Branching.HEAD_SUFFIX, StringComparison.Ordinal))
            {
                var rest = refName.Substring(Const.Repository.Branching.REMOTES_PREFIX.Length);
                var slash = rest.IndexOf('/');
                if (slash <= 0 || slash == rest.Length - 1)
                    continue;

                branches.Add(new BranchInfo(new Branch(rest.Substring(slash + 1)), new Remote(rest.Substring(0, slash)), false, commit, upstream));
            }
        }

        return branches;
    }
}
