using CedMod.Commands;
using CedMod.Commands.Stuiter;
using CedMod.Commands.Dialog;
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
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new PriorBansCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new RedirectCommand());
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(new DiaglogParentCommand());
        }
        public static void RegisterConsoleCommands()
        {
            Initializer.Logger.Info("INIT", "Registering Console commands...");
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new StuiterParentCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new LightsoutCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new RedirectCommand());
            GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(new DiaglogParentCommand());
        }

        public static void RegisterClientConsoleCommands()
        {
            Initializer.Logger.Info("INIT", "Registering Client Console commands...");
            QueryProcessor.DotCommandHandler.RegisterCommand(new TotalBansCommand());
            QueryProcessor.DotCommandHandler.RegisterCommand(new PriorBansCommand());
        }
        
        public void OnRoundEnd()
        {
            Timing.KillCoroutines("LightsOut");
        }
    }
}