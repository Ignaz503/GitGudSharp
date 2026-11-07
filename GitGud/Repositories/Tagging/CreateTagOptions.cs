// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Repositories.Operations;
using GitGud.Utility;
using TagConst = GitGud.Constants.Const.Repository.Tagging;

namespace GitGud.Repositories.Tagging;

public sealed class CreateTagOptions : IOperationOptions
{


    const int AnnotateIndex = 0;
    const int SignIndex = 1;
    const int SigningKeyIndex = 2;

    public const int ForceIndex = 3;
    public const int MessageIndex = 4;
    const int MessageFileIndex = 5;
    const int CleanupIndex = 6;
    const int CreateReflogIndex = 7;

    const int NameIndex = 8;
    const int TargetIndex = 9;

    private readonly Optional<Pair>[] arguments
        = new Optional<Pair>[10];

    public Tag Name 
    { 
        get => new(arguments[NameIndex].Value.First ?? string.Empty);
        init => arguments[NameIndex] = Optional<Pair>.Some(new Pair(value));
    }

    /// <summary>Commit or object to tag. Defaults to HEAD.</summary>
    public string? Target 
    {
        get => arguments[TargetIndex].Transform(static v => v.First, static () => default!);
        init => arguments[TargetIndex] = value is string val 
            ? Optional<Pair>.Some(new Pair(val))
            : Optional<Pair>.None;
    }

    /// <summary>Creates an unsigned annotated tag (-a). Requires a message, file or trailers.</summary>
    public bool Annotate
    {
        get => arguments[AnnotateIndex].HasValue;
        init => arguments[AnnotateIndex] = value
            ? Optional<Pair>.Some(new Pair(TagConst.ANNOTATE))
            : Optional<Pair>.None;
    }

    /// <summary>Creates a signed tag with the default key (-s).</summary>
    public bool Sign
    {
        get => arguments[SignIndex].HasValue;
        init => arguments[SignIndex] = value
            ? Optional<Pair>.Some(new Pair(TagConst.SIGN))
            : Optional<Pair>.None;
    }

    /// <summary>Creates a signed tag with the given key (-u).</summary>
    public string? SigningKey
    {
        get => arguments[SigningKeyIndex].Transform( static v => v.Second, static () => default!);
        init
        {
            if(value is string val) 
            {
                arguments[SigningKeyIndex] = Optional<Pair>.Some(new (TagConst.LOCAL_USER, val));
            }
            else 
            {
                arguments[SigningKeyIndex] = Optional<Pair>.None;
            }
        }
    }

    /// <summary>Replaces an existing tag (-f).</summary>
    public bool Force 
    { 
        get => arguments[ForceIndex].Transform(static v => true, static () => false);
        init => arguments[ForceIndex] = value
            ? Optional<Pair>.Some(new Pair(TagConst.FORCE))
            : Optional<Pair>.None;
    }

    /// <summary>Tag message (-m). Implies an annotated tag.</summary>
    public string? Message 
    {
        get => arguments[MessageIndex].Transform(static v => v.Second, static () => default!);
        init => arguments[MessageIndex] = value is string val
            ? Optional<Pair>.Some(new Pair(TagConst.MESSAGE, val))
            : Optional<Pair>.None;
    }

    /// <summary>File to read the tag message from (-F). Implies an annotated tag.</summary>
    public FilePath? MessageFile 
    { 
        get => arguments[MessageFileIndex].Transform(static v => new FilePath(v.Second!), static () => default);
        init => arguments[MessageFileIndex] = value is FilePath val
            ? Optional<Pair>.Some(new Pair(TagConst.FILE, val.ToString()))
            : Optional<Pair>.None;

    }

    private readonly IEnumerable<string> _trailers = Enumerable.Empty<string>();

    /// <summary>Trailers to apply, formatted as token=value or token:value.</summary>
    public IEnumerable<string> Trailers
    {
        get => _trailers;
        init => _trailers = value;
    }
    
