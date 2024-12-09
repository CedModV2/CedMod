using System;
using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Visibility;
using PluginAPI.Core;
namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(FpcSyncData), nameof(FpcSyncData.TryApply))] //todo implement when there is a need for it
    public static class FpcSyncDataPatch
    {
        public static Dictionary<ReferenceHub, FpcSyncData> SyncDatas = new Dictionary<ReferenceHub, FpcSyncData>();
        
        public static void Prefix(FpcSyncData __instance, ReferenceHub hub, ref FirstPersonMovementModule module, ref bool bit)
        {
            try
            {
                if (WebSocketSystem.HelloMessage == null || !WebSocketSystem.HelloMessage.SentinalPositions)
                    return;
                
                if (SyncDatas.TryGetValue(hub, out FpcSyncData data) && __instance == data)
                    return;
                SyncDatas[hub] = __instance;
                
                SentinalBehaviour.DirtyPositions.Enqueue((hub, __instance, SentinalBehaviour.UFrames));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}