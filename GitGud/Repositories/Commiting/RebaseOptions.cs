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
/// Options for <c>git rebase</c>. Yes/no option pairs (--x / --no-x) are <see cref="Nullable{T}"/> booleans:
/// <c>null</c> omits the option, <c>true</c> emits <c>--x</c>, <c>false</c> emits <c>--no-x</c>.
/// Examples:
/// <code>new RebaseOptions { Upstream = "master" }                                         // git rebase -- master
/// new RebaseOptions { Upstream = "master", Branch = "topic" }                     // git rebase -- master topic
/// new RebaseOptions { Onto = "master", Upstream = "next", Branch = "topic" }      // git rebase --onto master -- next topic
/// new RebaseOptions { Interactive = true, Exec = new[] { "make test" }, Upstream = "HEAD~3" }
/// new RebaseOptions { Root = true, Onto = "newbase", Branch = "topic" }           // git rebase --root --onto newbase -- topic
/// new RebaseOptions { Autostash = true, KeepEmpty = false, Upstream = "master" }  // git rebase --no-keep-empty --autostash -- master
/// new RebaseOptions { Mode = RebaseMode.Continue }                                // git rebase --continue</code>
/// </summary>
public sealed class RebaseOptions : IOperationOptions
{
    const int ModeIndex = 0;
    const int OntoIndex = 1;
    const int KeepBaseIndex = 2;
    const int ApplyIndex = 3;
    const int EmptyIndex = 4;
    const int KeepEmptyIndex = 5;
    const int ReapplyIndex = 6;
    const int AllowEmptyMessageIndex = 7;
    const int MergeIndex = 8;
    const int StrategyIndex = 9;
    const int RerereIndex = 10;
    const int GpgIndex = 11;
    const int QuietIndex = 12;
    const int VerboseIndex = 13;
    const int StatIndex = 14;
    const int VerifyIndex = 15;
    const int ContextIndex = 16;
    const int ForceRebaseIndex = 17;
    const int ForkPointIndex = 18;
    const int IgnoreWhitespaceIndex = 19;
    const int WhitespaceIndex = 20;
    const int CommitterDateIndex = 21;
    const int IgnoreDateIndex = 22;
    const int SignoffIndex = 23;
    const int InteractiveIndex = 24;
    const int RebaseMergesIndex = 25;
    const int RootIndex = 26;
    const int AutosquashIndex = 27;
    const int AutostashIndex = 28;
    const int RescheduleIndex = 29;
    const int UpdateRefsIndex = 30;
    const int Count = 31;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[Count];

    /// <summary>Upstream to compare against (positional). Defaults to the configured upstream.</summary>
    public string? Upstream { get; init; }

    /// <summary>Working branch (positional). Defaults to HEAD.</summary>
    public string? Branch { get; init; }

    /// <summary>Merge strategy options (-X, emitted as --strategy-option=&lt;option&gt;).</summary>
    public IReadOnlyList<string>? StrategyOptions { get; init; }

    /// <summary>Commands appended after each commit (--exec).</summary>
    public IReadOnlyList<string>? Exec { get; init; }

    /// <summary>Trailers appended to every commit message (--trailer=&lt;trailer&gt;).</summary>
    public IReadOnlyList<string>? Trailers { get; init; }

    /// <summary>Key id for -S&lt;keyid&gt;. Blank values are ignored. Invalid together with <c>GpgSign = false</c>.</summary>
    public string GpgKeyId { get; init; } = string.Empty;

