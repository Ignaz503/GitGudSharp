// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Repositories.Errors;
using GitGud.Utility;

using System.Globalization;

namespace GitGud.Repositories.Commiting;

/// <summary>
/// Options for <c>git cherry-pick</c>. Yes/no option pairs (--x / --no-x) are <see cref="Nullable{T}"/> booleans:
/// <c>null</c> omits the option, <c>true</c> emits <c>--x</c>, <c>false</c> emits <c>--no-x</c>.
/// Examples:
/// <code>new CherryPickOptions { Commits = new[] { "abc123" } }                                  // git cherry-pick -- abc123
/// new CherryPickOptions { Commits = new[] { "master~2", "next" }, AppendSource = true } // git cherry-pick -x -- master~2 next
/// new CherryPickOptions { Commits = new[] { "merge1" }, Mainline = 1 }                  // git cherry-pick -m 1 -- merge1
/// new CherryPickOptions { Commits = new[] { "a", "b" }, NoCommit = true }               // git cherry-pick -n -- a b
/// new CherryPickOptions { Commits = new[] { "a..b" }, Empty = CherryPickEmptyMode.Drop } // git cherry-pick --empty=drop -- a..b
/// new CherryPickOptions { Commits = new[] { "abc123" }, GpgSign = false }               // git cherry-pick --no-gpg-sign -- abc123
/// new CherryPickOptions { Commits = new[] { "abc123" }, GpgKeyId = "ABCD1234" }         // git cherry-pick -SABCD1234 -- abc123
/// new CherryPickOptions { Sequencer = CherryPickSequencerAction.Continue }              // git cherry-pick --continue</code>
/// </summary>
public sealed class CherryPickOptions : IOperationOptions
{
    const int EditIndex = 0;
    const int CleanupIndex = 1;
    const int AppendSourceIndex = 2;
    const int ReleaseIndex = 3;
    const int MainlineIndex = 4;
    const int NoCommitIndex = 5;
    const int SignoffIndex = 6;
    const int GpgIndex = 7;
    const int FastForwardIndex = 8;
    const int AllowEmptyIndex = 9;
    const int AllowEmptyMessageIndex = 10;
    const int EmptyIndex = 11;
    const int KeepRedundantIndex = 12;
    const int StrategyIndex = 13;
    const int RerereIndex = 14;
    const int SequencerIndex = 15;
    const int Count = 16;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[Count];

    /// <summary>Commits (or ranges, e.g. <c>master..next</c>) to cherry-pick. Required unless <see cref="Sequencer"/> is set.</summary>
    public IReadOnlyList<string>? Commits { get; init; }

    /// <summary>Merge strategy options passed as -X&lt;option&gt; (e.g. <c>theirs</c>).</summary>
    public IReadOnlyList<string>? StrategyOptions { get; init; }

    /// <summary>Key id for -S&lt;keyid&gt;. Blank values are ignored. Invalid together with <c>GpgSign = false</c>.</summary>
    public string GpgKeyId { get; init; } = string.Empty;

    /// <summary>Lets the commit message be edited before committing (-e).</summary>
    public bool Edit { get => Has(EditIndex); init => SetFlag(EditIndex, value, Const.Repository.CherryPickArguments.EDIT); }

    /// <summary>Commit message cleanup mode (--cleanup=&lt;mode&gt;). Blank values are ignored.</summary>
    public string Cleanup
    {
        get => GetSecond(CleanupIndex);
        init => SetValue(CleanupIndex, Const.Repository.CherryPickArguments.CLEANUP, value);
    }

    /// <summary>Appends "(cherry picked from commit ...)" to the message (-x).</summary>
    public bool AppendSource { get => Has(AppendSourceIndex); init => SetFlag(AppendSourceIndex, value, Const.Repository.CherryPickArguments.APPEND_SOURCE); }

    /// <summary>Legacy no-op option (-r).</summary>
    public bool Release { get => Has(ReleaseIndex); init => SetFlag(ReleaseIndex, value, Const.Repository.CherryPickArguments.NO_OP_RELEASE); }

