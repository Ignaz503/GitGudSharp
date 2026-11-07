// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Submodules;

public sealed class SubmoduleStatusInfo
{
    public SubmoduleStatusInfo(SubmoduleState state, string commit, string path, string? description)
    {
        State = state;
        Commit = commit;
        Path = path;
        Description = description;
    }

    public SubmoduleState State { get; }
    public string Commit { get; }
    public string Path { get; }
    public string? Description { get; }
}
