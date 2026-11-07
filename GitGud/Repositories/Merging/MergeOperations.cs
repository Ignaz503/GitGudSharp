// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap.Builders;

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using System.IO;
using System.Threading;

namespace GitGud.Repositories.Merging;

public sealed class MergeOperations : IMergeOperations
{
    private readonly ICommandExecutor _executor;
    private readonly DirectoryPath _path;

    public MergeOperations(ICommandExecutor executor, DirectoryPath path)
    {
        _executor = executor;
        _path = path;
    }

    public async Task<Result<Nothing, IError>> Merge(MergeOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Result<Nothing, IError>.None(
                new RepositoryValidationError(Const.Repository.GIT_MERGE, Const.Repository.OperationErrors.OPTIONS_REQUIRED));

        var validation = options.Validate();
        if (validation.HasNone)
            return validation;

        var merge = await _executor.RunAsync(
            Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.MERGE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        // `error is not null` is no longer needed: validation already returned above.
        if (merge.HasSome || merge.Handle() is RepositoryCancelledError)
            return merge;

        var conflicts = await ListConflictedPaths(cancellationToken).ConfigureAwait(false);
        if (conflicts.HasNone || conflicts.Unwrap().Count == 0)
            return merge;

        // `options!` becomes `options`: it is known to be non-null here.
        if (options.ConflictResolver is null)
            return Result<Nothing, IError>.None(new RepositoryMergeConflictError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.CONFLICTS, null, null));

        return await ResolveAsync(options, conflicts.Unwrap(), cancellationToken).ConfigureAwait(false);
    }

    public Task<Result<Nothing, IError>> AbortMerge(CancellationToken cancellationToken = default)
        => AbortAsync(MergeStrategy.Default, cancellationToken);

    private Task<Result<Nothing, IError>> AbortAsync(MergeStrategy strategy, CancellationToken cancellationToken)
        => _executor.RunAsync(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.ABORT_FAILED, args =>
        {
            if (strategy == MergeStrategy.Squash)
            {
                args.AddArgument(Const.Repository.Merging.RESET, true);
                args.Flag(Const.Repository.Merging.MERGE);
            }
            else
            {
                args.AddArgument(Const.Repository.Merging.MERGE, true);
                args.Flag(Const.Repository.Merging.ABORT);
            }
        }, null, cancellationToken);

    private async Task<Result<IReadOnlyList<string>, IError>> ListConflictedPaths(CancellationToken cancellationToken)
    {
        var output = await _executor.ExecuteAsync(Const.Repository.GIT_MERGE_CONFLICTS, Const.Repository.Merging.Errors.CONFLICTS_FAILED, args =>
        {
            args.AddArgument(Const.Repository.Merging.DIFF, true);
            args.AddArgument(Const.Repository.Merging.NULL_SEPARATOR_SHORT, true);
            args.Flag(Const.Repository.Merging.NAME_ONLY);
            args.Flag(Const.Repository.Merging.RELATIVE);
            args.Flag(Const.Repository.Merging.DIFF_FILTER_UNMERGED);
        }, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<string>, IError>.None(output.Handle());

        var paths = new List<string>();
        var text = output.Unwrap();
        if (!string.IsNullOrEmpty(text))
        {
            var parts = text!.Split(Const.Repository.Merging.NULL_SEPARATOR);
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                    paths.Add(parts[i]);
            }
        }

        return Result<IReadOnlyList<string>, IError>.Some(paths);
    }

