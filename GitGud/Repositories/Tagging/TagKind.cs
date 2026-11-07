// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Tagging;

public enum TagKind
{
    /// <summary>A reference pointing directly at an object.</summary>
    Lightweight,

    /// <summary>A tag object (annotated or signed).</summary>
    Annotated
}
