// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Remotes;

public interface IRemoteOperations
{
    Task<Result<Nothing, IError>> Fetch(FetchOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> Pull(PullOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> Push(PushOptions? options = null, CancellationToken cancellationToken = default);
}
