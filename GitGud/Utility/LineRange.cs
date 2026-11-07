// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Utility;

/// <summary>A line range (e.g. 10,20 or +5) or function pattern (e.g. /int main/,/^}/) for line history.</summary>
public readonly struct LineRange : IEquatable<LineRange>, IEquatable<string>
{
    private readonly string? _value;

    public LineRange(string value)
        => _value = value;

    public override string ToString()
        => _value ?? string.Empty;

    public override int GetHashCode()
        => ToString().GetHashCode();

    public override bool Equals(object? obj)
        => (obj is LineRange range && Equals(range)) || (obj is string val && Equals(val));

    public bool Equals(LineRange other)
        => ToString() == other.ToString();

    public bool Equals(string? other)
        => ToString() == other;

    public static implicit operator string(LineRange range) => range.ToString();

    public static explicit operator LineRange(string value) => new(value);

    public static bool operator ==(LineRange left, LineRange right) => left.Equals(right);

    public static bool operator !=(LineRange left, LineRange right) => !(left == right);
}
