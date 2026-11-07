// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Constants;

public static partial class Const
{
    public const string TAG = "tag";

    public static partial class Repository
    {
        public const string GIT_TAG = $"{GIT} {TAG}";

        public static class Tagging
        {
            public const string ANNOTATE = "-a";
            public const string SIGN = "-s";
            public const string LOCAL_USER = "-u";
            public const string FORCE = "-f";
            public const string DELETE = "-d";
            public const string VERIFY = "-v";
            public const string LIST = "-l";
            public const string MESSAGE = "-m";
            public const string FILE = "-F";
            public const string IGNORE_CASE = "-i";
            public const string TRAILER = "trailer";
            public const string CLEANUP = "cleanup";
            public const string CLEANUP_VERBATIM = "verbatim";
            public const string CLEANUP_WHITESPACE = "whitespace";
            public const string CLEANUP_STRIP = "strip";
            public const string CREATE_REFLOG = "create-reflog";
            public const string FORMAT = "format";
            public const string SORT = "sort";
            public const string CONTAINS = "contains";
            public const string NO_CONTAINS = "no-contains";
            public const string MERGED = "merged";
            public const string NO_MERGED = "no-merged";
            public const string POINTS_AT = "points-at";
            public const string LIST_FORMAT = "%(refname:strip=2)%09%(objecttype)%09%(objectname)%09%(*objectname)%09%(contents:subject)";
            public const string OBJECT_TYPE_TAG = "tag";
            public const char FIELD_SEPARATOR = '\t';
            public const char LINE_SEPARATOR = '\n';
            public const int FIELD_COUNT = 5;

            public static class Errors
            {
                public const string LIST_FAILED = "Failed to list tags.";
                public const string CREATE_FAILED = "Failed to create tag.";
                public const string DELETE_FAILED = "Failed to delete tags.";
                public const string VERIFY_FAILED = "Failed to verify tags.";
                public const string NAME_INVALID = "Tag name is invalid.";
                public const string NAMES_REQUIRED = "At least one tag is required.";
                public const string TARGET_INVALID = "Tag target is invalid.";
                public const string MESSAGE_REQUIRED = "Annotated or signed tags require a message, message file or trailers.";
                public const string MESSAGE_CONFLICT = "Message and message file cannot both be specified.";
                public const string SIGN_CONFLICT = "Sign and signing key cannot both be specified.";
                public const string VALUE_INVALID = "A tag option value is invalid.";
            }
        }
    }
}
