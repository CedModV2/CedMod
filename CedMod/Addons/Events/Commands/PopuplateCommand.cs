using System;
using System.Linq;
using CedMod.Addons.Audio;
using CedMod.Addons.QuerySystem;
using CommandSystem;
using MEC;
using Mirror;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using PluginAPI.Core;
using Object = UnityEngine.Object;

namespace CedMod.Addons.Events.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Populate : ICommand, IUsageProvider
    {
        public string Command { get; } = "populate";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "spawns an given amount of dummy players for testing purposes.";

        public string[] Usage { get; } = new string[]
        {
            "%amount%",
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.populate"))
            {
                response = "no permission";
                return false;
            }
            if (arguments.Count < 1)
            {
                response = "To execute this command provide at least 1 argument!\nUsage: " + this.DisplayCommandUsage();
                return false;
            }
            int amount = Convert.ToInt16(arguments.At(0));

            int id = 0;
            while (id < amount)
            {
                id++;
                var newPlayer = Object.Instantiate(NetworkManager.singleton.playerPrefab);

                var fakeConnection = new FakeConnection(id);


                var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
                AudioCommand.FakeConnections.Add(fakeConnection, hubPlayer);
                AudioCommand.FakeConnectionsIds.Add(id, hubPlayer);

                NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
                try
                {
                    hubPlayer.characterClassManager.UserId = $"player{id}@server";
                }
                catch (Exception e)
                {
                }
                hubPlayer.characterClassManager.InstanceMode = ClientInstanceMode.Host;
                try
                {
                    hubPlayer.nicknameSync.SetNick($"Dummy player {id}");
                }
                catch (Exception e)
                {
                }
            }
            response = "Success";
            return true;
        }
    }
}