using System;
using System.Collections.Generic;
using System.Diagnostics;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Visibility;
namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(FpcSyncData), nameof(FpcSyncData.TryApply))] //todo implement when there is a need for it
    public static class FpcSyncDataPatch
    {
        public static Dictionary<ReferenceHub, FpcSyncData> SyncDatas = new Dictionary<ReferenceHub, FpcSyncData>();
        public static Dictionary<ReferenceHub, Stopwatch> AuthorizedLunge = new Dictionary<ReferenceHub, Stopwatch>();
        
        public static bool Prefix(FpcSyncData __instance, ReferenceHub hub, ref FirstPersonMovementModule module, ref bool bit)
        {
            try
            {
                if (SyncDatas.TryGetValue(hub, out FpcSyncData data) && __instance == data)
                    return true;
                
                if (AuthorizedLunge.TryGetValue(hub, out Stopwatch authorizedLunge) && authorizedLunge.Elapsed.TotalSeconds > 5)
                    AuthorizedLunge.Remove(hub);
                
                SyncDatas[hub] = __instance;
                if (WebSocketSystem.HelloMessage == null || !WebSocketSystem.HelloMessage.SentinalPositions)
                    return true;
                
                SentinalBehaviour.DirtyPositions.Enqueue((hub, hub.transform.eulerAngles, hub.transform.position, SentinalBehaviour.UFrames));
            }
            catch (Exception e)
            {
                LabApi.Features.Console.Logger.Error(e.ToString());
            }

            return true;
        }
    }
}