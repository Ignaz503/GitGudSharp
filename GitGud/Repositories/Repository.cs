// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Erorrs;
using GitGud.Repositories.Administration;
using GitGud.Repositories.Applying;
using GitGud.Repositories.Branching;
using GitGud.Repositories.Commiting;
using GitGud.Repositories.Diffing;
using GitGud.Repositories.Logging;
using GitGud.Repositories.Merging;
using GitGud.Repositories.Remotes;
using GitGud.Repositories.Reseting;
using GitGud.Repositories.Restoring;
using GitGud.Repositories.Stashing;
using GitGud.Repositories.Submodules;
using GitGud.Repositories.Tagging;
using GitGud.Utility;

namespace GitGud.Repositories;

public sealed class Repository : IRepository
{
    private readonly IRemoteOperations _remote;
    private readonly ICommitOperations _commits;
    private readonly IBranchOperations _branches;
    private readonly IStashOperations _stashes;
    private readonly IMergeOperations _merges;
    private readonly IRestoreOperations _restores;
    private readonly IResetOperations _resets;
    private readonly IDiffOperations _diffs;
    private readonly ITagOperations _tags;
    private readonly ILogOperations _logs;
    private readonly ICleanOperations _cleans;
    private readonly IGcOperations _gcs;
    private readonly IFsCheckOperations _fsChecks;

    private readonly ISubmoduleOperations _submodule;

    // new field
    private readonly IApplyOperations _applies;

    public Repository(DirectoryPath path, bool isBare)
        : this(path, isBare, new CommandExecutor(path))
    {
    }   

    public Repository(DirectoryPath path, bool isBare, ICommandExecutor executor)
        : this(path, isBare, new RemoteOperations(executor), new CommitOperations(executor), new BranchOperations(executor), new StashOperations(executor), new MergeOperations(executor, path), new RestoreOperations(executor), new ResetOperations(executor), new DiffOperations(executor), new TagOperations(executor), new LogOperations(executor), new CleanOperations(executor), new GcOperations(executor), new FsCheckOperations(executor), new ApplyOperations(executor), new SubmoduleOperations(executor))
    { }


    public Repository(DirectoryPath path, bool isBare, IRemoteOperations remote, ICommitOperations commits, IBranchOperations branches, IStashOperations stashes, IMergeOperations merges, IRestoreOperations restores, IResetOperations resets, IDiffOperations diffs, ITagOperations tags, ILogOperations logs, ICleanOperations cleans, IGcOperations gcs, IFsCheckOperations fsChecks, IApplyOperations applies, ISubmoduleOperations submodule)

    {
        Path = path;
        IsBare = isBare;
        _remote = remote;
        _commits = commits;
        _branches = branches;
        _stashes = stashes;
        _merges = merges;
        _restores = restores;
        _resets = resets;
        _diffs = diffs;
        _tags = tags;
        _logs = logs;
        _cleans = cleans;
        _gcs = gcs;
        _fsChecks = fsChecks;
        _applies = applies;
        _resets = resets;
        _diffs = diffs;
        _tags = tags;
        _logs = logs;
        _cleans = cleans;
        _gcs = gcs;
        _fsChecks = fsChecks;
        _applies = applies;
        _submodule = submodule;
    }

    public DirectoryPath Path { get; }
    public bool IsBare { get; }

    public Task<Result<Nothing, IError>> Fetch(FetchOptions? options = null, CancellationToken cancellationToken = default)
        => _remote.Fetch(options, cancellationToken);

    public Task<Result<Nothing, IError>> Pull(PullOptions? options = null, CancellationToken cancellationToken = default)
        => _remote.Pull(options, cancellationToken);

    public Task<Result<Nothing, IError>> Push(PushOptions? options = null, CancellationToken cancellationToken = default)
        => _remote.Push(options, cancellationToken);

    public Task<Result<Nothing, IError>> Add(AddOptions options, CancellationToken cancellationToken = default)
        => _commits.Add(options, cancellationToken);

