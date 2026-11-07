// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

public enum RebaseMergesMode
{
    None = 0,
    /// <summary>--rebase-merges</summary>
    Default,
    /// <summary>--rebase-merges=rebase-cousins</summary>
    RebaseCousins,
    /// <summary>--rebase-merges=no-rebase-cousins</summary>
    NoRebaseCousins,
    /// <summary>--no-rebase-merges</summary>
    Disabled
}