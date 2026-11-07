// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Administration;

public interface IFsCheckOperations
{
    /// <summary>Verifies the connectivity and validity of the objects in the database.</summary>
    Task<Result<FsCheckResult, IError>> FsCheck(FsCheckOptions? options = null, CancellationToken cancellationToken = default);
}
