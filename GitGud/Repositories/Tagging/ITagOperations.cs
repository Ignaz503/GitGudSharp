// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Erorrs;
using GitGud.Utility;

using System.Threading;

namespace GitGud.Repositories.Tagging;

public interface ITagOperations
{
    Task<Result<IReadOnlyList<TagInfo>, IError>> ListTags(ListTagsOptions? options = null, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> CreateTag(CreateTagOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> DeleteTag(DeleteTagOptions options, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> VerifyTag(VerifyTagOptions options, CancellationToken cancellationToken = default);
}
