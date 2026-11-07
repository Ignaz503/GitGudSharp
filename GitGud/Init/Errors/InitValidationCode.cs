// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Init.Errors;

public enum InitValidationCode
{
    OptionsRequired,
    TargetPathRequired,
    InvalidTargetPath,
    InvalidObjectFormat,
    InvalidRefStorageFormat
}

public static class InitValidationCodeExtensions 
{
    public static string GetMessage(this InitValidationCode code)
    => code switch {
        InitValidationCode.OptionsRequired => Const.Init.Errors.OPTIONS_REQUIRED,
        InitValidationCode.TargetPathRequired => Const.Init.Errors.TARGET_PATH_REQUIRED,
        InitValidationCode.InvalidObjectFormat => Const.Init.Errors.OBJECT_FORMAT_INVALID,
        InitValidationCode.InvalidRefStorageFormat => Const.Init.Errors.REF_FORMAT_INVALID,
        _ => Const.Init.Errors.TARGET_PATH_INVALID
    };
}

