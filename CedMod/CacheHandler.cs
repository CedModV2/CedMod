using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod
{
    public class CacheHandler
    {
        public static void Loop()
        {
            while (!Shutdown._quitting)
            {
                try
                {
                    foreach (var file in Directory.GetFiles(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal")))
                    {
                        var fileData = new FileInfo(file);
                        
                        if (fileData.Name.StartsWith("tempb-"))
                        {
                            var fileContent = File.ReadAllText(file);
                            fileContent = EnsureValidJson(fileContent);
                            var dat1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(fileContent);
                            if (!int.TryParse(dat1["BanDuration"].ToString(), out int dat))
                            {
                                Log.Info($"Fixing broke pending ban");
                                dat1["BanDuration"] = 1440;
                            }
                            else
                            {
                                dat1["BanDuration"] = dat;
                            }

                            fileContent = JsonConvert.SerializeObject(dat1);
                            
                            Dictionary<string, string> result = (Dictionary<string, string>) API.APIRequest("Auth/Ban", fileContent, false, "POST").Result;
                            if (result == null)
                            {
                                Log.Error($"Ban api request still failed, retrying later");
                                continue;
                            }
                            Log.Info($"Ban api request succeeded");
                            File.Delete(file);
                        }
                        else if (fileData.Name.StartsWith("tempm-"))
                        {
                            var fileContent = File.ReadAllText(file);
                            fileContent = EnsureValidJson(fileContent);
                            var dat1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(fileContent);
                            if (!int.TryParse(dat1["Muteduration"].ToString(), out int dat))
                            {
                                Log.Info($"Fixing broke pending mute");
                                dat1["Muteduration"] = 1440;
                            }
                            else
                            {
                                dat1["Muteduration"] = dat;
                            }

                            if (dat1.ContainsKey("Userid"))
                                dat1["UserId"] = dat1["Userid"];
                            
                            fileContent = JsonConvert.SerializeObject(dat1);
                            
                            Dictionary<string, string> result = (Dictionary<string, string>) API.APIRequest($"api/Mute/{dat1["UserId"]}", fileContent, false, "POST").Result;
                            if (result == null)
                            {
                                Log.Error($"Mute api request still failed, retrying later");
                                continue;
                            }
                            Log.Info($"Mute api request succeeded");
                            File.Delete(file);
                        }
                        else if (fileData.Name.StartsWith("tempum-"))
                        {
                            var fileContent = File.ReadAllText(file);
                            fileContent = EnsureValidJson(fileContent);
                            Dictionary<string, string> result = (Dictionary<string, string>) API.APIRequest($"api/Mute/{fileContent}", "", false, "DELETE").Result;
                            if (result == null)
                            {
                                Log.Error($"Unmute api request still failed, retrying later");
                                continue;
                            }
                            Log.Info($"Unmute api request succeeded");
                            File.Delete(file);
                        }
                        else if (fileData.Name.StartsWith("tempd-"))
                        {
                            var date = File.GetLastWriteTime(file);
                            if (date < DateTime.UtcNow.AddDays(-30))
                            {
                                File.Delete(file);
                            }
                        }
                    }

                    WaitForSecond(10);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to process cache: {e}");
                    WaitForSecond(10);
                }
            }
        }

        private static void WaitForSecond(int i)
        {
            int wait = 10;
            while (wait <= 0)
            {
                Thread.Sleep(1000);
                wait--;
            }
        }

        public static string EnsureValidJson(string json)
        {
            json = json.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
            return json;
        }
    }
}
