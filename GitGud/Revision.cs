// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud;

/// <summary>A git revision expression: branch, tag, commit id, or a range of them.</summary>
public readonly struct Revision : IEquatable<Revision>, IEquatable<string>
{
    private readonly string? _value;

    public Revision(string value)
        => _value = value;

    /// <summary>Commits reachable from <paramref name="to"/> but not from <paramref name="from"/> (from..to).</summary>
    public static Revision Range(Revision from, Revision to)
        => new(string.Format(Const.Repository.Logging.RANGE_FORMAT, from, to));

    /// <summary>Commits reachable from either side but not both (a...b).</summary>
    public static Revision SymmetricDifference(Revision left, Revision right)
        => new(string.Format(Const.Repository.Logging.SYMMETRIC_RANGE_FORMAT, left, right));

    /// <summary>Excludes everything reachable from <paramref name="revision"/> (^rev).</summary>
    public static Revision Exclude(Revision revision)
        => new(string.Format(Const.Repository.Logging.EXCLUDE_FORMAT, revision));

    public override string ToString()
        => _value ?? string.Empty;

    public override int GetHashCode()
        => ToString().GetHashCode();

    public override bool Equals(object? obj)
        => (obj is Revision revision && Equals(revision)) || (obj is string val && Equals(val));

    public bool Equals(Revision other)
        => ToString() == other.ToString();

    public bool Equals(string? other)
        => ToString() == other;

    public static implicit operator string(Revision revision) => revision.ToString();

    public static implicit operator Revision(Branch branch) => new(branch.ToString());

    public static implicit operator Revision(Tag tag) => new(tag.ToString());

    public static explicit operator Revision(string value) => new(value);

    public static bool operator ==(Revision left, Revision right) => left.Equals(right);

    public static bool operator !=(Revision left, Revision right) => !(left == right);
}
