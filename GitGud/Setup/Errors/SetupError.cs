// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Erorrs;

namespace GitGud.Setup.Errors;

public abstract class SetupError : BaseError
{
    protected SetupError(string operation, string message, int? exitCode = null, string? standardError = null)
        : base(operation, message, exitCode, standardError)
    {
    }
}



