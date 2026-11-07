// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Runtime.InteropServices;
using System.Security.Principal;

using GitGud.Constants;

namespace GitGud.Utility;

internal static class ElevationHelper
{
    [DllImport(Const.Setup.Elevation.LIBC, EntryPoint = Const.Setup.Elevation.GETEUID)]
    private static extern uint GetEffectiveUserId();

    /// <summary>
    /// Returns false only when the process is known to be not elevated; unknown states are treated as elevated.
    /// </summary>
    public static bool IsElevated()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var identity = WindowsIdentity.GetCurrent();
                return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
            }

            return GetEffectiveUserId() == 0;
        }
        catch (Exception)
        {
            return true;
        }
    }
}
