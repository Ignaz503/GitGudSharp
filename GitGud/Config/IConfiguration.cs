// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;

using GitGud.CliArguments;
using GitGud.Config;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud;
public record class ConfigListQueryParameters(FileOption FileOption, bool DoIncludes = false, RegexPattern? RegexPattern = null);

public record class ConfigGetQueryParameters(FileOption FileOption, bool DoIncludes = false, RegexPattern? RegexPattern = null, bool DoAllValues = true, ValuePattern? ValuePattern = null, Url? Url = null, string? Default = null);

public record class ConfigSetQueryParameters(FileOption FileOption, bool DoAllValues = true, ValuePattern? ValuePattern = null);

public record class ConfigUnsetQueryParameters(FileOption FileOption, bool DoAllValues = true, ValuePattern? ValuePattern = null);

public record class RenameSectionQueryParameters(FileOption FileOption);

public record class RemoveSectionQueryParameters(FileOption FileOption);

public interface IConfiguration
{
    /*
        skip for now.
        git config [<file-option>] --get-colorbool <name> [<stdout-is-tty>]
     */

    Task<Result<List<ConfigEntry>, IError>> ListAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ScopedConfigEntry>, IError>> ListScopedAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ConfigEntryNameOnly>, IError>> ListNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ScopedConfigEntryNameOnly>, IError>> ListScopedNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<OriginConfigEntry>, IError>> ListOriginAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ScopedOriginConfigEntry>, IError>> ListScopedOriginAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<OriginConfigEntryNameOnly>, IError>> ListOriginNameOnlyAsync(ConfigListQueryParameters query , CancellationToken cancellationToken = default);
    Task<Result<List<ScopedOriginConfigEntryNameOnly>, IError>> ListScopedOriginNameOnlyAsync(ConfigListQueryParameters query, CancellationToken cancellationToken = default);


    Task<Result<List<ConfigEntry>, IError>> GetAsync( ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ScopedConfigEntry>, IError>> GetScopedAsync(ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ConfigEntryNameOnly>, IError>> GetNameOnlyAsync(ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ScopedConfigEntryNameOnly>, IError>> GetScopedNameOnlyAsync(ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<OriginConfigEntry>, IError>> GetOriginAsync(ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<ScopedOriginConfigEntry>, IError>> GetScopedOriginAsync(ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);
    Task<Result<List<OriginConfigEntryNameOnly>, IError>> GetOriginNameOnlyAsync(ConfigKey name,  ConfigGetQueryParameters query , CancellationToken cancellationToken = default);
    Task<Result<List<ScopedOriginConfigEntryNameOnly>, IError>> GetScopedOriginNameOnlyAsync(ConfigKey name,  ConfigGetQueryParameters query, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> SetAsync(ConfigKey name, ConfigValue value, ConfigSetQueryParameters query, CancellationToken cancellationToken = default);
 
    Task<Result<Nothing, IError>> UnsetAsync(ConfigKey name, ConfigUnsetQueryParameters query, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> RenameSectionAsync(Section oldName, Section newName, RenameSectionQueryParameters query, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> RemoveSection(Section toRemove, RenameSectionQueryParameters query, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> EditAsync(FileOption fileOption, CancellationToken cancellationToken = default);

    Task<Result<Nothing, IError>> Apply(IConfigCollection configCollection, CancellationToken cancelationToken = default);

}
