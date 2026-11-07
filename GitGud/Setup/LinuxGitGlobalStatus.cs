// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Setup;

public class LinuxGitGlobalStatus : IGitGlobStatusResult{
    /// <summary>
    /// Indicates if installed or not.
    /// </summary>
    public bool IsInstalled { get; init; }

    public bool IsInPATH { get; init; }

    /// <summary>
    /// Indicates the path to the installed exe file.
    /// </summary>
    public string? InstallPath { get; init; }

    public string? Version { get; init; }

    public System.Version? ParsedVersion { get; init; }
}
