// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using CliWrap.Builders;

using GitGud.CliArguments;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Repositories;

/// <summary>Runs git in a repository and maps failures to <see cref="IError"/>. Replace to test operations without spawning processes.</summary>
public interface ICommandExecutor
{
    /// <summary>Executes git and returns standard output on success.</summary>
    Task<Result<string?, IError>> ExecuteAsync(
        string operation,
        string failureMessage,
        Action<CliWrapArgumentBuilder> buildArguments,
        string? validationError,
        CancellationToken cancellationToken);
}
