// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud;

public interface IConfigCollection 
{
    Task<Result<Nothing, IError>> Apply(IConfiguration applyTo, CancellationToken cancellationToken = default);
}
