// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

namespace GitGud.Utility;

/// <summary>
/// A unit-like type representing the absence of a meaningful value.
/// Useful when a generic type requires a success value but the operation
/// has no meaningful result to return.
/// For example, <see cref="Result{TSome,TNone}"/> with
/// <see cref="Nothing"/> as the success type.
/// </summary>
public readonly struct Nothing { }
