// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Restoring;

public interface IRestoreOperations
{
    Task<Result<Nothing, IError>> Restore(RestoreOptions options, CancellationToken cancellationToken = default);
}
