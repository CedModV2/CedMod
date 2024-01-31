using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.Sentinal.Patches;
using Newtonsoft.Json;
using PluginAPI.Core;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CedMod.Addons.Sentinal
{
    public class SentinalBehaviour: MonoBehaviour
    {
        public List<string> Ids = new List<string>();
        public float Time = 120;

        public void FixedUpdate()
        {
            Time -= UnityEngine.Time.fixedDeltaTime;

            if (Time <= 0)
            {
                Ids.Clear();
                Time = 120;
            }
            
            foreach (var pack in VoicePacketPacket.PacketsSent)
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Info($"Audioguard {pack.Key} val {pack.Value}");
                int amount = pack.Value;
                if (amount >= 30)
                {
                    var plr = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == pack.Key, null);
                    if (plr != null && !Ids.Contains(plr.authManager.UserId))
                    {
                        Ids.Add(plr.authManager.UserId);
                        Log.Info($"CedMod Reporting {plr.nicknameSync.MyNick} {plr.authManager.UserId}");
                        Task.Run(async () =>
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                                await VerificationChallenge.AwaitVerification();
                                try
                                {
                                    var response = await client.GetAsync($"http{(QuerySystem.QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.QuerySystem.CurrentMaster}/Api/Sentinal/ReportVC?key={QuerySystem.QuerySystem.QuerySystemKey}&userid={plr.authManager.UserId}&amount={amount}");
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
                }
            }
            
            VoicePacketPacket.PacketsSent.Clear();
        }
    }
}