using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MiniGames;
using Mirror;
using RemoteAdmin;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class FunnyCommand : ICommand
    {
        public string Command { get; } = "door";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "yes";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.funny"))
            {
                response = "no permission";
                return false;
            }

            foreach(var light in FlickerableLightController.Instances)
            {
                light.gameObject.AddComponent<RainbowLight>();
            }
            
            response = "Done";
            return true;

            foreach (var VARIABLE in Object.FindObjectsOfType<DoorSpawnpoint>())
            {
                Log.Error(VARIABLE.TargetPrefab.name.ToUpper());
            }
            
            var LczDoorObj = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.ToUpper().Contains("LCZ")).TargetPrefab.gameObject;
            var HczDoorObj = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.ToUpper().Contains("HCZ")).TargetPrefab.gameObject;
            var EzDoorObj = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.ToUpper().Contains("EZ")).TargetPrefab.gameObject;
            
            Player player = Player.Get(sender as PlayerCommandSender);
            GameObject gameObject = Object.Instantiate(HczDoorObj, player.Position, Quaternion.identity);
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            
            NetworkServer.Spawn(gameObject);

            DoorVariant doorVaraintComponent = gameObject.GetComponent<DoorVariant>();
            Door door = Door.Get(doorVaraintComponent);
            door.TryPryOpen();
            
            response = "hehe xd";
            return true;
        }
    }
}