// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Merging;

public interface IMergeOperations
{
    Task<Result<Nothing, IError>> Merge(MergeOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> AbortMerge(CancellationToken cancellationToken = default);
}
