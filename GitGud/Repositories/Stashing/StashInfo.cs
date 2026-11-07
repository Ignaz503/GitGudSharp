// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories.Stashing;

public sealed class StashInfo
{
    public StashInfo(int index, string commit, string message)
    {
        Index = index;
        Commit = commit;
        Message = message;
    }

    /// <summary>Position in the stash list; 0 is the most recent.</summary>
    public int Index { get; }

    public string Commit { get; }

    /// <summary>Reflog subject, e.g. "On main: my message".</summary>
    public string Message { get; }
}
