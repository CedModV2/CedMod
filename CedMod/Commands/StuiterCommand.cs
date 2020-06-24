using System;
using CommandSystem;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StuiterCommand : ParentCommand
    {
        public override string Command { get; } = "stuiter";
        public override string[] Aliases { get; }
        public override string Description { get; } = "Does fun stuff";
        public static StuiterCommand Create()
        {
            StuiterCommand StuiterCommand = new StuiterCommand();
            StuiterCommand.LoadGeneratedCommands();
            return StuiterCommand;
        }
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            response = "Please specify a valid subcommand";
            return false;
        }

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Stuiter.AllCommand());
            RegisterCommand(new Stuiter.SpecCommand());
        }
    }
}