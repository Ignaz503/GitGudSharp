// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Utility;

namespace GitGud;

public struct ValuePattern 
{
    public RegexPattern? RegexPattern { get; init; } = null;
    public bool DoFixedValue { get; init; } = true;

    public ValuePattern()
    {
    }
}
