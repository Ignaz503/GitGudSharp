// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Utility;

public struct FilePath : IEquatable<FilePath>, IEquatable<string>
{
    string _value;

    public FilePath() => _value = string.Empty;

    public FilePath(string value)
        => _value = value;

    public override string ToString()
     => _value;

    public override int GetHashCode()
     => _value.GetHashCode();

    public override bool Equals(object obj)
        => (obj is FilePath path && Equals(path)) || (obj is string val && Equals(val));

    public bool Equals(FilePath other)
        => _value == other._value;

    public bool Equals(string other)
        => _value == other;

    public static implicit operator string(FilePath path) => path._value;

    public static explicit operator FilePath(string value) => new FilePath(value);

    public static bool operator ==(FilePath left, FilePath right) => left.Equals(right);

    public static bool operator !=(FilePath left, FilePath right) => !(left == right);
}
