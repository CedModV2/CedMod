using System;
using System.Collections.Generic;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using MapGeneration;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBulletholeDecal))]
    public static class BulletHolePatch
    {
        public static Dictionary<Player, BulletHoleCreator> HoleCreators = new Dictionary<Player, BulletHoleCreator>();
        
        public static void Prefix(StandardHitregBase __instance, Ray ray, RaycastHit hit)
        {
            try
            {
                Player player = Player.Get(__instance.Hub);
                if (player == null)
                    return;
                RoomIdentifier currentRoom = RoomIdUtils.RoomAtPosition(player.Position);
                if (!HoleCreators.ContainsKey(player))
                    HoleCreators.Add(player, new BulletHoleCreator());
                if (!HoleCreators[player].Rooms.ContainsKey(currentRoom))
                    HoleCreators[player].Rooms.Add(currentRoom, new RoomHoles());
           
                HoleCreators[player].Rooms[currentRoom].Holes.Add(hit.point);
            }
            catch (Exception e)
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Error(e.Message);
            }
        }
    }

    public class BulletHoleCreator
    {
        public Dictionary<RoomIdentifier, RoomHoles> Rooms = new Dictionary<RoomIdentifier, RoomHoles>();
    }

    public class RoomHoles
    {
        public List<Vector3> Holes = new List<Vector3>();
    }
}