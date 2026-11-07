// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Erorrs;

public class BaseError : IError
{
    public BaseError(string operation, string message, int? exitCode = null, string? standardError = null)
    {
        Operation = operation;
        Message = message;
        ExitCode = exitCode;
        StandardError = standardError;
    }

    public string Operation { get; }
    public string Message { get; }
    public int? ExitCode { get; }
    public string? StandardError { get; }
}



