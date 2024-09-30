using System;
using System.Threading.Tasks;
using CedMod.Components;
using CommandSystem;
using LabApi.Features.Console;
using Object = UnityEngine.Object;

namespace CedMod.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class InstallUpdateCommand : ICommand
    {
        public string Command { get; } = "installcedmodupdate";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Installs the pending CedMod update (if any) (If an update is pending, the server will be restarted immediately to install the update)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
            if (!CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                Task.Run(async () =>
                {
                    var data = await updater.CheckForUpdates(true);
                    if (data == null)
                        Logger.Error($"There are no updates pending for this version.");
                    else
                    {
                        AutoUpdater.Pending = data;
                        await updater.InstallUpdate();
                    }
                });
                response = "";
                return true;
            }
            if (AutoUpdater.Pending == null)
            {
                response = "There are no updates pending for this server, please run the checkforcedmodupdate command to check for updates. (Updates are also checked every 5 minutes)";
                return false;
            }
            
            Task.Run(async () =>
            {
                var data = await updater.CheckForUpdates(true);
                if (data == null)
                    Logger.Error($"There are no updates pending for this version.");
                else
                {
                    AutoUpdater.Pending = data;
                    await updater.InstallUpdate();
                }
            });
            response = "Update Install requested to the UpdateService";
            return true;
        }
    }
}