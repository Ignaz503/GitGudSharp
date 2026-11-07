// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud.Config.Errors;

public sealed class ConfigValidationError : ConfigError
{
    public ConfigValidationError(ConfigValidationCode code)
        : base(code.GetMessage())
        => Code = code;

    public ConfigValidationCode Code { get; }
}
