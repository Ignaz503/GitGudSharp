// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud;

public struct Branch : IEquatable<Branch>, IEquatable<string>
{
    string _value;

    public Branch() => _value = string.Empty;

    public Branch(string value)
        => _value = value;

    public override string ToString()
     => _value;

    public override int GetHashCode()
     => _value.GetHashCode();

    public override bool Equals(object obj)
        => (obj is Branch path && Equals(path)) || (obj is string val && Equals(val));

    public bool Equals(Branch other)
        => _value == other._value;

    public bool Equals(string other)
        => _value == other;

    public static implicit operator string(Branch path) => path._value;

    public static explicit operator Branch(string value) => new(value);

    public static bool operator ==(Branch left, Branch right) => left.Equals(right);

    public static bool operator !=(Branch left, Branch right) => !(left == right);
}



