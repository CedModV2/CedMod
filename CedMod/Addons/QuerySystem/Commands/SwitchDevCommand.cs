using System;
using System.Linq;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.Patches;
using CedMod.Addons.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;

namespace CedMod.Addons.QuerySystem.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SwitchDevCommand : ICommand
    {
        public string Command { get; } = "paneldev";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Forces the plugin to connect to the dev panel, this is intended for internal testing";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count >= 1)
                QuerySystem.CurrentMaster = arguments.At(0);
            else
            {
                if (QuerySystem.CurrentMaster == QuerySystem.DevPanelUrl)
                {
                    QuerySystem.CurrentMaster = QuerySystem.MainPanelUrl;
                }
                else
                {
                    QuerySystem.CurrentMaster = QuerySystem.DevPanelUrl;
                }

            }
            
            Task.Factory.StartNew(() =>
            { 
                WebSocketSystem.Stop();
                WebSocketSystem.Start();
            });

            response = $"Plugin will now connect to {QuerySystem.CurrentMaster}";
            return true;
        }
    }
}