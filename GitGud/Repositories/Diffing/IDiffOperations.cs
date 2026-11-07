// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Diffing;

public interface IDiffOperations
{
    /// <summary>Shows the changes described by the options as a structured, parsed result.</summary>
    Task<Result<DiffResult, IError>> Diff(DiffOptions? options = null, CancellationToken cancellationToken = default);
}
