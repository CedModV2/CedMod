using System;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using CommandSystem;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif

namespace CedMod.Addons.QuerySystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RestartQueryCommand : ICommand
    {
        public string Command { get; } = "restartqueryserver";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "restarts querysystem";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.restartquery"))
            {
                response = "No permission";
                return false;
            }

            Task.Factory.StartNew(async () =>
            { 
                WebSocketSystem.Stop();
                await WebSocketSystem.Start();
            });
            response = "Query server restarted";
            return true;
        }
    }
}