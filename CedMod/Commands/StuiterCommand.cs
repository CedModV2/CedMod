using CommandSystem;
using System;

namespace CedMod.Commands.Stuiter
{
    public class StuiterParentCommand : ParentCommand
    {
        public override string Command => "stuiter";
        public override string[] Aliases { get; }
        public override string Description => "fun stuff";

        public StuiterParentCommand()
        {
            RegisterCommand(new StuiterAllCommand());
            RegisterCommand(new StuiterSpecCommand());
        }

        public override void LoadGeneratedCommands()
        {
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please specify a valid subcommand!";
            return false;
        }
    }
}