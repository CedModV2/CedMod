using System;
using System.Drawing;
using System.IO;
using System.Linq;
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

namespace CedMod
{
    public class AutoUpdater: MonoBehaviour
    {
        public static CedModVersion Pending = new CedModVersion();
        public float TimePassed;
        public float TimePassedCheck;
        public bool Installing = false;

        public void Update()
        {
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                if (CedModMain.Singleton.Config.CedMod.AutoUpdateWait != 0 && Pending != null)
                {
                    if (Player.Count == 0)
                        TimePassed += Time.deltaTime;
                    else
                        TimePassed = 0;
                    
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
            }
        }

        public void Start()
        {
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                Log.Info($"CedMod AutoUpdater initialized, checking for updates.");
                Task.Factory.StartNew(() =>
                {
                    var data = CheckForUpdates(true);
                    Pending = data;
                });
            }
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

        [PluginEvent(ServerEventType.RoundRestart)]
        public void RoundRestart()
        {
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && Pending == null)
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
                    InstallUpdate();
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
                            Log.Info($"Saving to: {CedModMain.Assembly.Location}");
                            File.WriteAllBytes(CedModMain.Assembly.Location, data1);
                        
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
    }
}