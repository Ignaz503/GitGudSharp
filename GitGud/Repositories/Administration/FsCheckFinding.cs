// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Administration;

/// <summary>A single finding reported by <c>git fsck</c>.</summary>
/// <param name="Kind"></param>
/// <param name="ObjectKind"></param>
/// <param name="ObjectId"></param>
/// <param name="Name">Tag name for <see cref="FsCheckFindingKind.Tagged"/> findings.</param>
/// <param name="TargetKind">Kind of the linked object for broken links.</param>
/// <param name="TargetId">Id of the linked object for broken links, or the containing object for tagged findings.</param>
/// <param name="Message">Diagnostic text for errors and warnings.</param>
public sealed record FsCheckFinding(FsCheckFindingKind Kind, FsCheckObjectKind ObjectKind, string ObjectId, string? Name, FsCheckObjectKind? TargetKind, string? TargetId, string? Message);
