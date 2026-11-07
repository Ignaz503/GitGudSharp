// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Utility;

public readonly struct Pair
{
    public Pair(string first, string second)
    {
        First = first;
        Second = second;
    }
    public Pair(string first)
    {
        First = first;
        Second = default;
    }
    public string? First { get; }
    public string? Second { get; }

    public bool TwoValues => Second is not null;
}