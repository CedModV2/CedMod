using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(AutomaticAction), nameof(AutomaticAction.ServerAuthorizeShot))]
    public static class AutomaticFirearmPatch
    {
        public static List<int> FiredSerials = new List<int>();

        public static bool Prefix(AutomaticAction __instance)
        {
            if (FiredSerials.Contains(__instance._firearm.ItemSerial))
            {
                CedModPlayer plr = CedModPlayer.Get(__instance._firearm.Owner);
                Log.Info($"Rejected shot from {plr.Nickname} ({plr.UserId}) for firing in the same frame");

                if (!SentinalBehaviour.Ids.Contains(__instance._firearm.Owner.authManager.UserId))
                {
                    SentinalBehaviour.Ids.Add(__instance._firearm.Owner.authManager.UserId);
                    Task.Run(async () =>
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                            await VerificationChallenge.AwaitVerification();
                            try
                            {
                                var response = await client.GetAsync($"http{(QuerySystem.QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.QuerySystem.CurrentMaster}/Api/Sentinal/Report?key={QuerySystem.QuerySystem.QuerySystemKey}&userid={plr.UserId}&data=FireSameFrame&type=VCExploit");
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug(await response.Content.ReadAsStringAsync());
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.ToString());
                            }
                        }
                    });
                }
                return false;
            }
            
            FiredSerials.Add(__instance._firearm.ItemSerial);
            return true;
        }
    }
}