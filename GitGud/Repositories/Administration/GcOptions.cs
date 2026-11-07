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

using System.Globalization;

using GcConst = GitGud.Constants.Const.Repository.Gc;

namespace GitGud.Repositories.Administration;

/// <summary>Description of a <c>git gc</c> call. Only the options that are explicitly set end up as arguments.</summary>
public sealed class GcOptions : IOperationOptions
{
    const int AggressiveIndex = 0;
    const int AutoIndex = 1;
    const int DetachIndex = 2;
    const int CruftIndex = 3;
    const int MaxCruftSizeIndex = 4;
    const int ExpireToIndex = 5;
    const int PruneIndex = 6;
    const int PruneNowIndex = 7;
    const int NoPruneIndex = 8;
    const int QuietIndex = 9;
    const int ForceIndex = 10;
    const int KeepLargestPackIndex = 11;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[12];
    private DateTimeOffset? _prune;

    /// <summary>Optimizes more aggressively at the expense of time (--aggressive).</summary>
    public bool Aggressive { get => Has(AggressiveIndex); init => SetFlag(AggressiveIndex, GcConst.AGGRESSIVE, value); }

    /// <summary>Only runs when housekeeping is required (--auto).</summary>
    public bool Auto { get => Has(AutoIndex); init => SetFlag(AutoIndex, GcConst.AUTO, value); }

    /// <summary>When true runs in the background (--detach), when false in the foreground (--no-detach); null uses the git default.</summary>
    public bool? Detach
    {
        get => GetTriState(DetachIndex, GcConst.DETACH);
        init => SetTriState(DetachIndex, value, GcConst.DETACH, GcConst.NO_DETACH);
    }

    /// <summary>When true packs unreachable objects into a cruft pack (--cruft), when false stores them loose (--no-cruft); null uses the git default.</summary>
    public bool? Cruft
    {
        get => GetTriState(CruftIndex, GcConst.CRUFT);
        init => SetTriState(CruftIndex, value, GcConst.CRUFT, GcConst.NO_CRUFT);
    }

    /// <summary>Limits the size of new cruft packs in bytes (--max-cruft-size).</summary>
    public long? MaxCruftSize
    {
        get => GetValue(MaxCruftSizeIndex) is string text
               && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes)
            ? bytes
            : default(long?);
        init => SetValue(MaxCruftSizeIndex, GcConst.MAX_CRUFT_SIZE, value?.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Writes pruned objects to a cruft pack in this directory (--expire-to). Cannot be combined with Cruft = false.</summary>
    public DirectoryPath? ExpireTo
    {
        get => GetValue(ExpireToIndex) is string text ? new DirectoryPath(text) : default(DirectoryPath?);
        init => SetValue(ExpireToIndex, GcConst.EXPIRE_TO, value is DirectoryPath directory ? directory.ToString() : null);
    }

    /// <summary>Prunes loose objects older than the date (--prune).</summary>
    public DateTimeOffset? Prune
    {
        get => _prune;
        init
        {
            _prune = value;
            SetValue(PruneIndex, GcConst.PRUNE, value?.ToString(GcConst.DATE_FORMAT, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>Prunes loose objects regardless of their age (--prune=now).</summary>
    public bool PruneNow
    {
        get => Has(PruneNowIndex);
        init => arguments[PruneNowIndex] = value
            ? Optional<Pair>.Some(new Pair(GcConst.PRUNE, GcConst.PRUNE_NOW))
            : Optional<Pair>.None;
    }

    /// <summary>Does not prune any loose objects (--no-prune).</summary>
    public bool NoPrune { get => Has(NoPruneIndex); init => SetFlag(NoPruneIndex, GcConst.NO_PRUNE, value); }

    /// <summary>Suppresses all progress reports (--quiet).</summary>
    public bool Quiet { get => Has(QuietIndex); init => SetFlag(QuietIndex, GcConst.QUIET, value); }

    /// <summary>Runs even if another gc may be running (--force).</summary>
    public bool Force { get => Has(ForceIndex); init => SetFlag(ForceIndex, GcConst.FORCE, value); }

    /// <summary>Consolidates all packs except the largest one (--keep-largest-pack).</summary>
    public bool KeepLargestPack { get => Has(KeepLargestPackIndex); init => SetFlag(KeepLargestPackIndex, GcConst.KEEP_LARGEST_PACK, value); }

    private bool Has(int index)
        => arguments[index].HasValue;

    private string? GetValue(int index)
        => arguments[index].Transform(static v => v.Second, static () => default!);

    private void SetFlag(int index, string name, bool value)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(name))
            : Optional<Pair>.None;

    private void SetValue(int index, string name, string? value)
        => arguments[index] = value is string text
            ? Optional<Pair>.Some(new Pair(name, text))
            : Optional<Pair>.None;

    private void SetTriState(int index, bool? value, string onName, string offName)
        => arguments[index] = value is bool enabled
            ? Optional<Pair>.Some(new Pair(enabled ? onName : offName))
            : Optional<Pair>.None;

    private bool? GetTriState(int index, string onName)
        => arguments[index].Transform(
            v => string.Equals(v.First, onName, StringComparison.Ordinal),
            static () => default);

    public void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder)
    {
        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (!optional.HasValue)
                continue;

            var pair = optional.Value;
            if (pair.TwoValues)
                argumentBuilder.AddArgument(
                    string.Format(Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE, pair.First, pair.Second), true);
            else
                argumentBuilder.Flag(pair.First!);
        }
    }

    public Result<Nothing, IError> Validate()
    {
        string? error = null;

        if (MaxCruftSize is long bytes && bytes < 0)
            error = GcConst.Errors.NUMBER_INVALID;
        else if (GetValue(ExpireToIndex) is string directory && string.IsNullOrWhiteSpace(directory))
            error = GcConst.Errors.VALUE_INVALID;
        else if (NoPrune && (Has(PruneIndex) || Has(PruneNowIndex)))
            error = GcConst.Errors.PRUNE_CONFLICT;
        else if (Has(PruneIndex) && Has(PruneNowIndex))
            error = GcConst.Errors.PRUNE_CONFLICT;
        else if (Has(ExpireToIndex) && Cruft == false)
            error = GcConst.Errors.EXPIRE_TO_REQUIRES_CRUFT;

        if (error is not null)
            return Result<Nothing, IError>.None(new RepositoryValidationError(Const.Repository.GIT_GC, error));

        return Result<Nothing, IError>.Some(new());
    }
}