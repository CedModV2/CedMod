using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
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
    public class DoorCommand : ICommand, IUsageProvider
    {
        public static Dictionary<string, Door> Doors = new Dictionary<string, Door>();
        public string Command { get; } = "door";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string[] Usage { get; } = new string[]
        {
            "HCZ/LCZ/EZ",
            "LOCKED",
            "NAME",
            "DESTROYABLE",
            "ALLOWSCP106"
        };

        public string Description { get; } = "spawning doors";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "no";
            return true;
            if (!sender.CheckPermission("cedmod.door"))
            {
                response = "no permission";
                return false;
            }

            foreach (var VARIABLE in Object.FindObjectsOfType<DoorSpawnpoint>())
            {
                Log.Error(VARIABLE.TargetPrefab.name.ToUpper());
            }
            
            if (arguments.Count < 3)
            {
                response = "door [HCZ/LCZ/EZ] [LOCKED] [NAME] [DESTROYABLE] [ALLOWSCP106]";
                return false;
            }
            
            string type = arguments.At(0);
            bool locked;
            string name = arguments.At(2);
            bool destroyable;
            bool allowlarry;
            if (!bool.TryParse(arguments.At(1), out locked) || !bool.TryParse(arguments.At(3), out destroyable) || !bool.TryParse(arguments.At(4), out allowlarry))
            {
                response = "door [HCZ/LCZ/EZ] [LOCKED] [NAME] [DESTROYABLE] [ALLOWSCP106]";
                return false;
            }

            var LczDoorObj = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.ToUpper().Contains("LCZ")).TargetPrefab.gameObject;
            var HczDoorObj = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.ToUpper().Contains("HCZ")).TargetPrefab.gameObject;
            var EzDoorObj = Object.FindObjectsOfType<DoorSpawnpoint>().First(x => x.TargetPrefab.name.ToUpper().Contains("EZ")).TargetPrefab.gameObject;
            
            Player player = Player.Get(sender as PlayerCommandSender);
            GameObject gameObject = null;

            switch (type.ToUpper())
            {
                case "HCZ":
                    gameObject = Object.Instantiate(HczDoorObj, player.Position - new Vector3(0, -1, 0), Quaternion.Euler(player.Rotations));
                    break;
                case "LCZ":
                    gameObject = Object.Instantiate(LczDoorObj, player.Position - new Vector3(0, -1, 0), Quaternion.Euler(player.Rotations));
                    break;
                case "EZ":
                    gameObject = Object.Instantiate(EzDoorObj, player.Position - new Vector3(0, -1, 0), Quaternion.Euler(player.Rotations));
                    break;
            }
            
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            NetworkServer.Spawn(gameObject);

            DoorVariant doorVaraintComponent = gameObject.GetComponent<DoorVariant>();
            Door door = Door.Get(doorVaraintComponent);
            if (locked)
                door.DoorLockType = DoorLockType.SpecialDoorFeature;
            if (destroyable)
                door.IgnoredDamageTypes = DoorDamageType.Grenade | DoorDamageType.Scp096 | DoorDamageType.Weapon | DoorDamageType.ServerCommand;
            Doors.Add(name, door);
            door.AllowsScp106 = allowlarry;
            response = "hehe xd";
            return true;
        }
    }
}