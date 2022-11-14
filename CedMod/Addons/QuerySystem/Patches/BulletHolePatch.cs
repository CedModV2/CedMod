using System;
using System.Collections.Generic;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
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
                if (!HoleCreators.ContainsKey(player))
                    HoleCreators.Add(player, new BulletHoleCreator());
                if (!HoleCreators[player].Rooms.ContainsKey(player.CurrentRoom))
                    HoleCreators[player].Rooms.Add(player.CurrentRoom, new RoomHoles());
           
                HoleCreators[player].Rooms[player.CurrentRoom].Holes.Add(hit.point);
            }
            catch (Exception e)
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Error(e);
            }
        }
    }

    public class BulletHoleCreator
    {
        public Dictionary<Room, RoomHoles> Rooms = new Dictionary<Room, RoomHoles>();
    }

    public class RoomHoles
    {
        public List<Vector3> Holes = new List<Vector3>();
    }
}