    /// <summary>--continue, --skip, --abort, --quit, --edit-todo or --show-current-patch. Cannot be combined with anything else.</summary>
    public RebaseMode Mode
    {
        get => arguments[ModeIndex].Transform(
            static v => v.First switch
            {
                Const.Repository.RebaseArguments.CONTINUE => RebaseMode.Continue,
                Const.Repository.RebaseArguments.SKIP => RebaseMode.Skip,
                Const.Repository.RebaseArguments.ABORT => RebaseMode.Abort,
                Const.Repository.RebaseArguments.QUIT => RebaseMode.Quit,
                Const.Repository.RebaseArguments.EDIT_TODO => RebaseMode.EditTodo,
                Const.Repository.RebaseArguments.SHOW_CURRENT_PATCH => RebaseMode.ShowCurrentPatch,
                _ => RebaseMode.None
            },
            static () => RebaseMode.None);
        init
        {
            var token = value switch
            {
                RebaseMode.Continue => Const.Repository.RebaseArguments.CONTINUE,
                RebaseMode.Skip => Const.Repository.RebaseArguments.SKIP,
                RebaseMode.Abort => Const.Repository.RebaseArguments.ABORT,
                RebaseMode.Quit => Const.Repository.RebaseArguments.QUIT,
                RebaseMode.EditTodo => Const.Repository.RebaseArguments.EDIT_TODO,
                RebaseMode.ShowCurrentPatch => Const.Repository.RebaseArguments.SHOW_CURRENT_PATCH,
                _ => null
            };
            arguments[ModeIndex] = token is not null ? Optional<Pair>.Some(new Pair(token)) : Optional<Pair>.None;
        }
    }

    /// <summary>Starting point for the new commits (--onto &lt;newbase&gt;). Blank values are ignored.</summary>
    public string Onto
    {
        get => GetSecond(OntoIndex);
        init => SetValue(OntoIndex, Const.Repository.RebaseArguments.ONTO, value);
    }

    /// <summary>Uses the merge base of upstream and branch as the starting point (--keep-base).</summary>
    public bool KeepBase { get => Has(KeepBaseIndex); init => SetFlag(KeepBaseIndex, value, Const.Repository.RebaseArguments.KEEP_BASE); }

    /// <summary>Uses the apply backend (--apply).</summary>
    public bool Apply { get => Has(ApplyIndex); init => SetFlag(ApplyIndex, value, Const.Repository.RebaseArguments.APPLY); }

    /// <summary>Handling of commits that become empty (--empty=...).</summary>
    public RebaseEmptyMode Empty
    {
        get => arguments[EmptyIndex].Transform(
            static v => v.Second switch
            {
                Const.Repository.RebaseArguments.EMPTY_DROP => RebaseEmptyMode.Drop,
                Const.Repository.RebaseArguments.EMPTY_KEEP => RebaseEmptyMode.Keep,
                Const.Repository.RebaseArguments.EMPTY_STOP => RebaseEmptyMode.Stop,
                Const.Repository.RebaseArguments.EMPTY_ASK => RebaseEmptyMode.Ask,
                _ => RebaseEmptyMode.Default
            },
            static () => RebaseEmptyMode.Default);
        init
        {
            var mode = value switch
            {
                RebaseEmptyMode.Drop => Const.Repository.RebaseArguments.EMPTY_DROP,
                RebaseEmptyMode.Keep => Const.Repository.RebaseArguments.EMPTY_KEEP,
                RebaseEmptyMode.Stop => Const.Repository.RebaseArguments.EMPTY_STOP,
                RebaseEmptyMode.Ask => Const.Repository.RebaseArguments.EMPTY_ASK,
                _ => null
            };
            arguments[EmptyIndex] = mode is not null
                ? Optional<Pair>.Some(new Pair(Const.Repository.RebaseArguments.EMPTY, mode))
                : Optional<Pair>.None;
        }
    }

    /// <summary>true: --keep-empty, false: --no-keep-empty, null: omitted.</summary>
    public bool? KeepEmpty
    {
        get => GetToggle(KeepEmptyIndex, Const.Repository.RebaseArguments.KEEP_EMPTY);
        init => SetToggle(KeepEmptyIndex, value, Const.Repository.RebaseArguments.KEEP_EMPTY, Const.Repository.RebaseArguments.NO_KEEP_EMPTY);
    }

