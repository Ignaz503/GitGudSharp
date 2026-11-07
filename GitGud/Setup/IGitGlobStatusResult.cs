// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Setup;

public interface IGitGlobStatusResult : IResult
{
    public bool IsInstalled { get; }

    public bool IsInPATH { get; }

    public string? InstallPath { get; }

    public string? Version { get; }

    /// <summary>
    /// The numeric part of <see cref="Version"/>, or null when it could not be parsed.
    /// </summary>
    public System.Version? ParsedVersion { get; }

}
