// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Constants;
using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

using FsckConst = GitGud.Constants.Const.Repository.FsCheck;

namespace GitGud.Repositories.Administration;

public sealed class FsCheckOperations : IFsCheckOperations
{
    private readonly ICommandExecutor _executor;

    public FsCheckOperations(ICommandExecutor executor)
        => _executor = executor;

    public async Task<Result<FsCheckResult, IError>> FsCheck(FsCheckOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new FsCheckOptions();

        var validation = options.Validate();
        if (validation.HasNone)
            return Result<FsCheckResult, IError>.None(validation.Handle());

        var output = await _executor.ExecuteAsync(
            Const.Repository.GIT_FSCK, FsckConst.Errors.FSCK_FAILED, options.ApplyOptionsToInvoke, null, cancellationToken).ConfigureAwait(false);

        if (output.HasNone)
            return Result<FsCheckResult, IError>.None(output.Handle());

        return Result<FsCheckResult, IError>.Some(Parse(output.Unwrap()));
    }

    internal static FsCheckResult Parse(string? output)
    {
        var findings = new List<FsCheckFinding>();
        if (string.IsNullOrEmpty(output))
            return new FsCheckResult(findings);

        var lines = output!.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
                continue;

            if (line.StartsWith(FsckConst.PREFIX_BROKEN_LINK, StringComparison.Ordinal))
            {
                var (fromKind, fromId) = KindAndId(line.Substring(FsckConst.PREFIX_BROKEN_LINK.Length));
                FsCheckObjectKind? toKind = null;
                string? toId = null;
                if (i + 1 < lines.Length)
                {
                    var next = lines[i + 1].Trim();
                    if (next.StartsWith(FsckConst.PREFIX_BROKEN_LINK_TARGET, StringComparison.Ordinal))
                    {
                        var (k, id) = KindAndId(next.Substring(FsckConst.PREFIX_BROKEN_LINK_TARGET.Length));
                        toKind = k;
                        toId = id;
                        i++;
                    }
                }

                findings.Add(new FsCheckFinding(FsCheckFindingKind.BrokenLink, fromKind, fromId, null, toKind, toId, null));
            }
            else if (line.StartsWith(FsckConst.PREFIX_DANGLING, StringComparison.Ordinal))
            {
                var (kind, id) = KindAndId(line.Substring(FsckConst.PREFIX_DANGLING.Length));
                findings.Add(new FsCheckFinding(FsCheckFindingKind.Dangling, kind, id, null, null, null, null));
            }
            else if (line.StartsWith(FsckConst.PREFIX_UNREACHABLE, StringComparison.Ordinal))
            {
                var (kind, id) = KindAndId(line.Substring(FsckConst.PREFIX_UNREACHABLE.Length));
                findings.Add(new FsCheckFinding(FsCheckFindingKind.Unreachable, kind, id, null, null, null, null));
            }
            else if (line.StartsWith(FsckConst.PREFIX_ROOT, StringComparison.Ordinal))
            {
                findings.Add(new FsCheckFinding(FsCheckFindingKind.Root, FsCheckObjectKind.Commit, FirstToken(line.Substring(FsckConst.PREFIX_ROOT.Length)), null, null, null, null));
            }
            else if (line.StartsWith(FsckConst.PREFIX_TAGGED, StringComparison.Ordinal))
            {
                findings.Add(ParseTagged(line.Substring(FsckConst.PREFIX_TAGGED.Length)));
            }
            else if (line.StartsWith(FsckConst.PREFIX_MISSING, StringComparison.Ordinal))
            {
                var (kind, id) = KindAndId(line.Substring(FsckConst.PREFIX_MISSING.Length));
                findings.Add(new FsCheckFinding(FsCheckFindingKind.Missing, kind, id, null, null, null, null));
            }
            else if (line.StartsWith(FsckConst.PREFIX_ERROR_IN, StringComparison.Ordinal))
            {
                findings.Add(ParseDiagnostic(FsCheckFindingKind.Error, line.Substring(FsckConst.PREFIX_ERROR_IN.Length)));
            }
            else if (line.StartsWith(FsckConst.PREFIX_WARNING_IN, StringComparison.Ordinal))
            {
                findings.Add(ParseDiagnostic(FsCheckFindingKind.Warning, line.Substring(FsckConst.PREFIX_WARNING_IN.Length)));
            }
        }

        return new FsCheckResult(findings);
    }

    private static (FsCheckObjectKind Kind, string Id) KindAndId(string text)
    {
        var trimmed = text.Trim();
        var space = trimmed.IndexOf(' ');
        if (space < 0)
            return (FsCheckObjectKind.Unknown, trimmed);

        return (trimmed.Substring(0, space).ParseKind(), FirstToken(trimmed.Substring(space + 1)));
    }

    // "<kind> <id> (<name>) in <containerId>"
    private static FsCheckFinding ParseTagged(string text)
    {
        var (kind, id) = KindAndId(text);
        string? name = null;
        string? container = null;

        var open = text.IndexOf('(');
        var inIndex = text.IndexOf(FsckConst.IN_SEPARATOR, StringComparison.Ordinal);
        if (open >= 0 && inIndex > open)
            name = text.Substring(open + 1, inIndex - open - 1);
        if (inIndex >= 0)
            container = FirstToken(text.Substring(inIndex + FsckConst.IN_SEPARATOR.Length));

        return new FsCheckFinding(FsCheckFindingKind.Tagged, kind, id, name, null, container, null);
    }

    // "<kind> <id>: <message>"
    private static FsCheckFinding ParseDiagnostic(FsCheckFindingKind findingKind, string text)
    {
        var separator = text.IndexOf(FsckConst.MESSAGE_SEPARATOR, StringComparison.Ordinal);
        var head = separator < 0 ? text : text.Substring(0, separator);
        var message = separator < 0 ? null : text.Substring(separator + FsckConst.MESSAGE_SEPARATOR.Length).Trim();
        var (kind, id) = KindAndId(head);

        return new FsCheckFinding(findingKind, kind, id, null, null, null, message);
    }

    private static string FirstToken(string text)
    {
        var trimmed = text.Trim();
        var space = trimmed.IndexOf(' ');
        return space < 0 ? trimmed : trimmed.Substring(0, space);
    }

}