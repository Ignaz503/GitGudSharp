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

using FsckConst = GitGud.Constants.Const.Repository.FsCheck;

namespace GitGud.Repositories.Administration;

/// <summary>Description of a <c>git fsck</c> call. Only the options that are explicitly set end up as arguments.</summary>
public sealed class FsCheckOptions : IOperationOptions
{
    const int TagsIndex = 0;
    const int RootIndex = 1;
    const int UnreachableIndex = 2;
    const int CacheIndex = 3;
    const int NoReflogsIndex = 4;
    const int FullIndex = 5;
    const int StrictIndex = 6;
    const int LostFoundIndex = 7;
    const int DanglingIndex = 8;
    const int ConnectivityOnlyIndex = 9;
    const int NameObjectsIndex = 10;
    const int ReferencesIndex = 11;

    private readonly Optional<Pair>[] arguments = new Optional<Pair>[12];

    /// <summary>Reports tags (--tags).</summary>
    public bool Tags { get => Has(TagsIndex); init => SetFlag(TagsIndex, FsckConst.TAGS, value); }

    /// <summary>Reports root nodes (--root).</summary>
    public bool Root { get => Has(RootIndex); init => SetFlag(RootIndex, FsckConst.ROOT, value); }

    /// <summary>Prints objects that are not reachable from any reference (--unreachable).</summary>
    public bool Unreachable { get => Has(UnreachableIndex); init => SetFlag(UnreachableIndex, FsckConst.UNREACHABLE, value); }

    /// <summary>Considers objects recorded in the index as heads (--cache).</summary>
    public bool Cache { get => Has(CacheIndex); init => SetFlag(CacheIndex, FsckConst.CACHE, value); }

    /// <summary>Does not treat commits only referenced by reflogs as reachable (--no-reflogs).</summary>
    public bool NoReflogs { get => Has(NoReflogsIndex); init => SetFlag(NoReflogsIndex, FsckConst.NO_REFLOGS, value); }

    /// <summary>When true also checks alternate object pools and packs (--full), when false only the object directory (--no-full); null uses the git default.</summary>
    public bool? Full
    {
        get => GetTriState(FullIndex, FsckConst.FULL);
        init => SetTriState(FullIndex, value, FsckConst.FULL, FsckConst.NO_FULL);
    }

    /// <summary>Enables stricter checking (--strict).</summary>
    public bool Strict { get => Has(StrictIndex); init => SetFlag(StrictIndex, FsckConst.STRICT, value); }

    /// <summary>Writes dangling objects into .git/lost-found (--lost-found).</summary>
    public bool LostFound { get => Has(LostFoundIndex); init => SetFlag(LostFoundIndex, FsckConst.LOST_FOUND, value); }

    /// <summary>When true prints dangling objects (--dangling), when false omits them (--no-dangling); null uses the git default.</summary>
    public bool? Dangling
    {
        get => GetTriState(DanglingIndex, FsckConst.DANGLING);
        init => SetTriState(DanglingIndex, value, FsckConst.DANGLING, FsckConst.NO_DANGLING);
    }

    /// <summary>Only checks connectivity of reachable objects (--connectivity-only).</summary>
    public bool ConnectivityOnly { get => Has(ConnectivityOnlyIndex); init => SetFlag(ConnectivityOnlyIndex, FsckConst.CONNECTIVITY_ONLY, value); }

    /// <summary>When true displays how objects are reachable (--name-objects), when false does not (--no-name-objects); null uses the git default.</summary>
    public bool? NameObjects
    {
        get => GetTriState(NameObjectsIndex, FsckConst.NAME_OBJECTS);
        init => SetTriState(NameObjectsIndex, value, FsckConst.NAME_OBJECTS, FsckConst.NO_NAME_OBJECTS);
    }

    /// <summary>When true checks the references database (--references), when false skips it (--no-references); null uses the git default.</summary>
    public bool? References
    {
        get => GetTriState(ReferencesIndex, FsckConst.REFERENCES);
        init => SetTriState(ReferencesIndex, value, FsckConst.REFERENCES, FsckConst.NO_REFERENCES);
    }

    /// <summary>Objects treated as heads of an unreachability trace.</summary>
    public IReadOnlyList<string>? Objects { get; init; }

    private bool Has(int index)
        => arguments[index].HasValue;

    private void SetFlag(int index, string name, bool value)
        => arguments[index] = value
            ? Optional<Pair>.Some(new Pair(name))
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
        argumentBuilder.AddArgument(Const.FSCK, true);
        argumentBuilder.Flag(FsckConst.NO_PROGRESS);

        for (var i = 0; i < arguments.Length; i++)
        {
            var optional = arguments[i];
            if (optional.HasValue)
                argumentBuilder.Flag(optional.Value.First!);
        }

        var objects = Objects;
        if (objects is null || objects.Count == 0)
            return;

        argumentBuilder.AddArgument(Const.Repository.END_OF_OPTIONS, true);
        for (var i = 0; i < objects.Count; i++)
            argumentBuilder.AddArgument(objects[i], true);
    }

    public Result<Nothing, IError> Validate()
    {
        var objects = Objects;
        if (objects is not null)
        {
            for (var i = 0; i < objects.Count; i++)
            {
                var value = objects[i];
                if (string.IsNullOrWhiteSpace(value) || value[0] == Const.Repository.Branching.OPTION_PREFIX)
                    return Result<Nothing, IError>.None(
                        new RepositoryValidationError(Const.Repository.GIT_FSCK, FsckConst.Errors.OBJECT_INVALID));
            }
        }

        return Result<Nothing, IError>.Some(new());
    }
}