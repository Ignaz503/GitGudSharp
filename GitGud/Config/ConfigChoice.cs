// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;

using GitGud.Config.Errors;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Config;

public readonly struct ConfigChoice
{
    private ConfigChoice(string value)
        => Value = value;

    public string Value { get; }

    public static Result<ConfigChoice, IError> Create(string? value, IReadOnlyCollection<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value) || allowedValues is null)
            return Result<ConfigChoice, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidOptions));

        foreach (var allowedValue in allowedValues)
        {
            if (string.Equals(value, allowedValue, StringComparison.OrdinalIgnoreCase))
                return Result<ConfigChoice, IError>.Some(new ConfigChoice(allowedValue));
        }

        return Result<ConfigChoice, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidOptions));
    }
}
