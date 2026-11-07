// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Init.Errors;

public sealed class InitValidationError : InitError
{
    public InitValidationError(InitValidationCode code)
        : base(code.GetMessage())
    {
        Code = code;
    }

    public InitValidationError(string message, int? exitCode = null, string? standardError = null) : base(message, exitCode, standardError)
    {
    }

    public InitValidationCode Code { get; }

}



