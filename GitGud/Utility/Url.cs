// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Utility;

public struct Url : IEquatable<Url>, IEquatable<string>
{
    string _value;

    public Url() => _value = string.Empty;

    public Url(string value)
        => _value = value;

    public override string ToString()
     => _value;

    public override int GetHashCode()
     => _value.GetHashCode();

    public override bool Equals(object obj)
        => (obj is Url path && Equals(path)) || (obj is string val && Equals(val));

    public bool Equals(Url other)
        => _value == other._value;

    public bool Equals(string other)
        => _value == other;

    public static implicit operator string(Url path) => path._value;

    public static explicit operator Url(string value) => new(value);

    public static bool operator ==(Url left, Url right) => left.Equals(right);

    public static bool operator !=(Url left, Url right) => !(left == right);
}



