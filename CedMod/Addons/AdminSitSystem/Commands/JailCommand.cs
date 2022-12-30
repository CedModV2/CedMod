using System;
using CedMod.Addons.AdminSitSystem.Commands.Jail;
using CommandSystem;

namespace CedMod.Addons.AdminSitSystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class JailParentCommand : ParentCommand
    {
        public override string Command => "jail";
        public override string[] Aliases { get; } = new string[]
        {
            "adminsit"
        };
        public override string Description => "Manages the jail system.";
        
        public JailParentCommand() => LoadGeneratedCommands();
        
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Create());
            RegisterCommand(new Remove());
            RegisterCommand(new Add());
            RegisterCommand(new Join());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "jail -\ncreate - creates and assigns a jail location to you for use.\njail add {playerid} - adds a player to the jail\njail remove {playerid} - removes a player from the jail";
            return false;
        }
    }
}