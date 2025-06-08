using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.ApiModals;
using Exiled.Loader;
using GameCore;
using LabApi.Events.Arguments.ServerEvents;
using Newtonsoft.Json;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Player = LabApi.Features.Wrappers.Player;
using Version = GameCore.Version;

namespace CedMod.Components
{
    public class AutoUpdater: MonoBehaviour
    {
        public static CedModVersion Pending = new CedModVersion();
        public float TimePassed;
        public float TimePassedCheck;
        
        public float TimePassedWarning;
        public float TimePassedUpdateNotify;
        public bool Installing = false;
        public Byte[] FileToWriteDelayed;
        public bool ForceLog { get; set; }

        public void Update()
        {
            if (QuerySystem.QuerySystemKey == "")
            {
                TimePassedWarning += Time.unscaledDeltaTime;
                if (TimePassedWarning >= 2)
                {
                    TimePassedWarning = 0;
                    Logger.Error($"CedMod requires additional Setup, the plugin will not function and some features will not work if the plugin is not setup.\nPlease follow the setup guide on https://cedmod.nl/Servers/Setup");
                }
                return;
            }
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateWait != 0 && Pending != null)
            {
                if (Player.Count <= 0)
                {
                    TimePassed += Time.unscaledDeltaTime;
                        
                    // if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    //     Logger.Debug($"Checking players {Player.Count} {TimePassed}");
                    
                    if (TimePassed >= CedModMain.Singleton.Config.CedMod.AutoUpdateWait * 60 && !Installing)
                    { 
                        if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                            Logger.Debug($"Prepping install 1");
                        TimePassed = 0;
                        Task.Run(async () =>
                        {
                            Logger.Info($"Installing update {Pending.VersionString} - {Pending.VersionCommit}");
                            await InstallUpdate();
                        });
                    }
                    else if (Installing)
                        TimePassed = 0;
                }
                else
                    TimePassed = 0;
            }

            TimePassedCheck += Time.unscaledDeltaTime;
            // if (CedModMain.Singleton.Config.CedMod.ShowDebug)
            //     Logger.Debug($"Checking players {TimePassedCheck}");
            if (TimePassedCheck >= 300)
            {
                TimePassedCheck = 0;
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Logger.Debug($"Prepping Check");
                Task.Run(async () =>
                {
                    var data = await CheckForUpdates(ForceLog);
                    ForceLog = false;
                    Pending = data;
                });
            }

            if (Pending != null && !CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                TimePassedUpdateNotify += Time.unscaledDeltaTime;
                // if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                //     Logger.Debug($"Checking 3 {TimePassedUpdateNotify}");
                if (TimePassedUpdateNotify >= 300)
                { 
                    TimePassedUpdateNotify = 0;
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Logger.Debug($"Prepping Check");
                    Logger.Warn($"New CedMod Update available: {Pending.VersionString} - {Pending.VersionCommit}\nCurrent: {CedModMain.PluginVersion} - {CedModMain.GitCommitHash}\nPlease update your CedMod version by enabling AutoUpdate in your config or run installcedmodupdate if you dont want automatic updates");
                }
            }
        }

        public void Start()
        {
            Logger.Info($"CedMod AutoUpdater initialized, checking for updates.");
            Task.Run(async () =>
            {
                var data = await CheckForUpdates(true);
                Pending = data;
            });

            LabApi.Events.Handlers.ServerEvents.RoundRestarted += RoundRestart;
            LabApi.Events.Handlers.ServerEvents.RoundEnded += RoundEnd;
        }

        public async Task<CedModVersion> CheckForUpdates(bool b = false)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
#if !EXILED
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/UpdateAvailableNW?VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#else
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/UpdateAvailable?VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Loader.Version.ToString()}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#endif
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            if (b)
                                Logger.Info($"No new updates found for your CedMod Version.");
                        }
                        else
                        {
                            Logger.Error($"Failed to check for updates: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                        }
                        return null;
                    }
                    else
                    {
                        var dat = JsonConvert.DeserializeObject<CedModVersion>(await response.Content.ReadAsStringAsync());
                        Logger.Info($"Update available: {dat.VersionString} - {dat.VersionCommit}\nCurrent: {CedModMain.PluginVersion} - {CedModMain.GitCommitHash}");
                        return dat;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to check for updates: {e}");
            }

            return null;
        }
        
        public void RoundEnd(RoundEndedEventArgs ev)
        {
            if (Pending == null)
            {
                Task.Run(async () =>
                {
                    var data = await CheckForUpdates();
                    Pending = data;
                });
            }
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateRoundEnd && Pending != null && !Installing)
            {
                Task.Run(async () =>
                {
                    Logger.Info($"Installing update {Pending.VersionString} - {Pending.VersionCommit}");
                    await InstallUpdateDelayed();
                    await Task.Delay(2000);
                    Logger.Info($"Saving update {Pending.VersionString} - {Pending.VersionCommit}");
                    Logger.Info($"Saving to: {CedModMain.PluginLocation}");
                    File.WriteAllBytes(CedModMain.PluginLocation, FileToWriteDelayed);
                        
                    ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                    RoundRestarting.RoundRestart.ChangeLevel(true);
                });
            }
        }
        
        public void RoundRestart()
        {
            if (Pending == null)
            {
                Task.Run(async () =>
                {
                    var data = await CheckForUpdates();
                    Pending = data;
                });
            }
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateRoundEnd && Pending != null && Installing && FileToWriteDelayed.Length >= 1)
            {
                Task.Run(() =>
                {
                    Logger.Info($"Saving update {Pending.VersionString} - {Pending.VersionCommit}");
                    Logger.Info($"Saving to: {CedModMain.PluginLocation}");
                    File.WriteAllBytes(CedModMain.PluginLocation, FileToWriteDelayed);
                        
                    ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                    RoundRestarting.RoundRestart.ChangeLevel(true);
                });
            }
        }

        public async Task InstallUpdate()
        {
            if (Installing)
                return;
            Installing = true;
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
#if !EXILED
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownloadNW?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#else 
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownload?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Loader.Version.ToString()}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#endif
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Pending = null;
                        Logger.Info("Version no longer available, retrying later...");
                    }
                    else if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Logger.Error($"Failed to download update: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        Logger.Info($"Downloading CedMod Version {Pending.VersionString} - {Pending.VersionCommit}");
                        var data = await response.Content.ReadAsStreamAsync();
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Logger.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
                        }
                        else
                        {
                            var data1 = await response.Content.ReadAsByteArrayAsync();
                            Logger.Info($"Saving to: {CedModMain.PluginLocation}");
                            File.WriteAllBytes(CedModMain.PluginLocation, data1);
                        
                            ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                            RoundRestarting.RoundRestart.ChangeLevel(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            Installing = false;
        }
        
        public async Task InstallUpdateDelayed()
        {
            if (Installing)
                return;
            Installing = true;
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
#if !EXILED
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownloadNW?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#else 
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownload?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Loader.Version.ToString()}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#endif
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Logger.Error($"Failed to download update: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        Logger.Info($"Downloading CedMod Version {Pending.VersionString} - {Pending.VersionCommit}");
                        var data = await response.Content.ReadAsStreamAsync();
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Logger.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
                        }
                        else
                        {
                            FileToWriteDelayed = await response.Content.ReadAsByteArrayAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
    }
}
