// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Text;

using DiffConst = GitGud.Constants.Const.Repository.Diffing;

namespace GitGud.Repositories.Diffing;

internal static class DiffParser
{
    public static DiffResult Parse(string? output)
    {
        var files = new List<FileDiff>();
        if (string.IsNullOrEmpty(output))
            return new DiffResult(files);

        var lines = output!.Split(DiffConst.LINE_SEPARATOR);
        var index = 0;
        while (index < lines.Length)
        {
            if (lines[index].StartsWith(DiffConst.FILE_HEADER, StringComparison.Ordinal))
                files.Add(ParseFile(lines, ref index));
            else
                index++;
        }

        return new DiffResult(files);
    }

    private static FileDiff ParseFile(string[] lines, ref int index)
    {
        var header = lines[index++];
        string? oldPath = null;
        string? newPath = null;
        string? oldMode = null;
        string? newMode = null;
        string? oldHash = null;
        string? newHash = null;
        int? similarity = null;
        var status = DiffFileStatus.Modified;
        var binary = false;

        while (index < lines.Length)
        {
            var line = lines[index];
            if (IsBoundary(line))
                break;

            index++;
            if (line.StartsWith(DiffConst.NEW_FILE_MODE, StringComparison.Ordinal))
            {
                status = DiffFileStatus.Added;
                newMode = Tail(line, DiffConst.NEW_FILE_MODE);
            }
            else if (line.StartsWith(DiffConst.DELETED_FILE_MODE, StringComparison.Ordinal))
            {
                status = DiffFileStatus.Deleted;
                oldMode = Tail(line, DiffConst.DELETED_FILE_MODE);
            }
            else if (line.StartsWith(DiffConst.OLD_MODE, StringComparison.Ordinal))
                oldMode = Tail(line, DiffConst.OLD_MODE);
            else if (line.StartsWith(DiffConst.NEW_MODE, StringComparison.Ordinal))
                newMode = Tail(line, DiffConst.NEW_MODE);
            else if (line.StartsWith(DiffConst.RENAME_FROM, StringComparison.Ordinal))
            {
                status = DiffFileStatus.Renamed;
                oldPath = Tail(line, DiffConst.RENAME_FROM);
            }
            else if (line.StartsWith(DiffConst.RENAME_TO, StringComparison.Ordinal))
                newPath = Tail(line, DiffConst.RENAME_TO);
            else if (line.StartsWith(DiffConst.COPY_FROM, StringComparison.Ordinal))
            {
                status = DiffFileStatus.Copied;
                oldPath = Tail(line, DiffConst.COPY_FROM);
            }
            else if (line.StartsWith(DiffConst.COPY_TO, StringComparison.Ordinal))
                newPath = Tail(line, DiffConst.COPY_TO);
            else if (line.StartsWith(DiffConst.SIMILARITY_INDEX, StringComparison.Ordinal))
                similarity = ParsePercent(Tail(line, DiffConst.SIMILARITY_INDEX));
            else if (line.StartsWith(DiffConst.INDEX, StringComparison.Ordinal))
                ParseIndex(Tail(line, DiffConst.INDEX), ref oldHash, ref newHash, ref oldMode, ref newMode);
            else if (line.StartsWith(DiffConst.BINARY_FILES, StringComparison.Ordinal))
                binary = true;
            else if (line.StartsWith(DiffConst.OLD_FILE, StringComparison.Ordinal))
                oldPath = StripPrefix(Tail(line, DiffConst.OLD_FILE), DiffConst.OLD_PREFIX) ?? oldPath;
            else if (line.StartsWith(DiffConst.NEW_FILE, StringComparison.Ordinal))
                newPath = StripPrefix(Tail(line, DiffConst.NEW_FILE), DiffConst.NEW_PREFIX) ?? newPath;
        }

        if (status == DiffFileStatus.Added)
            oldPath = null;
        else if (status == DiffFileStatus.Deleted)
            newPath = null;

        if ((oldPath is null && status != DiffFileStatus.Added) || (newPath is null && status != DiffFileStatus.Deleted))
        {
            var fallback = PathFromHeader(header);
            if (oldPath is null && status != DiffFileStatus.Added)
                oldPath = fallback;
            if (newPath is null && status != DiffFileStatus.Deleted)
                newPath = fallback;
        }

        var hunks = new List<DiffHunk>();
        while (index < lines.Length && lines[index].StartsWith(DiffConst.HUNK_HEADER, StringComparison.Ordinal))
        {
            var hunk = ParseHunk(lines, ref index);
            if (hunk is null)
                break;

            hunks.Add(hunk);
        }

        return new FileDiff(status, oldPath, newPath, oldMode, newMode, oldHash, newHash, similarity, binary, hunks);
    }

