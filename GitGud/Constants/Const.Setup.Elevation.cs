// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Setup
    {
        public static class Elevation
        {
            public const string LIBC = "libc";
            public const string GETEUID = "geteuid";
            public const string REQUIRED_WINDOWS = "This operation requires the process to be started as an administrator.";
            public const string REQUIRED_UNIX = "This operation requires the process to be started as root (for example with sudo).";
        }
    }
}
