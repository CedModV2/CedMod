using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LabApi.Features.Console;

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
                    Logger.Debug($"Verification paused as server is not verified.");
                if (CedModMain.CancellationToken.IsCancellationRequested)
                    break;
                await Task.Delay(2000, CedModMain.CancellationToken);
            }
            
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                await VerificationChallenge.AwaitVerification();
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Logger.Debug($"Getting Id.");
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
                        Logger.Error($"Failed to obtain CedMod verification token: {responseString}");
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
                    await ServerPreferences.WaitForSecond(first ? 45000 : 15000, CedModMain.CancellationToken, (o) => !Shutdown._quitting && CedModMain.Singleton.CacheHandler != null);
                first = false;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Logger.Debug($"verifying Id.");
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
                            Logger.Error($"Failed to verify CedMod verification token: {responseString}");
                        }
                        else if (AmountErrored >= 10)
                        {
                            responseString = "";
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