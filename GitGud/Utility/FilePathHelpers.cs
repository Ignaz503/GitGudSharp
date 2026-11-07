// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using System.IO;

using GitGud.Constants;

namespace GitGud.Utility;

public static class FilePathHelpers 
{
    public static string? FindOnPath(string executableName)
    {
        var path = Environment.GetEnvironmentVariable(Const.Setup.Status.PATH_ENVIRONMENT_VARIABLE);
        if (string.IsNullOrWhiteSpace(path))
            return null;

        foreach (var entry in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            var candidate = Path.Combine(entry.Trim(), executableName);
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        return null;
    }

    public static string? FindInstalledExecutable(bool isWindows, bool isMac)
    {
        if (isWindows)
        {
            var programFilesPath = FindWindowsInstallation(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            if (programFilesPath is not null)
                return programFilesPath;

            var programFilesX86Path = FindWindowsInstallation(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            if (programFilesX86Path is not null)
                return programFilesX86Path;

            var userPrograms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Const.Setup.Status.USER_PROGRAMS_DIRECTORY);
            return FindWindowsInstallation(userPrograms);
        }

        return FindInKnownPaths(isMac ? Const.Setup.MacGitPaths : Const.Setup.LinuxGitPaths);
    }

    public static string? FindWindowsInstallation(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return null;

        var installationDirectory = Path.Combine(root, Const.Setup.Status.WINDOWS_INSTALL_DIRECTORY);
        var commandPath = Path.Combine(installationDirectory, Const.Setup.Status.WINDOWS_COMMAND_DIRECTORY, Const.Setup.Status.WINDOWS_EXECUTABLE);
        if (File.Exists(commandPath))
            return Path.GetFullPath(commandPath);

        var binaryPath = Path.Combine(installationDirectory, Const.Setup.Status.WINDOWS_BIN_DIRECTORY, Const.Setup.Status.WINDOWS_EXECUTABLE);
        return File.Exists(binaryPath) ? Path.GetFullPath(binaryPath) : null;
    }

    public static string? FindInKnownPaths(string[] paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }
}