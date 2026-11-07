// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Submodules;

public enum SubmoduleState
{
    Current,
    Uninitialized,
    Modified,
    Conflicted
}


public static class  SubmoduleStateExtensions
{
    public static SubmoduleState MapCharacterToSubmoduleState(this char c) 
        => c switch {
                '-' => SubmoduleState.Uninitialized,
                '+' => SubmoduleState.Modified,
                'U' => SubmoduleState.Conflicted,
                _ => SubmoduleState.Current
            };
    
}