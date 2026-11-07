// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using CliWrap.Builders;

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

namespace GitGud.Repositories.Stashing;

public sealed class StashOperations : IStashOperations
{
    private readonly ICommandExecutor _executor;

    public StashOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<IReadOnlyList<StashInfo>, IError>> ListStashes(CancellationToken cancellationToken = default)
    {
        var output = await _executor.ExecuteAsync(Const.Repository.GIT_STASH, Const.Repository.Stashing.Errors.LIST_FAILED, args =>
        {
            args.AddArgument(Const.STASH, true);
            args.AddArgument(Const.Repository.Stashing.LIST, true);
            args.AddArgument(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, Const.Repository.Stashing.FORMAT, Const.Repository.Stashing.LIST_FORMAT), true);
        }, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<StashInfo>, IError>.None(output.Handle());

        return Result<IReadOnlyList<StashInfo>, IError>.Some(ParseStashes(output.Unwrap()));
    }

    public Task<Result<Nothing, IError>> Stash(StashPushOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new StashPushOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_STASH, Const.Repository.Stashing.Errors.PUSH_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> ApplyStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default)
        => RunEntry(Const.Repository.Stashing.APPLY, Const.Repository.Stashing.Errors.APPLY_FAILED, options, true, cancellationToken);

    public Task<Result<Nothing, IError>> PopStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default)
        => RunEntry(Const.Repository.Stashing.POP, Const.Repository.Stashing.Errors.POP_FAILED, options, true, cancellationToken);

    public Task<Result<Nothing, IError>> DropStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default)
        => RunEntry(Const.Repository.Stashing.DROP, Const.Repository.Stashing.Errors.DROP_FAILED, options, false, cancellationToken);

    public Task<Result<Nothing, IError>> ClearStashes(CancellationToken cancellationToken = default)
        => _executor.RunAsync(Const.Repository.GIT_STASH, Const.Repository.Stashing.Errors.CLEAR_FAILED, args =>
        {
            args.AddArgument(Const.STASH, true);
            args.AddArgument(Const.Repository.Stashing.CLEAR, true);
        }, null, cancellationToken);

    public Task<Result<Nothing, IError>> CreateBranchFromStash(StashBranchOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_STASH, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(
            Const.Repository.GIT_STASH, Const.Repository.Stashing.Errors.BRANCH_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    private Task<Result<Nothing, IError>> RunEntry(
        string subcommand,
        string failureMessage,
        StashEntryOptions? options,
        bool supportsIndex,
        CancellationToken cancellationToken)
    {
        options ??= new StashEntryOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(Const.Repository.GIT_STASH, failureMessage, args =>
        {
            args.AddArgument(Const.STASH, true);
            args.AddArgument(subcommand, true);
            options.Apply(args, supportsIndex);
        }, null, cancellationToken);
    }

    internal static IReadOnlyList<StashInfo> ParseStashes(string? output)
    {
        var stashes = new List<StashInfo>();
        if (string.IsNullOrEmpty(output))
            return stashes;

        var lines = output!.Split(Const.Repository.Stashing.LINE_SEPARATOR);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd(Const.Repository.Stashing.CARRIAGE_RETURN);
            if (line.Length == 0)
                continue;

            var fields = line.Split(new[] { Const.Repository.Stashing.FIELD_SEPARATOR }, Const.Repository.Stashing.FIELD_COUNT);
            if (fields.Length < Const.Repository.Stashing.FIELD_COUNT)
                continue;

            var reference = fields[0];
            if (!reference.StartsWith(Const.Repository.Stashing.REF_PREFIX, StringComparison.Ordinal)
                || reference[reference.Length - 1] != Const.Repository.Stashing.REF_SUFFIX)
                continue;

            var start = Const.Repository.Stashing.REF_PREFIX.Length;
            if (!int.TryParse(reference.Substring(start, reference.Length - start - 1), out var index))
                continue;

            stashes.Add(new StashInfo(index, fields[1], fields[2]));
        }

        return stashes;
    }
}
