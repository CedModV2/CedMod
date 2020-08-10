using CommandSystem;
using System;

namespace CedMod.Commands.Dialog
{
    public class DiaglogParentCommand : ParentCommand
    {
        public override string Command => "popupbox";
        public override string[] Aliases { get; } = new string[]
        {
            "dialog"
        };
        public override string Description => "makes a popup appear on a set player(s)";

        public DiaglogParentCommand()
        {
            RegisterCommand(new DialogAllCommand());
            RegisterCommand(new DialogPlayeridCommand());
            RegisterCommand(new DialogUseridCommand());
        }

        public override void LoadGeneratedCommands()
        {
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please specify a valid subcommand!, options are, all, userid, playerid";
            return false;
        }
    }
}