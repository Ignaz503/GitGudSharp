// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

namespace GitGud.Config;

public enum ConfigValueType
{
    String,
    Boolean,
    BoolOrInt,
    Integer,
    Color,
    Pathname,
    ExpiryDate
}


public static class ConfigValueTypeExtensions
{
    public static string? GetTypeArgument(this ConfigValueType type)
        => type switch
        {
            ConfigValueType.String => null,
            ConfigValueType.Boolean => Const.Config.Arguments.TYPE_BOOLEAN,
            ConfigValueType.Integer => Const.Config.Arguments.TYPE_INTEGER,
            ConfigValueType.Color => Const.Config.Arguments.TYPE_COLOR,
            ConfigValueType.Pathname => Const.Config.Arguments.TYPE_PATHNAME,
            ConfigValueType.BoolOrInt => Const.Config.Arguments.TYPE_BOOL_OR_INT,
            ConfigValueType.ExpiryDate => Const.Config.Arguments.TYPE_EXPIRY_DATE,
            _ => Const.Config.Arguments.TYPE_STRING
        };
}