// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Config.Errors;

public sealed class ConfigProcessError : ConfigError
{
    public ConfigProcessError(string message, int exitCode, string? standardError = null)
        : base(message, exitCode, standardError)
    {
    }
}
