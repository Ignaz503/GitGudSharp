// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using CliWrap.Builders;


namespace GitGud.CliArguments;

public struct CliWrapArgumentBuilder :  ICliArgumentBuilder
{
    private readonly ArgumentsBuilder _internalBuilder;

    public CliWrapArgumentBuilder(ArgumentsBuilder internalBuilder)
    {
        _internalBuilder = internalBuilder;
    }

    public ICliArgumentBuilder AddArgument(string argument, bool escaped)
    {
        _internalBuilder.Add(argument, escaped);
        return this;
    }
}



