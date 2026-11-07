// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Utility;

namespace GitGud.Repositories;

public interface ICloneOptions 
{
    public DirectoryPath Into { get; init; }
    public IRepositorySource Source { get; init; }
    public bool Bare { get; }
    public string? Branch { get; }
    public int? Depth { get; }

    /// <summary>
    /// Initializes and clones all submodules (--recurse-submodules). Ignored when <see cref="Submodules"/> is non-empty.
    /// </summary>
    public bool RecurseSubmodules { get; }

    /// <summary>
    /// Specific submodules to initialize on clone (--recurse-submodules=&lt;path&gt;). Null or empty means none unless <see cref="RecurseSubmodules"/> is set.
    /// </summary>
    public IReadOnlyList<SubmoduleSpec>? Submodules { get; }

    /// <summary>
    /// Clones the submodules shallowly (--shallow-submodules). Only applies when submodules are cloned.
    /// </summary>
    public bool ShallowSubmodules { get; }

}