    /// <summary>true: --reapply-cherry-picks, false: --no-reapply-cherry-picks, null: omitted.</summary>
    public bool? ReapplyCherryPicks
    {
        get => GetToggle(ReapplyIndex, Const.Repository.RebaseArguments.REAPPLY_CHERRY_PICKS);
        init => SetToggle(ReapplyIndex, value, Const.Repository.RebaseArguments.REAPPLY_CHERRY_PICKS, Const.Repository.RebaseArguments.NO_REAPPLY_CHERRY_PICKS);
    }

    /// <summary>No-op (--allow-empty-message).</summary>
    public bool AllowEmptyMessage { get => Has(AllowEmptyMessageIndex); init => SetFlag(AllowEmptyMessageIndex, value, Const.Repository.RebaseArguments.ALLOW_EMPTY_MESSAGE); }

    /// <summary>Uses the merge backend (--merge).</summary>
    public bool Merge { get => Has(MergeIndex); init => SetFlag(MergeIndex, value, Const.Repository.RebaseArguments.MERGE); }

    /// <summary>Merge strategy (--strategy=&lt;strategy&gt;). Blank values are ignored.</summary>
    public string Strategy
    {
        get => GetSecond(StrategyIndex);
        init => SetValue(StrategyIndex, Const.Repository.RebaseArguments.STRATEGY, value);
    }

    /// <summary>true: --rerere-autoupdate, false: --no-rerere-autoupdate, null: omitted.</summary>
    public bool? RerereAutoUpdate
    {
        get => GetToggle(RerereIndex, Const.Repository.RebaseArguments.RERERE_AUTOUPDATE);
        init => SetToggle(RerereIndex, value, Const.Repository.RebaseArguments.RERERE_AUTOUPDATE, Const.Repository.RebaseArguments.NO_RERERE_AUTOUPDATE);
    }

    /// <summary>true: -S (or -S&lt;keyid&gt; when <see cref="GpgKeyId"/> is set), false: --no-gpg-sign, null: omitted unless <see cref="GpgKeyId"/> is set.</summary>
    public bool? GpgSign
    {
        get => GetToggle(GpgIndex, Const.Repository.RebaseArguments.GPG_SIGN);
        init => SetToggle(GpgIndex, value, Const.Repository.RebaseArguments.GPG_SIGN, Const.Repository.RebaseArguments.NO_GPG_SIGN);
    }

    /// <summary>--quiet. Cannot be combined with <see cref="Verbose"/>.</summary>
    public bool Quiet { get => Has(QuietIndex); init => SetFlag(QuietIndex, value, Const.Repository.RebaseArguments.QUIET); }

    /// <summary>--verbose. Cannot be combined with <see cref="Quiet"/>.</summary>
    public bool Verbose { get => Has(VerboseIndex); init => SetFlag(VerboseIndex, value, Const.Repository.RebaseArguments.VERBOSE); }

    /// <summary>true: --stat, false: --no-stat, null: omitted.</summary>
    public bool? Stat
    {
        get => GetToggle(StatIndex, Const.Repository.RebaseArguments.STAT);
        init => SetToggle(StatIndex, value, Const.Repository.RebaseArguments.STAT, Const.Repository.RebaseArguments.NO_STAT);
    }

    /// <summary>true: --verify, false: --no-verify, null: omitted.</summary>
    public bool? Verify
    {
        get => GetToggle(VerifyIndex, Const.Repository.RebaseArguments.VERIFY);
        init => SetToggle(VerifyIndex, value, Const.Repository.RebaseArguments.VERIFY, Const.Repository.RebaseArguments.NO_VERIFY);
    }