    private static DiffHunk? ParseHunk(string[] lines, ref int index)
    {
        var header = lines[index++];
        if (!TryParseHunkHeader(header, out var oldStart, out var oldCount, out var newStart, out var newCount, out var section))
            return null;

        var body = new List<DiffLine>();
        var oldLine = oldStart;
        var newLine = newStart;
        var oldRemaining = oldCount;
        var newRemaining = newCount;
        var additions = 0;
        var deletions = 0;

        while (index < lines.Length)
        {
            var line = lines[index];
            if (line.Length == 0)
                break;

            var marker = line[0];
            if (marker == DiffConst.NO_NEWLINE)
            {
                if (body.Count > 0)
                    body[body.Count - 1] = body[body.Count - 1].WithNoNewlineAtEnd();

                index++;
                continue;
            }

            if (oldRemaining <= 0 && newRemaining <= 0)
                break;

            if (marker == DiffConst.ADDED)
            {
                body.Add(new DiffLine(DiffLineKind.Added, line.Substring(1), null, newLine++));
                newRemaining--;
                additions++;
            }
            else if (marker == DiffConst.REMOVED)
            {
                body.Add(new DiffLine(DiffLineKind.Removed, line.Substring(1), oldLine++, null));
                oldRemaining--;
                deletions++;
            }
            else if (marker == DiffConst.CONTEXT)
            {
                body.Add(new DiffLine(DiffLineKind.Context, line.Substring(1), oldLine++, newLine++));
                oldRemaining--;
                newRemaining--;
            }
            else
                break;

            index++;
        }

        return new DiffHunk(oldStart, oldCount, newStart, newCount, section, body, additions, deletions);
    }

    private static bool TryParseHunkHeader(string line, out int oldStart, out int oldCount, out int newStart, out int newCount, out string? section)
    {
        oldStart = oldCount = newStart = newCount = 0;
        section = null;

        if (!line.StartsWith(DiffConst.HUNK_OLD_PREFIX, StringComparison.Ordinal))
            return false;

        var position = DiffConst.HUNK_OLD_PREFIX.Length;
        if (!ReadRange(line, ref position, out oldStart, out oldCount))
            return false;

        if (string.CompareOrdinal(line, position, DiffConst.HUNK_NEW_PREFIX, 0, DiffConst.HUNK_NEW_PREFIX.Length) != 0)
            return false;

        position += DiffConst.HUNK_NEW_PREFIX.Length;
        if (!ReadRange(line, ref position, out newStart, out newCount))
            return false;

        if (string.CompareOrdinal(line, position, DiffConst.HUNK_SUFFIX, 0, DiffConst.HUNK_SUFFIX.Length) != 0)
            return false;

        position += DiffConst.HUNK_SUFFIX.Length;
        if (position < line.Length)
        {
            var text = line.Substring(position).Trim();
            if (text.Length > 0)
                section = text;
        }

        return true;
    }

    private static bool ReadRange(string line, ref int position, out int start, out int count)
    {
        count = 1;
        if (!ReadNumber(line, ref position, out start))
            return false;

        if (position < line.Length && line[position] == DiffConst.RANGE_SEPARATOR)
        {
            position++;
            return ReadNumber(line, ref position, out count);
        }

        return true;
    }

    private static bool ReadNumber(string line, ref int position, out int value)
    {
        value = 0;
        var begin = position;
        while (position < line.Length && line[position] >= '0' && line[position] <= '9')
        {
            value = (value * 10) + (line[position] - '0');
            position++;
        }

        return position > begin;
    }

    private static void ParseIndex(string text, ref string? oldHash, ref string? newHash, ref string? oldMode, ref string? newMode)
    {
        var space = text.IndexOf(DiffConst.SPACE);
        var hashes = space < 0 ? text : text.Substring(0, space);
        if (space >= 0)
        {
            var mode = text.Substring(space + 1);
            oldMode ??= mode;
            newMode ??= mode;
        }

        var separator = hashes.IndexOf(DiffConst.HASH_SEPARATOR, StringComparison.Ordinal);
        if (separator < 0)
            return;

        oldHash = hashes.Substring(0, separator);
        newHash = hashes.Substring(separator + DiffConst.HASH_SEPARATOR.Length).TrimStart('.');
    }

    private static int? ParsePercent(string text)
    {
        var position = 0;
        return ReadNumber(text, ref position, out var value) ? value : null;
    }

    private static string? StripPrefix(string path, string prefix)
    {
        path = path.TrimEnd(DiffConst.TAB);
        if (path == DiffConst.DEV_NULL)
            return null;

        return path.StartsWith(prefix, StringComparison.Ordinal) ? path.Substring(prefix.Length) : path;
    }

    private static string? PathFromHeader(string header)
    {
        var rest = header.Substring(DiffConst.FILE_HEADER.Length);
        var length = rest.Length - DiffConst.HEADER_PATH_OVERHEAD;
        if (length <= 0 || length % 2 != 0)
            return null;

        return rest.Substring(DiffConst.OLD_PREFIX.Length, length / 2);
    }

    private static bool IsBoundary(string line)
        => line.StartsWith(DiffConst.HUNK_HEADER, StringComparison.Ordinal) || line.StartsWith(DiffConst.FILE_HEADER, StringComparison.Ordinal);

    private static string Tail(string line, string prefix)
        => line.Substring(prefix.Length);
}
