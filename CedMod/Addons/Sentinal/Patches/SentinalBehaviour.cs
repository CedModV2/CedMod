using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginAPI.Core;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CedMod.Addons.Sentinal.Patches
{
    public class SentinalBehaviour: MonoBehaviour
    {
        public List<string> Ids = new List<string>();
        public float Time = 30;

        public void Update()
        {
            Time -= UnityEngine.Time.deltaTime;

            if (Time <= 0)
            {
                Ids.Clear();
                Time = 30;
            }
            
            foreach (var pack in VoicePacketPacket.PacketsSent)
            {
                if (pack.Value >= 7)
                {
                    var plr = ReferenceHub.AllHubs.FirstOrDefault(s => s.connectionToClient.connectionId == pack.Key, null);
                    if (plr != null && !Ids.Contains(plr.authManager.UserId))
                    {
                        Ids.Add(plr.authManager.UserId);
                        Task.Run(async () =>
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                                await VerificationChallenge.AwaitVerification();
                                try
                                {
                                    var response = await client.GetAsync($"http{(QuerySystem.QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.QuerySystem.CurrentMaster}/Api/Sentinal/ReportVC?token={QuerySystem.QuerySystem.QuerySystemKey}&userid={plr.authManager.UserId}");
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