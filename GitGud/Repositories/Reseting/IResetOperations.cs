// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Reseting;

public interface IResetOperations
{
    Task<Result<Nothing, IError>> Reset(ResetOptions? options = null, CancellationToken cancellationToken = default);
}
