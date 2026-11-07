// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Utility;

namespace GitGud;

public static class FileOptions 
{
    public static readonly FileOption Local = new FileOption.Local();
    public static readonly FileOption Global = new FileOption.Global();
    public static readonly FileOption System = new FileOption.System();
    public static readonly FileOption Worktree = new FileOption.Worktree();

    public static FileOption ForFile(FilePath value) => new FileOption.File(value);
}
