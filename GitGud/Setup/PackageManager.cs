// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Setup;

enum PackageManager
{
    Winget,
    Chocolatey,
    Scoop,
    Brew,
    AptGet,
    Apt,
    Dnf,
    Yum,
    Pacman,
    Zypper,
    Apk
}


static class PackageManagers 
{
    public static readonly PackageManager[] WindowsManagers =
    {
        PackageManager.Winget,
        PackageManager.Chocolatey,
        PackageManager.Scoop
    };

    public static readonly PackageManager[] LinuxManagers =
    {
        PackageManager.AptGet,
        PackageManager.Apt,
        PackageManager.Dnf,
        PackageManager.Yum,
        PackageManager.Pacman,
        PackageManager.Zypper,
        PackageManager.Apk
    };

    public static readonly PackageManager[] MacManagers =
    {
        PackageManager.Brew
    };

    public static bool RequiresElevation(this PackageManager packageManager)
        => packageManager switch
        {
            PackageManager.Winget => false,
            PackageManager.Scoop => false,
            PackageManager.Brew => false,
            _ => true
        };

    public static bool TryGetExecutable(this PackageManager packageManager, out string executable)
    {
        executable = packageManager switch
        {
            PackageManager.Winget => Const.Setup.Executables.WINGET,
            PackageManager.Chocolatey => Const.Setup.Executables.CHOCOLATEY,
            PackageManager.Scoop => Const.Setup.Executables.SCOOP,
            PackageManager.Brew => Const.Setup.Executables.BREW,
            PackageManager.AptGet => Const.Setup.Executables.APT_GET,
            PackageManager.Apt => Const.Setup.Executables.APT,
            PackageManager.Dnf => Const.Setup.Executables.DNF,
            PackageManager.Yum => Const.Setup.Executables.YUM,
            PackageManager.Pacman => Const.Setup.Executables.PACMAN,
            PackageManager.Zypper => Const.Setup.Executables.ZYPPER,
            PackageManager.Apk => Const.Setup.Executables.EXECUTABLE_APK,
            _ => string.Empty
        };
        return executable.Length > 0;
    }
}
