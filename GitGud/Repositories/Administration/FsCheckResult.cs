// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

namespace GitGud.Repositories.Administration;

/// <summary>Parsed output of <c>git fsck</c>.</summary>
public sealed class FsCheckResult
{
    public FsCheckResult(IReadOnlyList<FsCheckFinding> findings)
        => Findings = findings;

    public IReadOnlyList<FsCheckFinding> Findings { get; }

    public bool IsClean
    {
        get
        {
            for (var i = 0; i < Findings.Count; i++)
            {
                var kind = Findings[i].Kind;
                if (kind == FsCheckFindingKind.Missing || kind == FsCheckFindingKind.BrokenLink || kind == FsCheckFindingKind.Error)
                    return false;
            }

            return true;
        }
    }
}
