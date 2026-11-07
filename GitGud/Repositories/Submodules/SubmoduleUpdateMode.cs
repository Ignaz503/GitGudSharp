// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

public enum SubmoduleUpdateMode
{
    Default,
    Checkout,
    Rebase,
    Merge
}


public static class SubmoduleUpdateMopdeExtensions
{
    
    public static SubmoduleUpdateMode GetSubmoduleUpdateMode(this string mode)
        => mode switch
        {
            SubConst.CHECKOUT => SubmoduleUpdateMode.Checkout,
            SubConst.REBASE => SubmoduleUpdateMode.Rebase,
            SubConst.MERGE => SubmoduleUpdateMode.Merge,
            _ => SubmoduleUpdateMode.Default
        };
}