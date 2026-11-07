// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Logging;

/// <summary>Identity and timestamp of a commit author or committer.</summary>
public readonly record struct GitSignature(string Name, string Email, DateTimeOffset Date);
