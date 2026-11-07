// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Globalization;

using GitGud.Config.Errors;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Config;

public readonly record struct ConfigValue(string Value, ConfigValueType Type)
{
    public static ConfigValue FromString(string value)
        => new(value, ConfigValueType.String);

    public static ConfigValue FromTypedString(string value, ConfigValueType type)
        => new(value, type);

    public static Result<ConfigValue, IError> Create(string? value, ConfigValueType type)
        => value is null
            ? Result<ConfigValue, IError>.None(new ConfigValidationError(ConfigValidationCode.ValueRequired))
            : Result<ConfigValue, IError>.Some(new ConfigValue(value, type));

    public static ConfigValue FromBoolean(bool value)
        => new(value ? "true" : "false", ConfigValueType.Boolean);

    public static ConfigValue FromInteger(long value)
        => new(value.ToString(CultureInfo.InvariantCulture), ConfigValueType.Integer);
}
