// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Init.Errors;

public sealed class InitExecutionError : InitError
{
    public InitExecutionError()
        : base(Const.Init.Errors.EXECUTION)
    {
    }
}