    /// <summary>Surrounding context lines that must match (-C&lt;n&gt;). Implies --apply. Null omits the option.</summary>
    public int? ContextLines
    {
        get => arguments[ContextIndex].Transform(
            static v => int.TryParse(v.Second, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? (int?)n : null,
            static () => null);
        init => arguments[ContextIndex] = value.HasValue
            ? Optional<Pair>.Some(new Pair(Const.Repository.RebaseArguments.CONTEXT, value.Value.ToString(CultureInfo.InvariantCulture)))
            : Optional<Pair>.None;
    }

    /// <summary>Replays all commits instead of fast-forwarding (--force-rebase, also --no-ff / -f).</summary>
    public bool ForceRebase { get => Has(ForceRebaseIndex); init => SetFlag(ForceRebaseIndex, value, Const.Repository.RebaseArguments.FORCE_REBASE); }

    /// <summary>true: --fork-point, false: --no-fork-point, null: omitted.</summary>
    public bool? ForkPoint
    {
        get => GetToggle(ForkPointIndex, Const.Repository.RebaseArguments.FORK_POINT);
        init => SetToggle(ForkPointIndex, value, Const.Repository.RebaseArguments.FORK_POINT, Const.Repository.RebaseArguments.NO_FORK_POINT);
    }

    /// <summary>--ignore-whitespace</summary>
    public bool IgnoreWhitespace { get => Has(IgnoreWhitespaceIndex); init => SetFlag(IgnoreWhitespaceIndex, value, Const.Repository.RebaseArguments.IGNORE_WHITESPACE); }

    /// <summary>Passed to git apply (--whitespace=&lt;option&gt;). Implies --apply. Blank values are ignored.</summary>
    public string Whitespace
    {
        get => GetSecond(WhitespaceIndex);
        init => SetValue(WhitespaceIndex, Const.Repository.RebaseArguments.WHITESPACE, value);
    }

    /// <summary>--committer-date-is-author-date</summary>
    public bool CommitterDateIsAuthorDate { get => Has(CommitterDateIndex); init => SetFlag(CommitterDateIndex, value, Const.Repository.RebaseArguments.COMMITTER_DATE_IS_AUTHOR_DATE); }

    /// <summary>Uses the current time as author date (--reset-author-date, synonym of --ignore-date).</summary>
    public bool ResetAuthorDate { get => Has(IgnoreDateIndex); init => SetFlag(IgnoreDateIndex, value, Const.Repository.RebaseArguments.RESET_AUTHOR_DATE); }

    /// <summary>Adds a Signed-off-by trailer (--signoff).</summary>
    public bool Signoff { get => Has(SignoffIndex); init => SetFlag(SignoffIndex, value, Const.Repository.RebaseArguments.SIGNOFF); }

    /// <summary>--interactive</summary>
    public bool Interactive { get => Has(InteractiveIndex); init => SetFlag(InteractiveIndex, value, Const.Repository.RebaseArguments.INTERACTIVE); }

    /// <summary>--rebase-merges[=mode] or --no-rebase-merges (<see cref="RebaseMergesMode.Disabled"/>).</summary>
    public RebaseMergesMode RebaseMerges
    {
        get => arguments[RebaseMergesIndex].Transform(
            static v => v.First switch
            {
                Const.Repository.RebaseArguments.NO_REBASE_MERGES => RebaseMergesMode.Disabled,
                _ => v.Second switch
                {
                    Const.Repository.RebaseArguments.REBASE_COUSINS => RebaseMergesMode.RebaseCousins,
                    Const.Repository.RebaseArguments.NO_REBASE_COUSINS => RebaseMergesMode.NoRebaseCousins,
                    _ => RebaseMergesMode.Default
                }
            },
            static () => RebaseMergesMode.None);
        init => arguments[RebaseMergesIndex] = value switch
        {
            RebaseMergesMode.Default => Optional<Pair>.Some(new Pair(Const.Repository.RebaseArguments.REBASE_MERGES)),
            RebaseMergesMode.RebaseCousins => Optional<Pair>.Some(new Pair(Const.Repository.RebaseArguments.REBASE_MERGES_VALUE, Const.Repository.RebaseArguments.REBASE_COUSINS)),
            RebaseMergesMode.NoRebaseCousins => Optional<Pair>.Some(new Pair(Const.Repository.RebaseArguments.REBASE_MERGES_VALUE, Const.Repository.RebaseArguments.NO_REBASE_COUSINS)),
            RebaseMergesMode.Disabled => Optional<Pair>.Some(new Pair(Const.Repository.RebaseArguments.NO_REBASE_MERGES)),
            _ => Optional<Pair>.None
        };
    }

    /// <summary>Rebases all commits reachable from the branch (--root).</summary>
    public bool Root { get => Has(RootIndex); init => SetFlag(RootIndex, value, Const.Repository.RebaseArguments.ROOT); }

    /// <summary>true: --autosquash, false: --no-autosquash, null: omitted.</summary>
    public bool? Autosquash
    {
        get => GetToggle(AutosquashIndex, Const.Repository.RebaseArguments.AUTOSQUASH);
        init => SetToggle(AutosquashIndex, value, Const.Repository.RebaseArguments.AUTOSQUASH, Const.Repository.RebaseArguments.NO_AUTOSQUASH);
    }

    /// <summary>true: --autostash, false: --no-autostash, null: omitted.</summary>
    public bool? Autostash
    {
        get => GetToggle(AutostashIndex, Const.Repository.RebaseArguments.AUTOSTASH);
        init => SetToggle(AutostashIndex, value, Const.Repository.RebaseArguments.AUTOSTASH, Const.Repository.RebaseArguments.NO_AUTOSTASH);
    }

    /// <summary>true: --reschedule-failed-exec, false: --no-reschedule-failed-exec, null: omitted.</summary>
    public bool? RescheduleFailedExec
    {
        get => GetToggle(RescheduleIndex, Const.Repository.RebaseArguments.RESCHEDULE_FAILED_EXEC);
        init => SetToggle(RescheduleIndex, value, Const.Repository.RebaseArguments.RESCHEDULE_FAILED_EXEC, Const.Repository.RebaseArguments.NO_RESCHEDULE_FAILED_EXEC);
    }

    /// <summary>true: --update-refs, false: --no-update-refs, null: omitted.</summary>
    public bool? UpdateRefs
    {
        get => GetToggle(UpdateRefsIndex, Const.Repository.RebaseArguments.UPDATE_REFS);
        init => SetToggle(UpdateRefsIndex, value, Const.Repository.RebaseArguments.UPDATE_REFS, Const.Repository.RebaseArguments.NO_UPDATE_REFS);
    }

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(Const.REBASE, true);

        var gpgKey = GpgKeyId;
        var hasGpgKey = !string.IsNullOrWhiteSpace(gpgKey);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];