    public Task<Result<Nothing, IError>> Commit(CommitOptions options, CancellationToken cancellationToken = default)
        => _commits.Commit(options, cancellationToken);

    public Task<Result<Nothing, IError>> CherryPick(CherryPickOptions options, CancellationToken cancellationToken = default)
        => _commits.CherryPick(options, cancellationToken);

    public Task<Result<Nothing, IError>> Rebase(RebaseOptions options, CancellationToken cancellationToken = default)
        => _commits.Rebase(options, cancellationToken);

    public Task<Result<IReadOnlyList<BranchInfo>, IError>> ListBranches(ListBranchesOptions? options = null, CancellationToken cancellationToken = default)
        => _branches.ListBranches(options, cancellationToken);

    public Task<Result<Nothing, IError>> CreateBranch(CreateBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.CreateBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> DeleteBranch(DeleteBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.DeleteBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> DeleteRemoteBranch(DeleteRemoteBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.DeleteRemoteBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> RenameBranch(RenameBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.RenameBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> RenameRemoteBranch(RenameRemoteBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.RenameRemoteBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> CopyBranch(CopyBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.CopyBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> SetUpstream(SetUpstreamOptions options, CancellationToken cancellationToken = default)
        => _branches.SetUpstream(options, cancellationToken);

    public Task<Result<Nothing, IError>> UnsetUpstream(UnsetUpstreamOptions? options = null, CancellationToken cancellationToken = default)
        => _branches.UnsetUpstream(options, cancellationToken);

    public Task<Result<Branch?, IError>> GetCurrentBranch(CancellationToken cancellationToken = default)
        => _branches.GetCurrentBranch(cancellationToken);

    public Task<Result<IReadOnlyList<Branch>, IError>> DeleteMergedBranches(DeleteMergedOptions options, CancellationToken cancellationToken = default)
        => _branches.DeleteMergedBranches(options, cancellationToken);

    public Task<Result<Nothing, IError>> CheckoutBranch(CheckoutBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.CheckoutBranch(options, cancellationToken);

    public Task<Result<Nothing, IError>> SwitchBranch(SwitchBranchOptions options, CancellationToken cancellationToken = default)
        => _branches.SwitchBranch(options, cancellationToken);

    public Task<Result<IReadOnlyList<LogEntry>, IError>> Log(LogOptions? options = null, CancellationToken cancellationToken = default)
        => _logs.Log(options, cancellationToken);

    public Task<Result<IReadOnlyList<TagInfo>, IError>> ListTags(ListTagsOptions? options = null, CancellationToken cancellationToken = default)
        => _tags.ListTags(options, cancellationToken);

    public Task<Result<Nothing, IError>> CreateTag(CreateTagOptions options, CancellationToken cancellationToken = default)
        => _tags.CreateTag(options, cancellationToken);

    public Task<Result<Nothing, IError>> DeleteTag(DeleteTagOptions options, CancellationToken cancellationToken = default)
        => _tags.DeleteTag(options, cancellationToken);

    public Task<Result<Nothing, IError>> VerifyTag(VerifyTagOptions options, CancellationToken cancellationToken = default)
        => _tags.VerifyTag(options, cancellationToken);

    public Task<Result<IReadOnlyList<StashInfo>, IError>> ListStashes(CancellationToken cancellationToken = default)
        => _stashes.ListStashes(cancellationToken);

    public Task<Result<Nothing, IError>> Stash(StashPushOptions? options = null, CancellationToken cancellationToken = default)
        => _stashes.Stash(options, cancellationToken);

    public Task<Result<Nothing, IError>> ApplyStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default)
        => _stashes.ApplyStash(options, cancellationToken);

    public Task<Result<Nothing, IError>> PopStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default)
        => _stashes.PopStash(options, cancellationToken);

    public Task<Result<Nothing, IError>> DropStash(StashEntryOptions? options = null, CancellationToken cancellationToken = default)
        => _stashes.DropStash(options, cancellationToken);

    public Task<Result<Nothing, IError>> ClearStashes(CancellationToken cancellationToken = default)
        => _stashes.ClearStashes(cancellationToken);

    public Task<Result<Nothing, IError>> CreateBranchFromStash(StashBranchOptions options, CancellationToken cancellationToken = default)
        => _stashes.CreateBranchFromStash(options, cancellationToken);

    public Task<Result<Nothing, IError>> Merge(MergeOptions options, CancellationToken cancellationToken = default)
        => _merges.Merge(options, cancellationToken);

    public Task<Result<Nothing, IError>> AbortMerge(CancellationToken cancellationToken = default)
        => _merges.AbortMerge(cancellationToken);

    public Task<Result<Nothing, IError>> Restore(RestoreOptions options, CancellationToken cancellationToken = default)
        => _restores.Restore(options, cancellationToken);

    public Task<Result<Nothing, IError>> Reset(ResetOptions? options = null, CancellationToken cancellationToken = default)
        => _resets.Reset(options, cancellationToken);

    public Task<Result<DiffResult, IError>> Diff(DiffOptions? options = null, CancellationToken cancellationToken = default)
        => _diffs.Diff(options, cancellationToken);

    public Task<Result<IReadOnlyList<string>, IError>> Clean(CleanOptions options, CancellationToken cancellationToken = default)
        => _cleans.Clean(options, cancellationToken);

    public Task<Result<Nothing, IError>> Gc(GcOptions? options = null, CancellationToken cancellationToken = default)
        => _gcs.Gc(options, cancellationToken);

    public Task<Result<FsCheckResult, IError>> FsCheck(FsCheckOptions? options = null, CancellationToken cancellationToken = default)
        => _fsChecks.FsCheck(options, cancellationToken);
    
    public Task<Result<string?, IError>> ApplyPatch(ApplyOptions options, CancellationToken cancellationToken = default)
        => _applies.ApplyPatch(options, cancellationToken);


    public Task<Result<Nothing, IError>> Add(SubmoduleAddOptions options, CancellationToken cancellationToken = default)
        => _submodule.Add(options, cancellationToken);
    public Task<Result<IReadOnlyList<SubmoduleStatusInfo>, IError>> Status(SubmoduleStatusOptions? options = null, CancellationToken cancellationToken = default)
        => _submodule.Status(options, cancellationToken);
    public Task<Result<Nothing, IError>> Init(SubmoduleInitOptions? options = null, CancellationToken cancellationToken = default)
        => _submodule.Init(options, cancellationToken);
    public Task<Result<Nothing, IError>> Deinit(SubmoduleDeinitOptions options, CancellationToken cancellationToken = default)
        => _submodule.Deinit(options, cancellationToken);
    public Task<Result<Nothing, IError>> Update(SubmoduleUpdateOptions? options = null, CancellationToken cancellationToken = default)
        => _submodule.Update(options, cancellationToken);
    public Task<Result<Nothing, IError>> SetBranch(SubmoduleSetBranchOptions options, CancellationToken cancellationToken = default)
        => _submodule.SetBranch(options, cancellationToken);
    public Task<Result<Nothing, IError>> SetUrl(SubmoduleSetUrlOptions options, CancellationToken cancellationToken = default)
        => _submodule.SetUrl(options, cancellationToken);
    public Task<Result<string?, IError>> Summary(SubmoduleSummaryOptions? options = null, CancellationToken cancellationToken = default)
        => _submodule.Summary(options, cancellationToken);
    public Task<Result<string?, IError>> Foreach(SubmoduleForeachOptions options, CancellationToken cancellationToken = default)
        => _submodule.Foreach(options, cancellationToken);
    public Task<Result<Nothing, IError>> Sync(SubmoduleSyncOptions? options = null, CancellationToken cancellationToken = default)
        => _submodule.Sync(options, cancellationToken);
    public Task<Result<Nothing, IError>> AbsorbGitDirs(SubmoduleAbsorbGitDirsOptions? options = null, CancellationToken cancellationToken = default)
        => _submodule.AbsorbGitDirs(options, cancellationToken);

}