    /// <summary>Parent number (starting at 1) of the mainline when cherry-picking a merge (-m). Zero means unset.</summary>
    public int Mainline
    {
        get => arguments[MainlineIndex].Transform(
            static v => int.TryParse(v.Second, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 0,
            static () => 0);
        init => arguments[MainlineIndex] = value != 0
            ? Optional<Pair>.Some(new Pair(Const.Repository.CherryPickArguments.MAINLINE, value.ToString(CultureInfo.InvariantCulture)))
            : Optional<Pair>.None;
    }

    /// <summary>Applies changes to the working tree and index without committing (-n).</summary>
    public bool NoCommit { get => Has(NoCommitIndex); init => SetFlag(NoCommitIndex, value, Const.Repository.CherryPickArguments.NO_COMMIT); }

    /// <summary>Adds a Signed-off-by trailer (-s).</summary>
    public bool Signoff { get => Has(SignoffIndex); init => SetFlag(SignoffIndex, value, Const.Repository.CherryPickArguments.SIGNOFF); }

    /// <summary>true: -S (or -S&lt;keyid&gt; when <see cref="GpgKeyId"/> is set), false: --no-gpg-sign, null: omitted unless <see cref="GpgKeyId"/> is set.</summary>
    public bool? GpgSign
    {
        get => GetToggle(GpgIndex, Const.Repository.CherryPickArguments.GPG_SIGN);
        init => SetToggle(GpgIndex, value, Const.Repository.CherryPickArguments.GPG_SIGN, Const.Repository.CherryPickArguments.NO_GPG_SIGN);
    }

    /// <summary>Fast-forwards when HEAD is the parent of the picked commit (--ff).</summary>
    public bool FastForward { get => Has(FastForwardIndex); init => SetFlag(FastForwardIndex, value, Const.Repository.CherryPickArguments.FF); }

    /// <summary>Keeps initially empty commits (--allow-empty).</summary>
    public bool AllowEmpty { get => Has(AllowEmptyIndex); init => SetFlag(AllowEmptyIndex, value, Const.Repository.CherryPickArguments.ALLOW_EMPTY); }

    /// <summary>Allows commits with empty messages (--allow-empty-message).</summary>
    public bool AllowEmptyMessage { get => Has(AllowEmptyMessageIndex); init => SetFlag(AllowEmptyMessageIndex, value, Const.Repository.CherryPickArguments.ALLOW_EMPTY_MESSAGE); }

    /// <summary>Handling of redundant commits (--empty=drop|keep|stop). <see cref="CherryPickEmptyMode.Default"/> omits the option.</summary>
    public CherryPickEmptyMode Empty
    {
        get => arguments[EmptyIndex].Transform(
            static v => v.Second switch
            {
                Const.Repository.CherryPickArguments.EMPTY_DROP => CherryPickEmptyMode.Drop,
                Const.Repository.CherryPickArguments.EMPTY_KEEP => CherryPickEmptyMode.Keep,
                Const.Repository.CherryPickArguments.EMPTY_STOP => CherryPickEmptyMode.Stop,
                _ => CherryPickEmptyMode.Default
            },
            static () => CherryPickEmptyMode.Default);
        init
        {
            var mode = value switch
            {
                CherryPickEmptyMode.Drop => Const.Repository.CherryPickArguments.EMPTY_DROP,
                CherryPickEmptyMode.Keep => Const.Repository.CherryPickArguments.EMPTY_KEEP,
                CherryPickEmptyMode.Stop => Const.Repository.CherryPickArguments.EMPTY_STOP,
                _ => null
            };
            arguments[EmptyIndex] = mode is not null
                ? Optional<Pair>.Some(new Pair(Const.Repository.CherryPickArguments.EMPTY, mode))
                : Optional<Pair>.None;
        }
    }

    /// <summary>Deprecated synonym for <c>--empty=keep</c> (--keep-redundant-commits).</summary>
    public bool KeepRedundantCommits { get => Has(KeepRedundantIndex); init => SetFlag(KeepRedundantIndex, value, Const.Repository.CherryPickArguments.KEEP_REDUNDANT_COMMITS); }

    /// <summary>Merge strategy (--strategy=&lt;strategy&gt;). Blank values are ignored.</summary>
    public string Strategy
    {
        get => GetSecond(StrategyIndex);
        init => SetValue(StrategyIndex, Const.Repository.CherryPickArguments.STRATEGY, value);
    }

    /// <summary>true: --rerere-autoupdate, false: --no-rerere-autoupdate, null: omitted.</summary>
    public bool? RerereAutoUpdate
    {
        get => GetToggle(RerereIndex, Const.Repository.CherryPickArguments.RERERE_AUTOUPDATE);
        init => SetToggle(RerereIndex, value, Const.Repository.CherryPickArguments.RERERE_AUTOUPDATE, Const.Repository.CherryPickArguments.NO_RERERE_AUTOUPDATE);
    }

    /// <summary>Continue, skip, abort or quit an in-progress cherry-pick. Cannot be combined with <see cref="Commits"/>.</summary>
    public CherryPickSequencerAction Sequencer
    {
        get => arguments[SequencerIndex].Transform(
            static v => v.First switch
            {
                Const.Repository.CherryPickArguments.CONTINUE => CherryPickSequencerAction.Continue,
                Const.Repository.CherryPickArguments.SKIP => CherryPickSequencerAction.Skip,
                Const.Repository.CherryPickArguments.ABORT => CherryPickSequencerAction.Abort,
                Const.Repository.CherryPickArguments.QUIT => CherryPickSequencerAction.Quit,
                _ => CherryPickSequencerAction.None
            },
            static () => CherryPickSequencerAction.None);
        init
        {
            var token = value switch
            {
                CherryPickSequencerAction.Continue => Const.Repository.CherryPickArguments.CONTINUE,
                CherryPickSequencerAction.Skip => Const.Repository.CherryPickArguments.SKIP,
                CherryPickSequencerAction.Abort => Const.Repository.CherryPickArguments.ABORT,
                CherryPickSequencerAction.Quit => Const.Repository.CherryPickArguments.QUIT,
                _ => null
            };
            arguments[SequencerIndex] = token is not null
                ? Optional<Pair>.Some(new Pair(token))
                : Optional<Pair>.None;
        }
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.Repository.CHERRY_PICK, true);

        var gpgKey = GpgKeyId;
        var hasGpgKey = !string.IsNullOrWhiteSpace(gpgKey);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];

