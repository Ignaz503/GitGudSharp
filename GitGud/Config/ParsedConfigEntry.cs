// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Config;

namespace GitGud;

internal readonly struct ParsedConfigEntry
{
    public ParsedConfigEntry(ConfigScope scope, string origin, string name, string? value)
        => (Scope, Origin, Name, Value) = (scope, origin, name, value);

    public ConfigScope Scope { get; }
    public string Origin { get; }
    public string Name { get; }
    public string? Value { get; }
}
