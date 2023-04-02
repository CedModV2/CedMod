using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.ApiModals;
using Exiled.Loader;
using Newtonsoft.Json;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using UnityEngine;
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

        public void Update()
        {
            if (QuerySystem.QuerySystemKey == "")
            {
                TimePassedWarning += Time.deltaTime;
                if (TimePassedWarning >= 2)
                {
                    TimePassedWarning = 0;
                    Log.Error($"CedMod requires additional Setup, the plugin will not function and some features will not work if the plugin is not setup.\nPlease follow the setup guide on https://cedmod.nl/Servers/Setup");
                }
                return;
            }
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                if (CedModMain.Singleton.Config.CedMod.AutoUpdateWait != 0 && Pending != null)
                {
                    if (Player.Count <= 1)
                    {
                        TimePassed += Time.deltaTime;
                        
                        // if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        //     Log.Debug($"Checking players {Player.Count} {TimePassed}");
                    
                        if (TimePassed >= CedModMain.Singleton.Config.CedMod.AutoUpdateWait * 60 && !Installing)
                        { 
                            if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                                Log.Debug($"Prepping install 1");
                            TimePassed = 0;
                            Task.Factory.StartNew(async () =>
                            {
                                Log.Info($"Installing update {Pending.VersionString} - {Pending.VersionCommit}");
                                await InstallUpdate();
                            });
                        }
                        else if (Installing)
                            TimePassed = 0;
                    }
                    else
                        TimePassed = 0;
                    
                }
            }
            
            if (Pending == null)
            {
                TimePassedCheck += Time.deltaTime;
                // if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                //     Log.Debug($"Checking players {TimePassedCheck}");
                if (TimePassedCheck >= 300)
                { 
                    TimePassedCheck = 0;
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Prepping Check");
                    Task.Factory.StartNew(async () =>
                    {
                        var data = await CheckForUpdates();
                        Pending = data;
                    });
                }
            }
            else if (Pending != null && !CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                TimePassedUpdateNotify += Time.deltaTime;
                // if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                //     Log.Debug($"Checking 3 {TimePassedUpdateNotify}");
                if (TimePassedUpdateNotify >= 300)
                { 
                    TimePassedUpdateNotify = 0;
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Prepping Check");
                    Log.Warning($"New CedMod Update available: {Pending.VersionString} - {Pending.VersionCommit}\nCurrent: {CedModMain.PluginVersion} - {CedModMain.GitCommitHash}\nPlease update your CedMod version by enabling AutoUpdate in your config or run installcedmodupdate if you dont want automatic updates");
                }
            }
        }

        public void Start()
        {
            Log.Info($"CedMod AutoUpdater initialized, checking for updates.");
            Task.Factory.StartNew(async () =>
            {
                var data = await CheckForUpdates(true);
                Pending = data;
            });
        }

        public async Task<CedModVersion> CheckForUpdates(bool b = false)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
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
                                Log.Info($"No new updates found for your CedMod Version.");
                        }
                        else
                        {
                            Log.Error($"Failed to check for updates: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                        }
                        return null;
                    }
                    else
                    {
                        var dat = JsonConvert.DeserializeObject<CedModVersion>(await response.Content.ReadAsStringAsync());
                        Log.Info($"Update available: {dat.VersionString} - {dat.VersionCommit}\nCurrent: {CedModMain.PluginVersion} - {CedModMain.GitCommitHash}");
                        return dat;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to check for updates: {e}");
            }

            return null;
        }
        
        [PluginEvent(ServerEventType.RoundEnd)]
        public void RoundEnd(RoundSummary.LeadingTeam team)
        {
            if (Pending == null)
            {
                Task.Factory.StartNew(async () =>
                {
                    var data = await CheckForUpdates();
                    Pending = data;
                });
            }
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateRoundEnd && Pending != null && !Installing)
            {
                Task.Factory.StartNew(async () =>
                {
                    Log.Info($"Installing update {Pending.VersionString} - {Pending.VersionCommit}");
                    await InstallUpdateDelayed();
                });
            }
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void RoundRestart()
        {
            if (Pending == null)
            {
                Task.Factory.StartNew(async () =>
                {
                    var data = await CheckForUpdates();
                    Pending = data;
                });
            }
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateRoundEnd && Pending != null && Installing && FileToWriteDelayed.Length >= 1)
            {
                Task.Factory.StartNew(() =>
                {
                    Log.Info($"Saving update {Pending.VersionString} - {Pending.VersionCommit}");
                    Log.Info($"Saving to: {CedModMain.PluginLocation}");
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
#if !EXILED
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownloadNW?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#else 
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownload?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Loader.Version.ToString()}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#endif
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to download update: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        Log.Info($"Downloading CedMod Version {Pending.VersionString} - {Pending.VersionCommit}");
                        var data = await response.Content.ReadAsStreamAsync();
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Log.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
                        }
                        else
                        {
                            var data1 = await response.Content.ReadAsByteArrayAsync();
                            Log.Info($"Saving to: {CedModMain.PluginLocation}");
                            File.WriteAllBytes(CedModMain.PluginLocation, data1);
                        
                            ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                            RoundRestarting.RoundRestart.ChangeLevel(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
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
#if !EXILED
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownloadNW?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#else 
                    var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Version/TargetDownload?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Loader.Version.ToString()}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}");
#endif
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to download update: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}");
                    }
                    else
                    {
                        Log.Info($"Downloading CedMod Version {Pending.VersionString} - {Pending.VersionCommit}");
                        var data = await response.Content.ReadAsStreamAsync();
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Log.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
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
                Log.Error(e.ToString());
            }
        }
    }
}