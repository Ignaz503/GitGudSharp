// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;

using System.Globalization;

using LogConst = GitGud.Constants.Const.Repository.Logging;

namespace GitGud.Repositories.Logging;

internal static class LogParser
{
    public static IReadOnlyList<LogEntry> Parse(string? output, bool patch = false)
    {
        var entries = new List<LogEntry>();
        if (string.IsNullOrEmpty(output))
            return entries;

        var records = output!.Split(LogConst.RECORD_SEPARATOR);
        for (var i = 0; i < records.Length; i++)
        {
            var record = records[i];
            if (record.Length == 0)
                continue;

            var fields = record.Split(LogConst.FIELD_SEPARATOR);
            if (fields.Length < LogConst.FIELD_COUNT)
                continue;

            var parents = fields[1].Length == 0
                ? Array.Empty<string>()
                : fields[1].Split(LogConst.PARENT_SEPARATOR);

            entries.Add(new LogEntry(
                fields[0],
                parents,
                new GitSignature(fields[2], fields[3], ParseDate(fields[4])),
                new GitSignature(fields[5], fields[6], ParseDate(fields[7])),
                ParseDecorations(fields[8]),
                fields[9],
                fields[10].TrimEnd('\r', '\n'),
                patch ? Array.Empty<LogFileChange>() : ParseFiles(fields[11]),
                patch ? fields[11].Trim('\r', '\n') : string.Empty));
        }

        return entries;
    }

    private static DateTimeOffset ParseDate(string value)
        => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : default;

    private static IReadOnlyList<RefDecoration> ParseDecorations(string value)
    {
        if (value.Length == 0)
            return Array.Empty<RefDecoration>();

        var parts = value.Split(new[] { LogConst.DECORATION_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<RefDecoration>(parts.Length);
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part == LogConst.HEAD)
                result.Add(new HeadDecoration(part, null));
            else if (part.StartsWith(LogConst.HEAD_ARROW, StringComparison.Ordinal))
            {
                var name = part.Substring(LogConst.HEAD_ARROW.Length);
                result.Add(new HeadDecoration(LogConst.HEAD, new Branch(StripPrefix(name, LogConst.REFS_HEADS))));
            }
            else if (part.StartsWith(LogConst.TAG_PREFIX, StringComparison.Ordinal))
                result.Add(new TagDecoration(new Tag(StripPrefix(part.Substring(LogConst.TAG_PREFIX.Length), LogConst.REFS_TAGS))));
            else if (part.StartsWith(LogConst.REFS_HEADS, StringComparison.Ordinal))
                result.Add(new LocalBranchDecoration(new Branch(part.Substring(LogConst.REFS_HEADS.Length))));
            else if (part.StartsWith(LogConst.REFS_REMOTES, StringComparison.Ordinal))
            {
                var name = part.Substring(LogConst.REFS_REMOTES.Length);
                var slash = name.IndexOf(LogConst.REMOTE_SEPARATOR);
                result.Add(new RemoteBranchDecoration(new Branch(name), slash > 0 ? name.Substring(0, slash) : string.Empty));
            }
            else
                result.Add(new OtherDecoration(part));
        }

        return result;
    }

    private static string StripPrefix(string value, string prefix)
        => value.StartsWith(prefix, StringComparison.Ordinal) ? value.Substring(prefix.Length) : value;

    private static IReadOnlyList<LogFileChange> ParseFiles(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Array.Empty<LogFileChange>();

        var files = new List<LogFileChange>();
        var lines = value.Split(LogConst.LINE_SEPARATOR);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (line.Length == 0)
                continue;

            var parts = line.Split(LogConst.STATUS_SEPARATOR);
            if (parts.Length < 2)
                continue;

            var kind = parts[0][0].ToKind();
            if (parts.Length >= 3 && (kind == FileChangeKind.Renamed || kind == FileChangeKind.Copied))
            {
                int.TryParse(parts[0].Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var similarity);
                files.Add(new RenamedFileChange(parts[2], kind, parts[1], similarity));
            }
            else
                files.Add(new LogFileChange(parts[1], kind));
        }

        return files;
    }
}
