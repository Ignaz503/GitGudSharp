// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Setup.Errors;

public sealed class SetupContextError : SetupError
{
    public SetupContextError()
        : base(Const.Setup.Operations.GIT_SETUP, Const.Setup.Errors.OPERATION_CONTEXT_REQUIRED)
    {
    }
}



