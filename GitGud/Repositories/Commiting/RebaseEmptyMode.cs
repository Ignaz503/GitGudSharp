// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Commiting;

/// <summary>Handling of commits that become empty (--empty).</summary>
public enum RebaseEmptyMode
{
    Default = 0,
    Drop,
    Keep,
    Stop,
    /// <summary>Deprecated synonym of <see cref="Stop"/>.</summary>
    Ask
}
