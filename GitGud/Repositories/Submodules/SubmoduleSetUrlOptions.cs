// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;

using SubConst = GitGud.Constants.Const.Repository.Submodules;

namespace GitGud.Repositories.Submodules;

/// <summary>git submodule set-url</summary>
public sealed class SubmoduleSetUrlOptions : SubmoduleOptionsBase
{
    const int PathIndex = 1;
    const int UrlIndex = 2;

    public SubmoduleSetUrlOptions() : base(3) { }

    private protected override string SubCommand => SubConst.SET_URL;

    /// <summary>Submodule path (required).</summary>
    public string? Path
    {
        get => GetValue(PathIndex);
        init => SetValue(PathIndex, value);
    }

    /// <summary>New url (required).</summary>
    public string? Url
    {
        get => GetValue(UrlIndex);
        init => SetValue(UrlIndex, value);
    }

    private protected override void ApplyBody(CliWrapArgumentBuilder argumentBuilder)
    {
        argumentBuilder.AddArgument(SubConst.PATH_SEPARATOR, true);
        AddRange(argumentBuilder, PathIndex, arguments.Length);
    }

    private protected override string? ValidateBody()
    {
        if (!arguments[PathIndex].HasValue || IsInvalid(Path))
            return SubConst.Errors.PATH_INVALID;
        if (!arguments[UrlIndex].HasValue || IsInvalid(Url))
            return SubConst.Errors.URL_INVALID;

        return null;
    }
}
