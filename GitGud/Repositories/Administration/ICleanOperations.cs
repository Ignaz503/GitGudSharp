// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Administration;

public interface ICleanOperations
{
    /// <summary>Removes untracked files from the working tree and returns git's output lines.</summary>
    Task<Result<IReadOnlyList<string>, IError>> Clean(CleanOptions options, CancellationToken cancellationToken = default);
}
