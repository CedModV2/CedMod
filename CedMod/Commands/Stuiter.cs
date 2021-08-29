using System;
using CedMod.Commands.Stuiter;
using CommandSystem;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StuiterParentCommand : ParentCommand
    {
        public override string Command => "stuiter";

        public override string[] Aliases { get; } = new string[]
        {
        };
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