            if (i == GpgIndex)
            {
                // -S<keyid> must be stuck to the option; a key id turns -S into -S<keyid>.
                if (hasGpgKey && (!optional.HasValue || optional.Value.First == Const.Repository.CherryPickArguments.GPG_SIGN))
                    argumentBuilder.AddArgument(string.Concat(Const.Repository.CherryPickArguments.GPG_SIGN, gpgKey), true);
                else if (optional.HasValue)
                    argumentBuilder.AddArgument(optional.Value.First!, true);
                continue;
            }

            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (!pair.TwoValues)
            {
                argumentBuilder.AddArgument(pair.First!, true);
            }
            else if (i == MainlineIndex)
            {
                argumentBuilder.AddArgument(pair.First!, true);
                argumentBuilder.AddArgument(pair.Second!, true);
            }
            else
            {
                // --cleanup=<mode>, --empty=<mode>, --strategy=<name>
                argumentBuilder.AddArgument(string.Concat(pair.First, pair.Second), true);
            }
        }

        var strategyOptions = StrategyOptions;
        if (strategyOptions is not null)
        {
            for (var i = 0; i < strategyOptions.Count; i++)
                argumentBuilder.AddArgument(string.Concat(Const.Repository.CherryPickArguments.STRATEGY_OPTION, strategyOptions[i]), true);
        }

        var commits = Commits;
        if (commits is not null && commits.Count > 0)
        {
            argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
            for (var i = 0; i < commits.Count; i++)
                argumentBuilder.AddArgument(commits[i], true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var commits = Commits;
        var hasCommits = commits is not null && commits.Count > 0;
        var hasSequencer = Has(SequencerIndex);
        string? error = null;

        if (hasSequencer && hasCommits)
            error = Const.Repository.CherryPickErrors.SEQUENCER_CONFLICT;
        else if (!hasSequencer && !hasCommits)
            error = Const.Repository.CherryPickErrors.COMMITS_REQUIRED;
        else if (Has(MainlineIndex) && Mainline <= 0)
            error = Const.Repository.CherryPickErrors.MAINLINE_INVALID;
        else if (GpgSign is false && !string.IsNullOrWhiteSpace(GpgKeyId))
            error = Const.Repository.CherryPickErrors.GPG_KEY_WITHOUT_SIGN;

        if (error is null && hasCommits)
        {
            for (var i = 0; i < commits!.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(commits[i]))
                {
                    error = Const.Repository.CherryPickErrors.COMMIT_INVALID;
                    break;
                }
            }
        }

        var strategyOptions = StrategyOptions;
        if (error is null && strategyOptions is not null)
        {
            for (var i = 0; i < strategyOptions.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(strategyOptions[i]))
                {
                    error = Const.Repository.CherryPickErrors.STRATEGY_OPTION_INVALID;
                    break;
                }
            }
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_CHERRY_PICK, error));

        return Result<Nothing, IError>.Some(new());
    }

    private bool Has(int index) => arguments[index].HasValue;

    private void SetFlag(int index, bool value, string token)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(token))
            : Optional<Pair>.None;

    private void SetValue(int index, string prefix, string value)
        => arguments[index] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(prefix, value))
            : Optional<Pair>.None;

    private string GetSecond(int index)
        => arguments[index].Transform(static v => v.Second ?? string.Empty, static () => string.Empty);

    private bool? GetToggle(int index, string enabledToken)
    {
        var optional = arguments[index];
        if (!optional.HasValue)
            return null;

        return optional.Value.First == enabledToken;
    }

    private void SetToggle(int index, bool? value, string enabledToken, string disabledToken)
        => arguments[index] = value is null
            ? Optional<Pair>.None
            : Optional<Pair>.Some(new Pair(value.Value ? enabledToken : disabledToken));
}