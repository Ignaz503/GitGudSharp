// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Utility;

public struct RegexPattern : IEquatable<RegexPattern>, IEquatable<string>
{
    string _value;
    public RegexPattern() => _value = string.Empty;
    public RegexPattern(string value)
        => _value = value;
    public override string ToString()
     => _value;
    public override int GetHashCode()
     => _value.GetHashCode();
    public override bool Equals(object obj)
        => (obj is RegexPattern pattern && Equals(pattern)) || (obj is string val && Equals(val));
    public bool Equals(RegexPattern other)
        => _value == other._value;
    public bool Equals(string other)
        => _value == other;
    public static implicit operator string(RegexPattern pattern) => pattern._value;
    public static explicit operator RegexPattern(string value) => new RegexPattern(value);
    public static bool operator ==(RegexPattern left, RegexPattern right) => left.Equals(right);
    public static bool operator !=(RegexPattern left, RegexPattern right) => !(left == right);
}
