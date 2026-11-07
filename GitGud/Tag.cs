// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud;

public struct Tag : IEquatable<Tag>, IEquatable<string>
{
    string _value;

    public Tag() => _value = string.Empty;

    public Tag(string value)
        => _value = value;

    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>Checks the name against the main git-check-ref-format rules.</summary>
    public bool IsValid
    {
        get
        {
            var value = _value;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (value[0] == '-' || value[0] == '/' || value[0] == '.' || value[value.Length - 1] == '/' || value[value.Length - 1] == '.' || value == "@")
                return false;
            if (value.EndsWith(".lock", StringComparison.Ordinal) || value.Contains("..") || value.Contains("//") || value.Contains("@{"))
                return false;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c <= ' ' || c == 127 || c == '~' || c == '^' || c == ':' || c == '?' || c == '*' || c == '[' || c == '\\')
                    return false;
            }

            return true;
        }
    }

    public override string ToString()
     => _value ?? string.Empty;

    public override int GetHashCode()
     => (_value ?? string.Empty).GetHashCode();

    public override bool Equals(object? obj)
        => (obj is Tag tag && Equals(tag)) || (obj is string val && Equals(val));

    public bool Equals(Tag other)
        => (_value ?? string.Empty) == (other._value ?? string.Empty);

    public bool Equals(string? other)
        => (_value ?? string.Empty) == other;

    public static implicit operator string(Tag tag) => tag._value ?? string.Empty;

    public static explicit operator Tag(string value) => new(value);

    public static bool operator ==(Tag left, Tag right) => left.Equals(right);

    public static bool operator !=(Tag left, Tag right) => !(left == right);
}
