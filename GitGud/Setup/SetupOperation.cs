// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


using GitGud.Constants;

namespace GitGud.Setup;

enum SetupOperation
{
    Install,
    Uninstall,
    Update
}



static class SetupOperationExtensions
{
    public static string GetOperationName(this SetupOperation operation)
        => operation switch
        {
            SetupOperation.Install => Const.Setup.Operations.GIT_INSTALLATION,
            SetupOperation.Uninstall => Const.Setup.Operations.GIT_UNINSTALLATION,
            SetupOperation.Update => Const.Setup.Operations.GIT_UPDATE,
            _ => Const.Setup.Operations.GIT_SETUP
        };
}