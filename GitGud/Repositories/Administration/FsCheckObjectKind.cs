// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using FsckConst = GitGud.Constants.Const.Repository.FsCheck;
namespace GitGud.Repositories.Administration;

/// <summary>Type of a git object reported by <c>git fsck</c>.</summary>
public enum FsCheckObjectKind
{
    Unknown,
    Blob,
    Tree,
    Commit,
    Tag,
}

public static class FsCheckObjectKindExtensions
{
    public static FsCheckObjectKind ParseKind(this string text)
    {
        return text switch
        {
            FsckConst.KIND_BLOB => FsCheckObjectKind.Blob,
            FsckConst.KIND_TREE => FsCheckObjectKind.Tree,
            FsckConst.KIND_COMMIT => FsCheckObjectKind.Commit,
            FsckConst.KIND_TAG => FsCheckObjectKind.Tag,
            _ => FsCheckObjectKind.Unknown,
        };
    }
}
