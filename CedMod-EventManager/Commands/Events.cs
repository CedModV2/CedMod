using System;
using System.Linq;
using CedMod.Commands.Stuiter;
using CommandSystem;

namespace CedMod.EventManager.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class EventsCommand : ParentCommand
    {
        public override string Command => "events";

        public override string[] Aliases { get; } = new string[]
        {
        };
        public override string Description => "Main command for CedMod EventSystem";

        public EventsCommand() => LoadGeneratedCommands();
        
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new ListEvents());
            RegisterCommand(new EnableEvent());
            RegisterCommand(new DisableEvent());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            
            response = "Please specify a valid subcommand!\n Valid subcommands are: list, enable, disable";
            return false;
        }
    }
}