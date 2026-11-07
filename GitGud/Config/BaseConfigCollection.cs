// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Threading;

using GitGud.Config;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud;

public abstract class BaseConfigCollection : IConfigCollection
{
    protected List<(ConfigKey, ConfigValue, ConfigSetQueryParameters)> _values = new(); 

    public async Task<Result<Nothing,IError>> Apply(IConfiguration applyTo, CancellationToken cancellationToken = default)
    {
        foreach(var (key, value, @param) in _values)
        {
            var res = await applyTo.SetAsync(key, value,@param, cancellationToken: cancellationToken);
            if(res.HasNone)
                return Result<Nothing, IError>.None(res.Handle());
        }
        return Result<Nothing, IError>.Some(new());
    }
}