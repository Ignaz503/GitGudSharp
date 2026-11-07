// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Commiting;

/// <summary>Sequencer subcommands used to continue or end an in-progress cherry-pick.</summary>
public enum CherryPickSequencerAction
{
    None = 0,
    Continue,
    Skip,
    Abort,
    Quit
}
