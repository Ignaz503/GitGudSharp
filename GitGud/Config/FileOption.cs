// Copyright (c) Ignaz503. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// SPDX-License-Identifier: MIT

#nullable enable

using GitGud.CliArguments;
using GitGud.Constants;
using GitGud.Utility;

namespace GitGud;

public abstract class FileOption
{
    protected string name = Const.Config.FileOptions.LOCAL;
    protected FilePath value = new();

    protected FileOption(string name)
     => this.name = name;

    protected FileOption(string name, FilePath value)
        => (this.name, this.value) = (name, value);

    public virtual string ToArgument()
        => new DoubleDashedArgument(name);

    internal virtual void AddArguments(CliWrapArgumentBuilder arguments)
        => arguments.AddArgument(name.StartsWith("--", StringComparison.Ordinal) ? name : ToArgument(), true);

    public class Local : FileOption
    {
        internal Local() : base(Const.Config.FileOptions.LOCAL) { }
    }

    public class Global : FileOption
    {
        internal Global() : base(Const.Config.FileOptions.GLOBAL) { }
    }

    public class System : FileOption
    {
        internal System() : base(Const.Config.FileOptions.SYSTEM) { }
    }

    public class Worktree : FileOption
    {
        internal Worktree() : base(Const.Config.FileOptions.WORKTREE) { }
    }

    public class File : FileOption
    {
        internal File(FilePath value) : base(Const.Config.FileOptions.FILE, value) { }
        public override string ToArgument()
            => new DoubleDashedArgument<string>(name, value, false);

        internal override void AddArguments(CliWrapArgumentBuilder arguments)
        {
            arguments.AddArgument(name, true);
            arguments.AddArgument(value.ToString(), true);
        }
    }

}
