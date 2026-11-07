// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Init.Errors;

public sealed class InitProcessError : InitError
{
    public InitProcessError(string message, int exitCode, string? standardError = null)
        : base(message, exitCode, standardError)
    {
    }
}



