// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

using CliWrap;

using GitGud.Constants;


namespace GitGud.CliArguments;

public struct DashedArgument : IEquatable<DashedArgument>
{
    string _name;

    public DashedArgument()
        => _name = string.Empty;


    public DashedArgument(string name)
        => _name = name;

    public override bool Equals(object obj)
     => obj is DashedArgument arg && Equals(arg);


    public bool Equals(DashedArgument other)
        => _name == other._name;
    public override int GetHashCode()
        => _name.GetHashCode();

    public override string ToString()
     => string.Format(CultureInfo.InvariantCulture, Const.FORMAT_DASHED_ARGUMENT, _name);

    public static implicit operator string(DashedArgument dv)
        => dv.ToString();

    public static bool operator ==(DashedArgument left, DashedArgument right) => left.Equals(right);

    public static bool operator !=(DashedArgument left, DashedArgument right) => !(left == right);
}

public struct DashedArgument<T> : IEquatable<DashedArgument<T>>
{
    T _value;
    string _name;

    public DashedArgument()
        => (_name, _value) = (string.Empty, default!);


    public DashedArgument(string name, T value)
        => (_name,_value) = (name, value);

    public override bool Equals(object obj)
     => obj is DashedArgument<T> arg && Equals(arg);


    public bool Equals(DashedArgument<T> other)
        => _name == other._name && EqualityComparer<T>.Default.Equals(_value, other._value);

    public override int GetHashCode()
    {
        int hashCode = 707945396;
        hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_value);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_name);
        return hashCode;
    }

    public override string ToString()
     => string.Format(CultureInfo.InvariantCulture, Const.FORMAT_DASHED_ARGUMENT_WITH_VALUE, _name, _value);

    public static implicit operator string(DashedArgument<T> dv)
        => dv.ToString();

    public static bool operator ==(DashedArgument<T> left, DashedArgument<T> right) => left.Equals(right);

    public static bool operator !=(DashedArgument<T> left, DashedArgument<T> right) => !(left == right);
}




