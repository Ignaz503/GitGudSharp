// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Utility;

/// <summary>A shell style glob matched against ref names (e.g. origin, feature/*, */master).</summary>
public readonly struct GlobPattern : IEquatable<GlobPattern>, IEquatable<string>
{
    private readonly string? _value;

    public GlobPattern(string value)
        => _value = value;

    public override string ToString()
        => _value ?? string.Empty;

    public override int GetHashCode()
        => ToString().GetHashCode();

    public override bool Equals(object? obj)
        => (obj is GlobPattern pattern && Equals(pattern)) || (obj is string val && Equals(val));

    public bool Equals(GlobPattern other)
        => ToString() == other.ToString();

    public bool Equals(string? other)
        => ToString() == other;

    public static implicit operator string(GlobPattern pattern) => pattern.ToString();

    public static explicit operator GlobPattern(string value) => new(value);

    public static bool operator ==(GlobPattern left, GlobPattern right) => left.Equals(right);

    public static bool operator !=(GlobPattern left, GlobPattern right) => !(left == right);
}
