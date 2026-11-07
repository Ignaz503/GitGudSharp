// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Setup.Errors;

public sealed class SetupExecutionError : SetupError
{
    public SetupExecutionError(string operation)
        : base(operation, Const.Setup.Errors.SETUP_EXECUTION)
    {
    }
}



