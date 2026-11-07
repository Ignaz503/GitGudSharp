// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GitGud.CliArguments;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Repositories.Applying;

public interface IApplyOperations
{
    /// <summary>Runs git apply. Returns the standard output, e.g. for --stat, --numstat and --summary.</summary>
    Task<Result<string?, IError>> ApplyPatch(ApplyOptions options, CancellationToken cancellationToken = default);
}
