// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud;

public struct Remote : IEquatable<Remote>, IEquatable<string>
{
    string _value;

    public Remote() => _value = string.Empty;

    public Remote(string value)
        => _value = value;

    public override string ToString()
     => _value;

    public override int GetHashCode()
     => _value.GetHashCode();

    public override bool Equals(object obj)
        => (obj is Remote path && Equals(path)) || (obj is string val && Equals(val));

    public bool Equals(Remote other)
        => _value == other._value;

    public bool Equals(string other)
        => _value == other;

    public static implicit operator string(Remote path) => path._value;

    public static explicit operator Remote(string value) => new(value);

    public static bool operator ==(Remote left, Remote right) => left.Equals(right);

    public static bool operator !=(Remote left, Remote right) => !(left == right);
}



