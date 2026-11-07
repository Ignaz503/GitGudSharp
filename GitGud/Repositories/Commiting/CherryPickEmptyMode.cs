// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Commiting;

/// <summary>How redundant (already applied) commits are handled (--empty).</summary>
public enum CherryPickEmptyMode
{
    Default = 0,
    Drop,
    Keep,
    Stop
}