            if (i == GpgIndex)
            {
                if (hasGpgKey && (!optional.HasValue || optional.Value.First == Const.Repository.RebaseArguments.GPG_SIGN))
                    argumentBuilder.AddArgument(string.Concat(Const.Repository.RebaseArguments.GPG_SIGN, gpgKey), true);
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
            else if (i == OntoIndex)
            {
                argumentBuilder.AddArgument(pair.First!, true);
                argumentBuilder.AddArgument(pair.Second!, true);
            }
            else
            {
                // --empty=<mode>, --strategy=<name>, --whitespace=<opt>, --rebase-merges=<mode>, -C<n>
                argumentBuilder.AddArgument(string.Concat(pair.First, pair.Second), true);
            }
        }

        var strategyOptions = StrategyOptions;
        if (strategyOptions is not null)
        {
            for (var i = 0; i < strategyOptions.Count; i++)
                argumentBuilder.AddArgument(string.Concat(Const.Repository.RebaseArguments.STRATEGY_OPTION, strategyOptions[i]), true);
        }

        var trailers = Trailers;
        if (trailers is not null)
        {
            for (var i = 0; i < trailers.Count; i++)
                argumentBuilder.AddArgument(string.Concat(Const.Repository.RebaseArguments.TRAILER, trailers[i]), true);
        }

        var exec = Exec;
        if (exec is not null)
        {
            for (var i = 0; i < exec.Count; i++)
            {
                argumentBuilder.AddArgument(Const.Repository.RebaseArguments.EXEC, true);
                argumentBuilder.AddArgument(exec[i], true);
            }
        }