    public TagCleanupMode Cleanup 
    {
        get 
        {
            return arguments[CleanupIndex].Transform(static v => v switch
            {
                var s when s.First?.EndsWith(TagConst.CLEANUP_VERBATIM) ?? false => TagCleanupMode.Verbatim,
                var s when s.First?.EndsWith(TagConst.CLEANUP_WHITESPACE) ?? false => TagCleanupMode.Whitespace,
                var s when s.First?.EndsWith(TagConst.CLEANUP_STRIP) ?? false => TagCleanupMode.Strip,
                _ => TagCleanupMode.Default
            }, static () => TagCleanupMode.Default);
        }
        init 
        {
            arguments[CleanupIndex] = value switch
             {
                TagCleanupMode.Verbatim => Optional<Pair>.Some(new(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, TagConst.CLEANUP, TagConst.CLEANUP_VERBATIM))),
                TagCleanupMode.Whitespace =>
                    Optional<Pair>.Some(new(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, TagConst.CLEANUP, TagConst.CLEANUP_WHITESPACE))), 
                    
                TagCleanupMode.Strip =>
                    Optional<Pair>.Some(new(string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, TagConst.CLEANUP, TagConst.CLEANUP_STRIP))),
                _ => Optional<Pair>.None
             };
        }
    }

    public bool CreateReflog 
    {
        get => arguments[CreateReflogIndex].Transform(static v => true, static () => false);
        set => arguments[CreateReflogIndex] = value
            ? Optional<Pair>.Some(new(Flag(TagConst.CREATE_REFLOG)))
            : Optional<Pair>.None;
    }

    private static string Flag(string name)
        => string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT, name);

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        for (int i = 0; i < CleanupIndex; i++)
        {
            var value = arguments[i];
            AddArgument(argumentBuilder, value);
        }

        //interlope trailers
        foreach (var trailer in _trailers)
        {
            argumentBuilder.Flag(TagConst.TRAILER);
            argumentBuilder.AddArgument(trailer, true);
        }

        for (int i = CleanupIndex; i < arguments.Length; i++)
        {
            var value = arguments[i];
           AddArgument(argumentBuilder, value);
        }
    }

    private void AddArgument(CliWrapArgumentBuilder argumentBuilder, Optional<Pair> value)
    {
        if (value.HasValue)
        {
            var pair = value.Value;
            argumentBuilder.AddArgument(pair.First!, true);
            if (pair.TwoValues)
            {
                argumentBuilder.AddArgument(pair.Second!, true);
            }
        }
    }

    public Result<Nothing, IError> Validate()
    {
        string? error;
        var hasFile = MessageFile is not null && !string.IsNullOrWhiteSpace(MessageFile.Value.ToString());
        var hasTrailers = Trailers is not null && Trailers.Any();
        var hasMessage = Message is not null;
        var signed = Sign || !string.IsNullOrWhiteSpace(SigningKey);
        var annotated = Annotate || signed || hasMessage || hasFile || hasTrailers;

        if (!arguments[NameIndex].HasValue || !Name.IsValid)
            error = TagConst.Errors.NAME_INVALID;
        else if (Target is not null && (string.IsNullOrWhiteSpace(Target) || Target[0] == Const.Repository.Branching.OPTION_PREFIX))
            error = TagConst.Errors.TARGET_INVALID;
        else if (arguments[SigningKeyIndex].HasValue && string.IsNullOrWhiteSpace(SigningKey))
            error = TagConst.Errors.VALUE_INVALID;
        else if (Sign && SigningKey is not null)
            error = TagConst.Errors.SIGN_CONFLICT;
        else if (hasMessage && hasFile)
            error = TagConst.Errors.MESSAGE_CONFLICT;
        else if (annotated && !hasMessage && !hasFile && !hasTrailers)
            error = TagConst.Errors.MESSAGE_REQUIRED;
        else
            error = ValidateValues(Trailers, false);

        if(error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_TAG, error));

        return Result<Nothing, IError>.Some(new());

    }

    private static string? ValidateValues(IEnumerable<string>? values, bool rejectDash)
    {
        if (values is null)
            return null;

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value) || (rejectDash && value[0] == Const.Repository.Branching.OPTION_PREFIX))
                return TagConst.Errors.VALUE_INVALID;
        }

        return null;
    }

}
