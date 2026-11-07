// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

namespace GitGud.Utility;

#nullable enable

public readonly struct Result<TSome,TNone>
{
    private const string ACHIEVEMENT = "🏆Achievment unlocked: How did we get here?🏆";

    private enum ValueType 
    {
        Invalid,
        None,
        Some,
    }

    private readonly ValueType _type;

    private readonly TNone _none;
    private readonly TSome _some;

    public Result()
        => throw new InvalidOperationException("Only constructiable via factory methods");

    private Result(TSome some, ValueType _)
    {
        _type = ValueType.Some;
        _none = default!;
        _some = some;
    }

    private Result(TNone none, ValueType _)
    {
        _type = ValueType.None;
        _some = default!;
        _none = none;
    }

    public bool HasSome => _type is ValueType.Some;
    public bool HasNone => _type is ValueType.None;

    /// <summary>
    /// Only call if <see cref="HasSome"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// By design unsafe. Know what you are doing.
    /// </remarks>
    /// <returns>The value.</returns>
    public TSome Unwrap()
        => _some!;//this is always unsafe

    /// <summary>
    /// Only call if <see cref="HasNone"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// By design unsafe. Know what you are doing.
    /// </remarks>
    /// <returns>The value.</returns>
    public TNone Handle()
        => _none!;


    public void Match(Action<TSome> onSome, Action<TNone> onNone)
    {  
        switch(_type) 
        {
            case ValueType.Some: onSome(_some); break;
            case ValueType.None: onNone(_none); break;
            default: throw new InvalidOperationException(ACHIEVEMENT);
        }
    }

    public void Match<TCtx>(Action<TCtx, TSome> onSome, Action<TCtx,TNone> onNone, TCtx ctx) 
    {
        switch (_type)
        {
            case ValueType.Some: onSome(ctx, _some); break;
            case ValueType.None: onNone(ctx, _none); break;
            default: throw new InvalidOperationException(ACHIEVEMENT);
        }
    }

    public TResult Transform<TResult>(Func<TSome, TResult> onSome, Func<TNone, TResult> onNone) 
        => _type switch {
                ValueType.Some => onSome(_some),
                ValueType.None => onNone(_none),
                _ => throw new InvalidOperationException(ACHIEVEMENT)
            };

    public TResult Transform<TCtxt, TResult>(Func<TCtxt, TSome, TResult> onSome, Func<TCtxt, TNone, TResult> onNone, TCtxt context) 
        => _type switch {
                ValueType.Some => onSome(context, _some),
                ValueType.None => onNone(context, _none),
                _ => throw new InvalidOperationException(ACHIEVEMENT)
            };

    public static Result<TSome, TNone> Some(TSome some)
        => new(some, ValueType.Some);

    public static Result<TSome, TNone> None(TNone none)
        => new(none, ValueType.None);

}
