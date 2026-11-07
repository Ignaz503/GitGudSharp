// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;
using GitGud.Erorrs;

namespace GitGud.Config.Errors;

public abstract class ConfigError : BaseError
{
    protected ConfigError(string message, int? exitCode = null, string? standardError = null)
        : base(Const.Config.GIT_CONFIG, message, exitCode, standardError)
    {
    }
}
