using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CedMod.ApiModals;
using Newtonsoft.Json;
using PluginAPI.Core;

namespace CedMod.Addons.QuerySystem
{
    public class ServerPreferences
    {
        public static ServerPreferenceModel Prefs = null;
        
        public static async Task ResolvePreferences()
        {
            if (string.IsNullOrEmpty(QuerySystem.QuerySystemKey))
                return;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Getting Prefs.");
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/ServerPreference/GetServerPreference/{QuerySystem.QuerySystemKey}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = JsonConvert.DeserializeObject<ServerPreferenceModel>(await response.Content.ReadAsStringAsync());
                        Prefs = data;
                        File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"ServerPrefs.json"), JsonConvert.SerializeObject(Prefs));
                    }
                    else
                    {
                        Log.Error($"Failed to resolve server preferences, using file: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                        if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"ServerPrefs.json")));
                        Prefs = JsonConvert.DeserializeObject<ServerPreferenceModel>(File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"ServerPrefs.json")));
                        await Task.Delay(1000);
                        await ResolvePreferences();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to resolve server preferences, using file: {e}");
                await Task.Delay(1000);
                await ResolvePreferences();
                return;
            }
            
            await Task.Delay(60000);
            await ResolvePreferences();
        }
    }
}