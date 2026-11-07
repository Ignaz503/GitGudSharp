// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Errors;

public sealed class RepositoryIndexLockedError : RepositoryError
{
    public RepositoryIndexLockedError(string operation, string message, int? exitCode, string? standardError)
        : base(operation, message, exitCode, standardError)
    {
    }
}
