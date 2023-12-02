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
            while (ServerStatic.PermissionsHandler == null || !CustomNetworkManager.IsVerified)
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Verification paused as server is not verified.");
                if (CedModMain.CancellationToken.IsCancellationRequested)
                    break;
                await Task.Delay(2000, CedModMain.CancellationToken);
            }
            
            using (HttpClient client = new HttpClient())
            {
                await VerificationChallenge.AwaitVerification();
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Getting Id.");
                var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Verification/GetId/{QuerySystem.QuerySystemKey}?ip={ServerConsole.Ip}&port={(ServerConsole.PortOverride == 0 ? ServerStatic.ServerPort : ServerConsole.PortOverride)}");
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ServerId = int.Parse(responseString);
                    AmountErrored = 0;
                    ServerConsole.ReloadServerName();
                    ServerConsole.Update = true;
                    await ConfirmId();
                }
                else
                {
                    if (AmountErrored <= 3)
                    {
                        AmountErrored++;
                        Log.Error($"Failed to obtain CedMod verification token: {responseString}");
                    }

                    await Task.Delay(1000, CedModMain.CancellationToken);
                    await ObtainId();
                }
            }
        }
        
        public static async Task<string> ConfirmId(bool loop = true)
        {
            bool first = true;
            while (!Shutdown._quitting)
            {
                if (loop)
                    await Task.Delay(first ? 45000 : 15000, CedModMain.CancellationToken);
                first = false;
                using (HttpClient client = new HttpClient())
                {
                    await VerificationChallenge.AwaitVerification();
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"verifying Id.");
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Verification/ConfirmId/{QuerySystem.QuerySystemKey}?ip={ServerConsole.Ip}&port={(ServerConsole.PortOverride == 0 ? ServerStatic.ServerPort : ServerConsole.PortOverride)}&queryId={ServerId}");
                    var responseString = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ServerId = int.Parse(responseString);
                        AmountErrored = 0;
                        if (!loop)
                            return "";
                    }
                    else
                    {
                        if (AmountErrored <= 3)
                        {
                            AmountErrored++;
                            Log.Error($"Failed to verify CedMod verification token: {responseString}");
                        }

                        if (!loop)
                            return responseString;
                    }
                }
            }

            return "";
        }
    }
}