// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Commiting;

/// <summary>Mode options. They cannot be combined with any other option.</summary>
public enum RebaseMode
{
    None = 0,
    Continue,
    Skip,
    Abort,
    Quit,
    EditTodo,
    ShowCurrentPatch
}
