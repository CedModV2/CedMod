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
                if (hub.roleManager.CurrentRole is Scp939Role scp939Role && scp939Role.SubroutineModule.TryGetSubroutine(out Scp939LungeAbility lungeAbility) && scp939Role.SubroutineModule.TryGetSubroutine(out Scp939FocusAbility focusAbility))
                {
                    //lunge prepare
                    if (lungeAbility._movementModule.Motor.Speed == 0)
                    {
                        if (!AuthorizedLunge.TryGetValue(hub, out Stopwatch stopwatch) || stopwatch.Elapsed.TotalSeconds >= 7)
                        {
                            WebSocketSystem.Enqueue(new QueryCommand()
                            {
                                Recipient = "PANEL",
                                Data = new Dictionary<string, string>()
                                {
                                    {"SentinalType", "SCP939LungeExploit"},
                                    {"UserId", hub.authManager.UserId},
                                    {"Pos", __instance._position.Position.ToString()}
                                }
                            });
                            Log.Warning($"SCP939 is trying to walk while lunging, this is likely a cheater");
                            return false;
                        }
                    }
                    
                    if (lungeAbility.State != Scp939LungeState.None && !lungeAbility._movementModule.Motor.IsJumping)
                    {
                        if (!AuthorizedLunge.TryGetValue(hub, out Stopwatch stopwatch) || stopwatch.Elapsed.TotalSeconds >= 7)
                        {
                            WebSocketSystem.Enqueue(new QueryCommand()
                            {
                                Recipient = "PANEL",
                                Data = new Dictionary<string, string>()
                                {
                                    {"SentinalType", "SCP939LungeExploit"},
                                    {"UserId", hub.authManager.UserId},
                                    {"Pos", __instance._position.Position.ToString()}
                                }
                            });
                            Log.Warning($"SCP939 is trying to walk while lunging, this is likely a cheater");
                            return false;
                        }
                    }
                    else if (!AuthorizedLunge.ContainsKey(hub))
                    {
                        AuthorizedLunge[hub] = Stopwatch.StartNew();
                    }

                    if (AuthorizedLunge.TryGetValue(hub, out Stopwatch authorizedLunge) && authorizedLunge.Elapsed.TotalSeconds > 7)
                        AuthorizedLunge.Remove(hub);
                }
                
                if (WebSocketSystem.HelloMessage == null || !WebSocketSystem.HelloMessage.SentinalPositions)
                    return true;
                
                if (SyncDatas.TryGetValue(hub, out FpcSyncData data) && __instance == data)
                    return true;
                SyncDatas[hub] = __instance;
                
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