// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

namespace GitGud.Repositories.Merging;

/// <summary>Called once per conflicted file and returns how to resolve it.</summary>
public delegate Task<MergeResolution> MergeConflictResolver(MergeConflict conflict, CancellationToken cancellationToken);
