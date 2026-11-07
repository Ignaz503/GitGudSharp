// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories;

public enum TagCleanupMode
{
    /// <summary>Uses git's default (strip).</summary>
    Default,
    Verbatim,
    Whitespace,
    Strip
}
