using System;
using CedMod.Commands.Dialog;
using CommandSystem;

namespace CedMod.Commands
{
    /// <summary>
    /// <see cref="Description"/>.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class DiaglogParentCommand : ParentCommand
    {
        public override string Command => "popupbox";
        public override string[] Aliases { get; } = new string[]
        {
            "dialog"
        };
        public override string Description => "makes a popup appear on a set player(s)";
        
        public DiaglogParentCommand() => LoadGeneratedCommands();
        
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new DialogAllCommand());
            RegisterCommand(new DialogUseridCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please specify a valid subcommand!, options are, all, userid, playerid";
            return false;
        }
    }
}