// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud;

public struct Section : IEquatable<Section>, IEquatable<string>
{
    string _value;
    public Section(string value)
        => _value = value;
    public override string ToString()
     => _value;
    public override int GetHashCode()
     => _value.GetHashCode();
    public override bool Equals(object obj)
        => (obj is Section section && Equals(section)) || (obj is string val && Equals(val));
    public bool Equals(Section other)
        => _value == other._value;
    public bool Equals(string other)
        => _value == other;
    public static implicit operator string(Section section) => section._value;
    public static explicit operator Section(string value) => new Section(value);
    public static bool operator ==(Section left, Section right) => left.Equals(right);
    public static bool operator !=(Section left, Section right) => !(left == right);
}