        var upstream = Upstream;
        var branch = Branch;
        var hasUpstream = !string.IsNullOrWhiteSpace(upstream);
        var hasBranch = !string.IsNullOrWhiteSpace(branch);
        if (hasUpstream || hasBranch)
        {
            argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
            if (hasUpstream)
                argumentBuilder.AddArgument(upstream!, true);
            if (hasBranch)
                argumentBuilder.AddArgument(branch!, true);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        var upstream = Upstream;
        var branch = Branch;
        var hasUpstream = upstream is not null;
        var hasBranch = branch is not null;
        var hasStrategyOptions = HasItems(StrategyOptions);
        var hasExec = HasItems(Exec);
        var hasTrailers = HasItems(Trailers);
        var hasGpgKey = !string.IsNullOrWhiteSpace(GpgKeyId);
        var root = Has(RootIndex);
        var onto = Has(OntoIndex);
        var keepBase = Has(KeepBaseIndex);
        string? error = null;

        if (hasUpstream && string.IsNullOrWhiteSpace(upstream)
            || hasBranch && string.IsNullOrWhiteSpace(branch)
            || HasBlank(StrategyOptions) || HasBlank(Exec) || HasBlank(Trailers))
        {
            error = Const.Repository.RebaseErrors.VALUE_INVALID;
        }
        else if (Has(ModeIndex))
        {
            var other = hasUpstream || hasBranch || hasStrategyOptions || hasExec || hasTrailers || hasGpgKey;
            for (var i = 1; i < arguments.Length && !other; i++)
                other = arguments[i].HasValue;

            if (other)
                error = Const.Repository.RebaseErrors.MODE_EXCLUSIVE;
        }
        else if (Has(QuietIndex) && Has(VerboseIndex))
            error = Const.Repository.RebaseErrors.QUIET_VERBOSE;
        else if (hasGpgKey && GpgSign is false)
            error = Const.Repository.RebaseErrors.GPG_KEY_WITHOUT_SIGN;
        else if (keepBase && onto)
            error = Const.Repository.RebaseErrors.KEEP_BASE_ONTO;
        else if (keepBase && root)
            error = Const.Repository.RebaseErrors.KEEP_BASE_ROOT;
        else if (ForkPoint is true && root)
            error = Const.Repository.RebaseErrors.FORK_POINT_ROOT;
        else if (root && hasUpstream)
            error = Const.Repository.RebaseErrors.ROOT_UPSTREAM;
        else if (hasBranch && !hasUpstream && !root)
            error = Const.Repository.RebaseErrors.BRANCH_WITHOUT_UPSTREAM;
        else if (Has(ContextIndex) && ContextLines is not >= 0)
            error = Const.Repository.RebaseErrors.CONTEXT_INVALID;
        else if (Has(ApplyIndex) || Has(WhitespaceIndex) || Has(ContextIndex))
        {
            var merges = RebaseMerges;
            if (Has(MergeIndex) || Has(StrategyIndex) || hasStrategyOptions
                || Autosquash is true
                || merges is RebaseMergesMode.Default or RebaseMergesMode.RebaseCousins or RebaseMergesMode.NoRebaseCousins
                || Has(InteractiveIndex)
                || hasExec
                || KeepEmpty is false
                || Has(EmptyIndex)
                || Has(ReapplyIndex) && !keepBase
                || UpdateRefs is true
                || root && !onto
                || hasTrailers)
            {
                error = Const.Repository.RebaseErrors.APPLY_INCOMPATIBLE;
            }
        }

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_REBASE, error));

        return Result<Nothing, IError>.Some(new());
    }

    private static bool HasItems(IReadOnlyList<string>? list) => list is not null && list.Count > 0;

    private static bool HasBlank(IReadOnlyList<string>? list)
    {
        if (list is null)
            return false;

        for (var i = 0; i < list.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(list[i]))
                return true;
        }

        return false;
    }

    private bool Has(int index) => arguments[index].HasValue;

    private void SetFlag(int index, bool value, string token)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(token))
            : Optional<Pair>.None;

    private void SetValue(int index, string token, string value)
        => arguments[index] = !string.IsNullOrWhiteSpace(value)
            ? Optional<Pair>.Some(new Pair(token, value))
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