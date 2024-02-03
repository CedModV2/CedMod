using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.Sentinal.Patches;
using InventorySystem.Items.Firearms.Modules;
using Newtonsoft.Json;
using PluginAPI.Core;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CedMod.Addons.Sentinal
{
    public class SentinalBehaviour: MonoBehaviour
    {
        public float AuthTimer = 1;
        public Dictionary<int, Dictionary<uint, int>> FrameCount = new Dictionary<int, Dictionary<uint, int>>();
        Dictionary<uint, Dictionary<int, int>> UserFrames = new Dictionary<uint, Dictionary<int, int>>();
        
        public void FixedUpdate()
        {
            AuthTimer -= UnityEngine.Time.fixedDeltaTime;

            if (AuthTimer <= 0)
            {
                VoicePacketPacket.Tracker.Clear();
                AuthTimer = 1;
                
                //process framedata into easily readable data for api
                foreach (var detectionPackets in FrameCount)
                {
                    foreach (var frameData in detectionPackets.Value)
                    {
                        UserFrames.TryAdd(frameData.Key, new Dictionary<int, int>());
                        UserFrames[frameData.Key].TryAdd(detectionPackets.Key, frameData.Value);
                    }
                }

                foreach (var userFrames in UserFrames)
                {
                    var plr = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == userFrames.Key, null);
                    if (plr != null)
                    {
                        Task.Run(async () =>
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                                await VerificationChallenge.AwaitVerification();
                                try
                                {
                                    var response = await client.PostAsync($"http{(QuerySystem.QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.QuerySystem.CurrentMaster}/Api/Sentinal/ReportV2?key={QuerySystem.QuerySystem.QuerySystemKey}&userid={plr.authManager.UserId}&type=VCExploit", new StringContent(JsonConvert.SerializeObject(userFrames), Encoding.Default, "application/json"));
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
                
                UserFrames.Clear();
                FrameCount.Clear(); //clear array as we have reported.
            }
            
            foreach (var pack in VoicePacketPacket.PacketsSent)
            {
                if (pack.Value >= 30) //keep track of all exceeding packets per frame
                {
                    FrameCount.TryAdd(UnityEngine.Time.frameCount, new Dictionary<uint, int>());
                    FrameCount[UnityEngine.Time.frameCount].TryAdd(pack.Key, pack.Value);
                }
            }
            
            //clear the current list for the next frame
            VoicePacketPacket.PacketsSent.Clear();
        }
    }
}