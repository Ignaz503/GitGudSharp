// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public static partial class Init 
    {
        public static class Errors 
        {
            public const string OPTIONS_REQUIRED = "Init options cannot be null.";
            public const string TARGET_PATH_REQUIRED = "A target path is required.";
            public const string TARGET_PATH_INVALID = "The target path is invalid.";
            public const string FAILED = "Git failed to initialize the repository.";
            public const string EXECUTION = "Git initialization could not be executed.";
            public const string OBJECT_FORMAT_INVALID = "The object format is invalid.";
            public const string REF_FORMAT_INVALID = "The reference storage format is invalid.";
        }

    }
}



