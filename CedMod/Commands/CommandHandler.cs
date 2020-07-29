using CedMod.Commands;
using CedMod.Commands.Stuiter;
using CedMod.INIT;
using MEC;
using RemoteAdmin;


namespace CedMod
{
    public class CedModCommandHandler
    {
        public static void RegisterRACommands()
        {
            Initializer.Logger.Info("INIT", "Registering RA commands...");
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new StuiterParentCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new LightsoutCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new FFADisableCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new PlayersCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new TotalBansCommand());
        }
        public static void RegisterConsoleCommands()
        {
            Initializer.Logger.Info("INIT", "Registering Console commands...");
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new StuiterParentCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new LightsoutCommand());
        }

        public static void RegisterClientConsoleCommands()
        {
            Initializer.Logger.Info("INIT", "Registering Client Console commands...");
            QueryProcessor.DotCommandHandler.RegisterCommand(new TotalBansCommand());
        }
        
        public void OnRoundEnd()
        {
            Timing.KillCoroutines("LightsOut");
        }
    }
}