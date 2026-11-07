// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap;
using System.Threading;
using System.Text;

namespace GitGud.Processes;

internal readonly struct ProcessResult
{
    public ProcessResult(int exitCode, string? standardOutput, string? standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    public int ExitCode { get; }
    public string? StandardOutput { get; }
    public string? StandardError { get; }
}

internal static class ProcessRunner
{
    public static async Task<ProcessResult> ExecuteAsync(
        Command command,
        CancellationToken cancellationToken = default,
        PipeTarget? standardOutputTarget = null,
        PipeTarget? standardErrorTarget = null)
    {
        var standardOutputBuffer = standardOutputTarget is null ? new StringBuilder() : null;
        var standardErrorBuffer = standardErrorTarget is null ? new StringBuilder() : null;

        var result = await command
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(standardOutputTarget ?? PipeTarget.ToStringBuilder(standardOutputBuffer!))
            .WithStandardErrorPipe(standardErrorTarget ?? PipeTarget.ToStringBuilder(standardErrorBuffer!))
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ProcessResult(result.ExitCode, standardOutputBuffer?.ToString(), standardErrorBuffer?.ToString());
    }
}
