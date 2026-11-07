// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Utility;

namespace GitGud.Config;

public readonly record struct ConfigEntry( string? Name, string? Value);
public readonly record struct ScopedConfigEntry(ConfigScope Scope, string? Name, string? Value);
public readonly record struct ConfigEntryNameOnly(string? Name);
public readonly record struct ScopedConfigEntryNameOnly(ConfigScope Scope, string? Name);
    
public readonly record struct OriginConfigEntry(FilePath Origin, string? Name, string? Value);

public readonly record struct ScopedOriginConfigEntry(ConfigScope Scope, FilePath Origin, string? Name, string? Value);
public readonly record struct OriginConfigEntryNameOnly(FilePath Origin, string? Name);
public readonly record struct ScopedOriginConfigEntryNameOnly(ConfigScope Scope, FilePath Origin, string? Name);