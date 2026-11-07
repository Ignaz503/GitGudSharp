// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Config;
using GitGud.Constants;

namespace GitGud;

internal static class ConfigOutputParser
{
    public static List<ParsedConfigEntry> Parse(string? output, bool hasScope, bool hasOrigin)
    {
        var entries = new List<ParsedConfigEntry>();
        if (string.IsNullOrEmpty(output))
            return entries;

        var text = output!;
        var position = 0;
        while (position < text.Length)
        {
            var scope = ConfigScope.Default;
            var origin = string.Empty;

            if (hasScope)
            {
                if (!TryReadToken(text, ref position, out var scopeToken))
                    break;
                scope = ParseScope(scopeToken);
            }

            if (hasOrigin)
            {
                if (!TryReadToken(text, ref position, out var originToken))
                    break;
                origin = originToken.StartsWith(Const.Config.Flags.ORIGIN_FILE_PREFIX, StringComparison.Ordinal)
                    ? originToken.Substring(Const.Config.Flags.ORIGIN_FILE_PREFIX.Length)
                    : originToken;
            }

            if (!TryReadToken(text, ref position, out var entryToken))
                break;

            var separator = entryToken.IndexOf('\n');
            if (separator < 0)
                separator = entryToken.IndexOf(' ');

            entries.Add(separator < 0
                ? new ParsedConfigEntry(scope, origin, entryToken, null)
                : new ParsedConfigEntry(scope, origin, entryToken.Substring(0, separator), entryToken.Substring(separator + 1)));
        }

        return entries;
    }

    private static bool TryReadToken(string text, ref int position, out string token)
    {
        if (position >= text.Length)
        {
            token = string.Empty;
            return false;
        }

        var end = text.IndexOf('\0', position);
        if (end < 0)
            end = text.Length;

        token = text.Substring(position, end - position);
        position = end + 1;
        return true;
    }

    private static ConfigScope ParseScope(string value)
        => value switch
        {
            Const.Config.Arguments.SYSTEM => ConfigScope.System,
            Const.Config.Arguments.GLOBAL => ConfigScope.Global,
            Const.Config.Arguments.LOCAL => ConfigScope.Local,
            Const.Config.Arguments.WORKTREE => ConfigScope.Worktree,
            Const.Config.Flags.SCOPE_COMMAND => ConfigScope.Command,
            _ => ConfigScope.Default
        };

    public static string EscapeRegex(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        foreach (var character in value)
        {
            if (Const.Config.Flags.REGEX_SPECIAL_CHARACTERS.IndexOf(character) >= 0)
                builder.Append('\\');
            builder.Append(character);
        }

        return builder.ToString();
    }
}
