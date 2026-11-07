// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Submodules;

/// <summary>Base for commands accepting an optional "[--] [path...]" list.</summary>
public abstract class SubmodulePathOptionsBase : SubmoduleOptionsBase
{
    private protected SubmodulePathOptionsBase(int size) : base(size) { }

    private readonly IReadOnlyList<string> _paths = Array.Empty<string>();

    /// <summary>Limits the operation to the given submodule paths.</summary>
    public IReadOnlyList<string> Paths
    {
        get => _paths;
        init => _paths = value ?? Array.Empty<string>();
    }
}
