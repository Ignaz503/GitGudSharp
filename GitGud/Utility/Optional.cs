// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Utility;

/// <summary>
/// A value that may or may not be present. Implicitly created from a value; a <see langword="null"/> value is treated as absent.
/// </summary>
public readonly struct Optional<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private Optional(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public bool HasValue => _hasValue;

    /// <summary>
    /// Only call if <see cref="HasValue"/> is <see langword="true"/>.
    /// </summary>
    public T Value => _value;

    public static Optional<T> Some(T value)
        => new(value);

    public static Optional<T> None
        => default;

    public static implicit operator Optional<T>(T value)
        => value is null ? default : new Optional<T>(value);

    public Optional<TResult> Map<TResult>(Func<T, TResult> map)
        => _hasValue ? Optional<TResult>.Some(map(_value)) : default;

    public TResult Transform<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
        => _hasValue ? onSome(_value) : onNone();
}
