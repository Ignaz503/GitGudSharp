// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap;

using GitGud.Constants;

using System.Globalization;


namespace GitGud.CliArguments;

public struct DoubleDashedArgument : IEquatable<DoubleDashedArgument>, IEquatable<string>
{
    private string _value;

    public DoubleDashedArgument() => _value = string.Empty;

    public DoubleDashedArgument(string value)
        => _value = value;

    public override bool Equals(object obj)
        => (obj is DoubleDashedArgument arg && Equals(arg)) || ((obj is string str) && Equals(str));

    public override int GetHashCode()
     => _value.GetHashCode();

    public bool Equals(DoubleDashedArgument other)
        => other._value == _value;

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, Const.FORMAT_DOUBLE_DASHED_ARGUMENT, _value);

    public bool Equals(string other)
        => _value == other;

    public static implicit operator string(DoubleDashedArgument dv)
        => dv.ToString();

    public static implicit operator DoubleDashedArgument(string value)
        => new(value);

    public static bool operator ==(DoubleDashedArgument left, DoubleDashedArgument right) => left.Equals(right);

    public static bool operator !=(DoubleDashedArgument left, DoubleDashedArgument right) => !(left == right);

}

public struct DoubleDashedArgument<T> : IEquatable<DoubleDashedArgument<T>>
{
    T _value;
    string _name;

    bool equals = true;

    public DoubleDashedArgument()
        => (_name, _value) = (string.Empty, default!);

    public DoubleDashedArgument(string name, T value)
        => (_name, _value) = (name, value);

    public DoubleDashedArgument(string name, T value, bool useEquals)
        => (_name, _value, equals) = (name, value, useEquals);

    public override bool Equals(object obj)
     => obj is DoubleDashedArgument<T> arg && Equals(arg);


    public bool Equals(DoubleDashedArgument<T> other)
        => _name == other._name && EqualityComparer<T>.Default.Equals(_value, other._value);

    public override int GetHashCode()
    {
        int hashCode = 707945396;
        hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_value);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_name);
        return hashCode;
    }

    public override string ToString()
     => string.Format(CultureInfo.InvariantCulture, equals ? Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE : Const.FORMAT_DOUBLE_DASHED_ARGUMENT_WITH_VALUE_WHITE_SPACE, _name, _value);

    public static implicit operator string(DoubleDashedArgument<T> dv)
        => dv.ToString();

    public static bool operator ==(DoubleDashedArgument<T> left, DoubleDashedArgument<T> right) => left.Equals(right);

    public static bool operator !=(DoubleDashedArgument<T> left, DoubleDashedArgument<T> right) => !(left == right);
}

