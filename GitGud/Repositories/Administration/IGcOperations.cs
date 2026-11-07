// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Administration;

public interface IGcOperations
{
    /// <summary>Cleans up unnecessary files and optimizes the local repository.</summary>
    Task<Result<Nothing, IError>> Gc(GcOptions? options = null, CancellationToken cancellationToken = default);
}
