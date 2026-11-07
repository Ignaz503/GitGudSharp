// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Utility;

public struct DirectoryPath : IEquatable<DirectoryPath>, IEquatable<string>
{
    string _value;

    public DirectoryPath() => _value = string.Empty;

    public DirectoryPath(string value)
        => _value = value;

    public override string ToString()
     => _value;

    public override int GetHashCode()
     => _value.GetHashCode();

    public override bool Equals(object obj)
        => (obj is DirectoryPath path && Equals(path)) || (obj is string val && Equals(val));

    public bool Equals(DirectoryPath other)
        => _value == other._value;

    public bool Equals(string other)
        => _value == other;

    public static implicit operator string(DirectoryPath path) => path._value;

    public static explicit operator DirectoryPath(string value) => new DirectoryPath(value);

    public static bool operator ==(DirectoryPath left, DirectoryPath right) => left.Equals(right);

    public static bool operator !=(DirectoryPath left, DirectoryPath right) => !(left == right);
}



