// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud;

public readonly record struct RepositorySharing(ushort Value)
{
    private const ushort ModeMask = 0xF000;
    private const ushort CustomMask = 0x0FFF;

    public static RepositorySharing False => new(0x1000);
    public static RepositorySharing True => new(0x2000);
    public static RepositorySharing Umask => new(0x3000);
    public static RepositorySharing Group => new(0x4000);
    public static RepositorySharing All => new(0x5000);

    // Aliases
    public static RepositorySharing World => All;
    public static RepositorySharing Everybody => All;

    public static RepositorySharing Custom(ushort mode) =>
        new((ushort)(mode & CustomMask));

    public bool IsCustom => (Value & ModeMask) == 0;

    public ushort CustomMode =>
        IsCustom ? (ushort)(Value & CustomMask) : (ushort)0;

    public override string ToString() =>
        (Value & ModeMask) switch
        {
            0x1000 => Const.FORMAT_REPOSITORY_SHARING_FALSE,
            0x2000 => Const.FORMAT_REPOSITORY_SHARING_TRUE,
            0x3000 => Const.FORMAT_REPOSITORY_SHARING_UMASK,
            0x4000 => Const.FORMAT_REPOSITORY_SHARING_GROUP,
            0x5000 => Const.FORMAT_REPOSITORY_SHARING_ALL,
            _ => Convert.ToString(Value & CustomMask, 8)
        };
}



