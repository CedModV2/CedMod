using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CedMod.Components;
using Newtonsoft.Json;
using PluginAPI.Core;

namespace CedMod.Addons.QuerySystem
{
    public class Verification
    {
        public static int ServerId { get; set; }
        public static int AmountErrored = 0;
        
        public static async Task ObtainId()
        {
            using (HttpClient client = new HttpClient())
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Getting Id.");
                var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Verification/GetId/{QuerySystem.QuerySystemKey}?ip={ServerConsole.Ip}&port={ServerStatic.ServerPort}");
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ServerId = int.Parse(responseString);
                    AmountErrored = 0;
                    await ConfirmId();
                }
                else
                {
                    if (AmountErrored <= 3)
                    {
                        AmountErrored++;
                        Log.Error($"Failed to obtain CedMod verification token: {responseString}");
                    }
                }
            }
        }
        
        public static async Task ConfirmId()
        {
            bool first = true;
            while (true)
            {
                await Task.Delay(first ? 45000 : 15000);
                first = false;
                using (HttpClient client = new HttpClient())
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"verifying Id.");
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Verification/ConfirmId/{QuerySystem.QuerySystemKey}?ip={ServerConsole.Ip}&port={ServerStatic.ServerPort}&queryId={ServerId}");
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ServerId = int.Parse(responseString);
                        AmountErrored = 0;
                    }
                    else
                    {
                        if (AmountErrored <= 3)
                        {
                            AmountErrored++;
                            Log.Error($"Failed to verify CedMod verification token: {responseString}");
                        }
                    }
                }
            }
        }
    }
}