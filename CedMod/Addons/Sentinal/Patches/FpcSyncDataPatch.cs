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
using PluginAPI.Core;
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
                
                if (hub.roleManager.CurrentRole is Scp939Role scp939Role && scp939Role.SubroutineModule.TryGetSubroutine(out Scp939LungeAbility lungeAbility) && scp939Role.SubroutineModule.TryGetSubroutine(out Scp939FocusAbility focusAbility))
                {
                    //lunge prepare
                    if (lungeAbility._movementModule.Motor.Speed == 0)
                    {
                        if (!AuthorizedLunge.TryGetValue(hub, out Stopwatch stopwatch) || stopwatch.Elapsed.TotalSeconds >= 5)
                        {
                            // WebSocketSystem.Enqueue(new QueryCommand()
                            // {
                            //     Recipient = "PANEL",
                            //     Data = new Dictionary<string, string>()
                            //     {
                            //         {"SentinalType", "SCP939LungeExploit"},
                            //         {"UserId", hub.authManager.UserId},
                            //         {"Pos", __instance._position.Position.ToString()}
                            //     }
                            // });
                            //Log.Warning($"SCP939 is trying to walk while lunging, this is likely a cheater {lungeAbility.State} {lungeAbility._movementModule.Motor.Speed}");
                            return false;
                        }
                    }
                    
                    if (lungeAbility.State == Scp939LungeState.None)
                    {
                        if (!AuthorizedLunge.TryGetValue(hub, out Stopwatch stopwatch))
                            AuthorizedLunge[hub] = new Stopwatch();
                        AuthorizedLunge[hub].Restart();
                    }
                }
                
                SyncDatas[hub] = __instance;
                if (WebSocketSystem.HelloMessage == null || !WebSocketSystem.HelloMessage.SentinalPositions)
                    return true;
                
                SentinalBehaviour.DirtyPositions.Enqueue((hub, hub.transform.eulerAngles, hub.transform.position, SentinalBehaviour.UFrames));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            return true;
        }
    }
}