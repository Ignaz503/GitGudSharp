// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud.Repositories.Branching;

internal static class BranchNameRules
{
    /// <summary>Returns <paramref name="missingError"/> for blank values, NAME_INVALID for values starting with a dash, otherwise null.</summary>
    internal static string? Validate(string? value, string missingError)
    {
        if (string.IsNullOrWhiteSpace(value))
            return missingError;
        if (value![0] == Const.Repository.Branching.OPTION_PREFIX)
            return Const.Repository.Branching.Errors.NAME_INVALID;

        return null;
    }
}