using CedMod.INIT;
using CedMod.Commands.Stuiter;
using MEC;
using RemoteAdmin;

namespace CedMod.Commands
{
    public class CedModCommandHandler
    {
        public static void RegisterRACommands()
        {
            Initializer.Logger.Info("INIT", "Registering RA commands...");
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new StuiterParentCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new LightsoutCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new FFADisableCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new AirstrikeCommand());
        }
        public static void RegisterConsoleCommands()
        {
            Initializer.Logger.Info("INIT", "Registering Console commands...");
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new StuiterParentCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new LightsoutCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new FFADisableCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new AirstrikeCommand());
        }

        public static void RegisterClientConsoleCommands()
        {
            Initializer.Logger.Info("INIT", "Registering Client Console commands...");
        }
        
        public void OnRoundEnd()
        {
            Timing.KillCoroutines("LightsOut");
        }
    }
}