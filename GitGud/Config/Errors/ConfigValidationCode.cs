// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud.Config.Errors;

public enum ConfigValidationCode
{
    InvalidKey,
    ValueRequired,
    FileRequired,
    InvalidOptions,
    UnsupportedScope
}


public static class ConfigValidationCodeExtensions 
{
    public static string GetMessage(this ConfigValidationCode code)
        => code switch
        {
            ConfigValidationCode.InvalidKey => Const.Config.Errors.INVALID_KEY,
            ConfigValidationCode.ValueRequired => Const.Config.Errors.VALUE_REQUIRED,
            ConfigValidationCode.FileRequired => Const.Config.Errors.FILE_REQUIRED,
            ConfigValidationCode.InvalidOptions => Const.Config.Errors.INVALID_OPTIONS,
            ConfigValidationCode.UnsupportedScope => Const.Config.Errors.UNSUPPORTED_SCOPE,
            _ => Const.Config.Errors.INVALID_KEY
        };
}