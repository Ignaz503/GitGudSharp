// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;


namespace GitGud.CliArguments;

public interface ICliParameters 
{
    Result<Nothing, IError> AddCliArguments<T>(T argBuilder)
        where T : ICliArgumentBuilder;
}



