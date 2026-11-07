// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Setup.Errors;

public sealed class SetupProcessError : SetupError
{
    public SetupProcessError(string operation, string message, int exitCode, string? standardError = null)
        : base(operation, message, exitCode, standardError)
    {
    }
}



