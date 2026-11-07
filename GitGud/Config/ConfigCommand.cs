// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap;

using GitGud.CliArguments;
using GitGud.Constants;

namespace GitGud.Config;

internal static class ConfigCommand
{
    public static Command Create(string executable)
        => Cli.Wrap(executable);

    public static void AddPrefix(CliWrap.Builders.ArgumentsBuilder arguments, ConfigScope scope, string? filePath)
    {
        arguments.Add(Const.CONFIG, true);
        AddScope(arguments, scope, filePath);
    }

    public static void AddScope(CliWrap.Builders.ArgumentsBuilder arguments, ConfigScope scope, string? filePath)
    {
        if (filePath is not null)
        {
            arguments.Add(new DoubleDashedArgument<string>(Const.Config.Arguments.FILE, filePath), true);
            return;
        }

        var scopeArgument = scope.GetScopeArgument();

        if (!string.IsNullOrEmpty(scopeArgument))
            arguments.Add(new DoubleDashedArgument(scopeArgument), true);
    }

    public static void AddFlag(CliWrap.Builders.ArgumentsBuilder arguments, string name)
        => arguments.Add(name.StartsWith("--", StringComparison.Ordinal) ? name : new DoubleDashedArgument(name).ToString(), true);

    public static void AddOption(CliWrap.Builders.ArgumentsBuilder arguments, string name, string value)
        => arguments.Add(name.StartsWith("--", StringComparison.Ordinal) ? string.Concat(name, "=", value) : new DoubleDashedArgument<string>(name, value).ToString(), true);
}