    private async Task<Result<Nothing, IError>> ResolveAsync(MergeOptions options, IReadOnlyList<string> paths, CancellationToken cancellationToken)
    {
        string workingDirectory;
        try
        {
            workingDirectory = System.IO.Path.GetFullPath(_path.ToString());
        }
        catch (Exception)
        {
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_MERGE, Const.Repository.OperationErrors.REPOSITORY_PATH_INVALID));
        }

        for (var i = 0; i < paths.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                return Result<Nothing, IError>.None(new RepositoryCancelledError(Const.Repository.GIT_MERGE, Const.Repository.Errors.CANCELLED));

            var result = await ResolveFileAsync(options, workingDirectory, paths[i], cancellationToken).ConfigureAwait(false);
            if (result.HasNone)
                return result;
        }

        if (options.NoCommit || options.Strategy == MergeStrategy.Squash)
            return Result<Nothing, IError>.Some(new Nothing());

        return await _executor.RunAsync(Const.Repository.GIT_COMMIT, Const.Repository.OperationErrors.COMMIT_FAILED, args =>
        {
            args.AddArgument(Const.COMMIT, true);
            args.Flag(Const.Repository.Arguments.NO_EDIT);
        }, null, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<Nothing, IError>> ResolveFileAsync(MergeOptions options, string workingDirectory, string path, CancellationToken cancellationToken)
    {
        var stages = await ExtractStagesAsync(workingDirectory, path, cancellationToken).ConfigureAwait(false);
        if (stages.HasNone)
            return Result<Nothing, IError>.None(stages.Handle());

        var files = stages.Unwrap();
        try
        {
            var conflict = new MergeConflict(path, System.IO.Path.Combine(workingDirectory, path), files.Base, files.Local, files.Incoming);

            MergeResolution? resolution;
            try
            {
                resolution = await options.ConflictResolver!(conflict, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return Result<Nothing, IError>.None(new RepositoryCancelledError(Const.Repository.GIT_MERGE, Const.Repository.Errors.CANCELLED));
            }
            catch (Exception)
            {
                return Result<Nothing, IError>.None(new RepositoryExecutionError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.RESOLVER_FAILED));
            }

            if (resolution is null)
                return Result<Nothing, IError>.None(new RepositoryExecutionError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.RESOLVER_FAILED));

            if (resolution.Kind == MergeResolutionKind.Abort)
            {
                var abort = await AbortAsync(options.Strategy, cancellationToken).ConfigureAwait(false);
                if (abort.HasNone)
                    return abort;

                return Result<Nothing, IError>.None(new RepositoryCancelledError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.MERGE_ABORTED));
            }

            return await ApplyAsync(workingDirectory, conflict, resolution, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Delete(files.Base);
            Delete(files.Local);
            Delete(files.Incoming);
        }
    }

    private async Task<Result<Nothing, IError>> ApplyAsync(string workingDirectory, MergeConflict conflict, MergeResolution resolution, CancellationToken cancellationToken)
    {
        string? source = null;
        switch (resolution.Kind)
        {
            case MergeResolutionKind.Path:
                if (string.IsNullOrWhiteSpace(resolution.Path))
                    return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.RESOLUTION_PATH_INVALID));
                source = System.IO.Path.Combine(workingDirectory, resolution.Path!);
                if (!File.Exists(source))
                    return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.RESOLUTION_PATH_INVALID));
                break;
            case MergeResolutionKind.Local:
                source = conflict.LocalPath;
                break;
            case MergeResolutionKind.Incoming:
                source = conflict.IncomingPath;
                break;
        }

        var isRemoval = resolution.Kind != MergeResolutionKind.Content && source is null;
        if (isRemoval)
        {
            return await _executor.RunAsync(Const.Repository.GIT_MERGE_REMOVE, Const.Repository.Merging.Errors.RESOLVE_FAILED, args =>
            {
                args.AddArgument(Const.Repository.Merging.REMOVE, true);
                args.AddArgument(Const.Repository.Merging.FORCE_SHORT, true);
                args.AddArgument(Const.Repository.END_OF_OPTIONS, true);
                args.AddArgument(conflict.Path, true);
            }, null, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            if (resolution.Kind == MergeResolutionKind.Content)
            {
                using var writer = new StreamWriter(conflict.WorkingFilePath, false, new UTF8Encoding(false));
                await writer.WriteAsync(resolution.Content ?? string.Empty).ConfigureAwait(false);
            }
            else
                File.Copy(source!, conflict.WorkingFilePath, true);
        }
        catch (Exception)
        {
            return Result<Nothing, IError>.None(new RepositoryExecutionError(Const.Repository.GIT_MERGE, Const.Repository.Merging.Errors.RESOLUTION_IO));
        }

        return await _executor.RunAsync(Const.Repository.GIT_ADD, Const.Repository.Merging.Errors.RESOLVE_FAILED, args =>
        {
            args.AddArgument(Const.ADD, true);
            args.AddArgument(Const.Repository.END_OF_OPTIONS, true);
            args.AddArgument(conflict.Path, true);
        }, null, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<StageFiles, IError>> ExtractStagesAsync(string workingDirectory, string path, CancellationToken cancellationToken)
    {
        var output = await _executor.ExecuteAsync(Const.Repository.GIT_MERGE_STAGES, Const.Repository.Merging.Errors.STAGES_FAILED, args =>
        {
            args.AddArgument(Const.Repository.Merging.CHECKOUT_INDEX, true);
            args.AddArgument(Const.Repository.Merging.NULL_SEPARATOR_SHORT, true);
            args.Flag(Const.Repository.Merging.STAGE_ALL);
            args.Flag(Const.Repository.Merging.TEMP);
            args.AddArgument(Const.Repository.END_OF_OPTIONS, true);
            args.AddArgument(path, true);
        }, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<StageFiles, IError>.None(output.Handle());

        var record = output.Unwrap();
        var tab = record is null ? -1 : record.IndexOf(Const.Repository.Merging.FIELD_SEPARATOR);
        if (tab < 0)
            return Result<StageFiles, IError>.None(new RepositoryExecutionError(Const.Repository.GIT_MERGE_STAGES, Const.Repository.Merging.Errors.STAGE_PARSE_FAILED));

        var names = record!.Substring(0, tab).Split(Const.Repository.Merging.STAGE_SEPARATOR);
        if (names.Length != Const.Repository.Merging.STAGE_COUNT)
            return Result<StageFiles, IError>.None(new RepositoryExecutionError(Const.Repository.GIT_MERGE_STAGES, Const.Repository.Merging.Errors.STAGE_PARSE_FAILED));

        return Result<StageFiles, IError>.Some(new StageFiles(
            ToFullPath(workingDirectory, names[0]),
            ToFullPath(workingDirectory, names[1]),
            ToFullPath(workingDirectory, names[2])));
    }

    private static string? ToFullPath(string workingDirectory, string name)
        => name == Const.Repository.Merging.MISSING_STAGE ? null : System.IO.Path.Combine(workingDirectory, name);

    private static void Delete(string? path)
    {
        if (path is null)
            return;

        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
        }
    }

    private readonly struct StageFiles
    {
        public StageFiles(string? baseFile, string? local, string? incoming)
        {
            Base = baseFile;
            Local = local;
            Incoming = incoming;
        }

        public string? Base { get; }
        public string? Local { get; }
        public string? Incoming { get; }
    }
}
