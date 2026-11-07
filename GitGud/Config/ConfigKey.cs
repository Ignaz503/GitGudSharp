// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.Config.Errors;
using GitGud.Erorrs;
using GitGud.Utility;

namespace GitGud.Config;

public readonly struct ConfigKey : IEquatable<ConfigKey>, IEquatable<string>
{
    private readonly string? _value;

    private ConfigKey(string value)
        => _value = value;

    public string Value => _value ?? string.Empty;
    public bool IsValid => IsValidName(Value);

    public static Result<ConfigKey, IError> Create(string? value)
        => IsValidName(value)
            ? Result<ConfigKey, IError>.Some(new ConfigKey(value!))
            : Result<ConfigKey, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidKey));

    public static Result<ConfigKey, IError> CreateSubsection(string? section, string? subsection, string? name)
        => IsValidSection(section) && IsValidSubsection(subsection) && IsValidVariableName(name)
            ? Result<ConfigKey, IError>.Some(new ConfigKey(string.Concat(section, ".", subsection, ".", name)))
            : Result<ConfigKey, IError>.None(new ConfigValidationError(ConfigValidationCode.InvalidKey));

    public bool Equals(ConfigKey other)
        => StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);

    public bool Equals(string? other)
        => StringComparer.OrdinalIgnoreCase.Equals(Value, other);

    public override bool Equals(object? obj)
        => obj is ConfigKey other && Equals(other);

    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString()
        => Value;

    public static bool operator ==(ConfigKey left, ConfigKey right) => left.Equals(right);

    public static bool operator !=(ConfigKey left, ConfigKey right) => !left.Equals(right);

    private static bool IsValidName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var separator = value!.LastIndexOf('.');
        if (separator <= 0 || separator == value.Length - 1)
            return false;

        var segmentStart = 0;
        for (var index = 0; index <= separator; index++)
        {
            if (index != separator && value[index] != '.')
                continue;

            if (index == segmentStart || !value[segmentStart].IsAsciiLetter())
                return false;

            for (var segmentIndex = segmentStart; segmentIndex < index; segmentIndex++)
            {
                var character = value[segmentIndex];
                if (!character.IsAsciiLetter() && !character.IsAsciiDigit() && character != '-')
                    return false;
            }

            segmentStart = index + 1;
        }

        if (!value[separator + 1].IsAsciiLetter())
            return false;

        for (var index = separator + 1; index < value.Length; index++)
        {
            var character = value[index];
            if (!character.IsAsciiLetter() && !character.IsAsciiDigit() && character != '-')
                return false;
        }

        return true;
    }

    private static bool IsValidSection(string? section)
    {
        if (string.IsNullOrEmpty(section) || !section![0].IsAsciiLetter())
            return false;

        for (var index = 0; index < section.Length; index++)
        {
            var character = section[index];
            if (!character.IsAsciiLetter() && !character.IsAsciiDigit() && character != '-' && character != '.')
                return false;
        }

        return true;
    }

    private static bool IsValidSubsection(string? subsection)
        => !string.IsNullOrEmpty(subsection) && subsection!.IndexOfAny(new[] { '\0', '\r', '\n' }) < 0;

    private static bool IsValidVariableName(string? name)
    {
        if (string.IsNullOrEmpty(name) || !name![0].IsAsciiLetter())
            return false;

        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (!character.IsAsciiLetter() && !character.IsAsciiDigit() && character != '-')
                return false;
        }

        return name[name.Length - 1] != '.' && !name.Contains("..");
    }

}
