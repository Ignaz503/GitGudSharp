// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable


namespace GitGud.Repositories;

/// <summary>
/// Identifies a submodule (by its path or pathspec relative to the repository root) to initialize on clone.
/// </summary>
public readonly struct SubmoduleSpec : IEquatable<SubmoduleSpec>
{
    private readonly string? _path;

    public SubmoduleSpec(string path)
        => _path = path;

    public string Path => _path ?? string.Empty;

    public bool IsValid => !string.IsNullOrWhiteSpace(_path);

    public override string ToString()
        => Path;

    public override int GetHashCode()
        => Path.GetHashCode();

    public override bool Equals(object? obj)
        => obj is SubmoduleSpec other && Equals(other);

    public bool Equals(SubmoduleSpec other)
        => Path == other.Path;

    public static implicit operator SubmoduleSpec(string path) => new SubmoduleSpec(path);

    public static bool operator ==(SubmoduleSpec left, SubmoduleSpec right) => left.Equals(right);

    public static bool operator !=(SubmoduleSpec left, SubmoduleSpec right) => !left.Equals(right);
}
