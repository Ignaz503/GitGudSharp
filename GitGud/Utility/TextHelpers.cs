// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GitGud.Utility;

public static class TextHelpers
{

    public static bool IsAsciiLetter(this char value)
        => value is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    public static bool IsAsciiDigit(this char value)
        => value is >= '0' and <= '9';

}
