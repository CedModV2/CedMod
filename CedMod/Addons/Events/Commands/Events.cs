using System;
using CommandSystem;

namespace CedMod.Addons.Events.Commands
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
            RegisterCommand(new Queue());
            RegisterCommand(new BumpEvent());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            
            response = "Please specify a valid subcommand!\n Valid subcommands are: list, enable, disable, queue, bump";
            return false;
        }
    }
}