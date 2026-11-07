// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud.Config;

public enum ConfigScope
{
    Default,
    System,
    Global,
    Local,
    Worktree,
    File,
    Command
}


public static class ConfigScopeExtensions 
{
    public static string GetScopeArgument(this ConfigScope scope)
        => scope switch
        {
            ConfigScope.System => Const.Config.Arguments.SYSTEM,
            ConfigScope.Global => Const.Config.Arguments.GLOBAL,
            ConfigScope.Local => Const.Config.Arguments.LOCAL,
            ConfigScope.Worktree => Const.Config.Arguments.WORKTREE,
            ConfigScope.Default => string.Empty,
            _ => string.Empty
        };
}