// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Repositories;

public interface IOperationOptions 
{
    void ApplyOptionsToInvoke(CliWrapArgumentBuilder argumentBuilder);

    Result<Nothing, IError> Validate();

}