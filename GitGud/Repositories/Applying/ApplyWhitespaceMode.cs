// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using ApplyConst = GitGud.Constants.Const.Repository.Applying;
namespace GitGud.Repositories.Applying;

public enum ApplyWhitespaceMode
{
    Default,
    NoWarn,
    Warn,
    Fix,
    Error,
    ErrorAll
}

public static class ApplyWhitespaceModeExtensions
{
    public static string? ToArgumentValue(this ApplyWhitespaceMode mode)
        => mode switch
        {
            ApplyWhitespaceMode.NoWarn => ApplyConst.WHITESPACE_NOWARN,
            ApplyWhitespaceMode.Warn => ApplyConst.WHITESPACE_WARN,
            ApplyWhitespaceMode.Fix => ApplyConst.WHITESPACE_FIX,
            ApplyWhitespaceMode.Error => ApplyConst.WHITESPACE_ERROR,
            ApplyWhitespaceMode.ErrorAll => ApplyConst.WHITESPACE_ERROR_ALL,
            _ => null
        };

}
