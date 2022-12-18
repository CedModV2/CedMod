using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.ApiModals;
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
                        
                        if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                            Log.Debug($"Checking players {Player.Count} {TimePassed}");
                    
                        if (TimePassed >= CedModMain.Singleton.Config.CedMod.AutoUpdateWait * 60 && !Installing)
                        { 
                            if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                                Log.Debug($"Prepping install 1");
                            TimePassed = 0;
                            Task.Factory.StartNew(() =>
                            {
                                Log.Info($"Installing update {Pending.VersionString} - {Pending.VersionCommit}");
                                InstallUpdate();
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
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Checking players {TimePassedCheck}");
                if (TimePassedCheck >= 300)
                { 
                    TimePassedCheck = 0;
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Prepping Check");
                    Task.Factory.StartNew(() =>
                    {
                        var data = CheckForUpdates();
                        Pending = data;
                    });
                }
            }
            else if (Pending != null && !CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                TimePassedUpdateNotify += Time.deltaTime;
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Checking 3 {TimePassedUpdateNotify}");
                if (TimePassedUpdateNotify >= 300)
                { 
                    TimePassedUpdateNotify = 0;
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Debug($"Prepping Check");
                    Log.Warning($"New CedMod Update available: {Pending.VersionString} - {Pending.VersionCommit}\nCurrent: {CedModMain.Version} - {CedModMain.GitCommitHash}\nPlease update your CedMod version by enabling AutoUpdate in your config or run installcedmodupdate if you dont want automatic updates");
                }
            }
        }

        public void Start()
        {
            Log.Info($"CedMod AutoUpdater initialized, checking for updates.");
            Task.Factory.StartNew(() =>
            {
                var data = CheckForUpdates(true);
                Pending = data;
            });
        }

        public CedModVersion CheckForUpdates(bool b = false)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync("https://" + QuerySystem.CurrentMaster + $"/Version/UpdateAvailableNW?VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}").Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            if (b)
                                Log.Info($"No new updates found for your CedMod Version.");
                        }
                        else
                        {
                            Log.Error($"Failed to check for updates: {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                        }
                        return null;
                    }
                    else
                    {
                        var dat = JsonConvert.DeserializeObject<CedModVersion>(response.Content.ReadAsStringAsync().Result);
                        Log.Info($"Update available: {dat.VersionString} - {dat.VersionCommit}\nCurrent: {CedModMain.Version} - {CedModMain.GitCommitHash}");
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
                Task.Factory.StartNew(() =>
                {
                    var data = CheckForUpdates();
                    Pending = data;
                });
            }
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateRoundEnd && Pending != null && !Installing)
            {
                Task.Factory.StartNew(() =>
                {
                    Log.Info($"Installing update {Pending.VersionString} - {Pending.VersionCommit}");
                    InstallUpdateDelayed();
                });
            }
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void RoundRestart()
        {
            if (Pending == null)
            {
                Task.Factory.StartNew(() =>
                {
                    var data = CheckForUpdates();
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

        public void InstallUpdate()
        {
            if (Installing)
                return;
            Installing = true;
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync("https://" + QuerySystem.CurrentMaster + $"/Version/TargetDownloadNW?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}").Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to download update: {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                    }
                    else
                    {
                        Log.Info($"Downloading CedMod Version {Pending.VersionString} - {Pending.VersionCommit}");
                        var data = response.Content.ReadAsStreamAsync().Result;
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Log.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
                        }
                        else
                        {
                            var data1 = response.Content.ReadAsByteArrayAsync().Result;
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
        
        public void InstallUpdateDelayed()
        {
            if (Installing)
                return;
            Installing = true;
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync("https://" + QuerySystem.CurrentMaster + $"/Version/TargetDownloadNW?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}").Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Error($"Failed to download update: {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                    }
                    else
                    {
                        Log.Info($"Downloading CedMod Version {Pending.VersionString} - {Pending.VersionCommit}");
                        var data = response.Content.ReadAsStreamAsync().Result;
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Log.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
                        }
                        else
                        {
                            FileToWriteDelayed = response.Content.ReadAsByteArrayAsync().Result;
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
    }
}