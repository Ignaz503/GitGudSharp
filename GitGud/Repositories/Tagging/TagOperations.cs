// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap.Builders;

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;

using System.Threading;

using TagConst = GitGud.Constants.Const.Repository.Tagging;

namespace GitGud.Repositories.Tagging;

public sealed class TagOperations : ITagOperations
{
    private readonly ICommandExecutor _executor;

    public TagOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<IReadOnlyList<TagInfo>, IError>> ListTags(ListTagsOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ListTagsOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<IReadOnlyList<TagInfo>, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_TAG, TagConst.Errors.LIST_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<IReadOnlyList<TagInfo>, IError>.None(output.Handle());

        return Result<IReadOnlyList<TagInfo>, IError>.Some(ParseTags(output.Unwrap()));
    }

    public Task<Result<Nothing, IError>> CreateTag(CreateTagOptions options, CancellationToken cancellationToken = default)
    {
        if (options is null)
            return Task.FromResult(Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_TAG, Const.Repository.OperationErrors.OPTIONS_REQUIRED)));

        var validation = options.Validate();
        if (validation.HasNone)
            return Task.FromResult(validation);

        return _executor.RunAsync(Const.Repository.GIT_TAG, TagConst.Errors.CREATE_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken);
    }

    public Task<Result<Nothing, IError>> DeleteTag(DeleteTagOptions options, CancellationToken cancellationToken = default)
    {
        var error = ValidateTags(options?.Tags, options is null);
        return _executor.RunAsync(Const.Repository.GIT_TAG, TagConst.Errors.DELETE_FAILED, args =>
        {
            args.AddArgument(TagConst.DELETE, true);
            AddTags(args, options!.Tags!);
        }, error, cancellationToken);
    }

    public Task<Result<Nothing, IError>> VerifyTag(VerifyTagOptions options, CancellationToken cancellationToken = default)
    {
        var error = ValidateTags(options?.Tags, options is null);
        return _executor.RunAsync(Const.Repository.GIT_TAG, TagConst.Errors.VERIFY_FAILED, args =>
        {
            args.AddArgument(TagConst.VERIFY, true);
            AddTags(args, options!.Tags!);
        }, error, cancellationToken);
    }

    private static void AddTags(CliWrapArgumentBuilder args, IReadOnlyList<Tag> tags)
    {
        for (var i = 0; i < tags.Count; i++)
            args.AddArgument(tags[i].ToString(), true);
    }

    private static string? ValidateTags(IReadOnlyList<Tag>? tags, bool optionsMissing)
    {
        if (optionsMissing)
            return Const.Repository.OperationErrors.OPTIONS_REQUIRED;
        if (tags is null || tags.Count == 0)
            return TagConst.Errors.NAMES_REQUIRED;

        for (var i = 0; i < tags.Count; i++)
        {
            if (!tags[i].IsValid)
                return TagConst.Errors.NAME_INVALID;
        }

        return null;
    }

    private static IReadOnlyList<TagInfo> ParseTags(string? output)
    {
        var tags = new List<TagInfo>();
        if (string.IsNullOrEmpty(output))
            return tags;

        var lines = output!.Split(TagConst.LINE_SEPARATOR);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (line.Length == 0)
                continue;

            var fields = line.Split(TagConst.FIELD_SEPARATOR);
            if (fields.Length < TagConst.FIELD_COUNT)
                continue;

            var annotated = fields[1] == TagConst.OBJECT_TYPE_TAG;
            var target = annotated && fields[3].Length > 0 ? fields[3] : fields[2];
            tags.Add(new TagInfo(new Tag(fields[0]), annotated ? TagKind.Annotated : TagKind.Lightweight, fields[2], target, fields[4]));
        }

        return tags;
    }
}
