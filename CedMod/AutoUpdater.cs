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
using Exiled.API.Features;
using Exiled.Loader;
using Newtonsoft.Json;
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
                    if (Player.Dictionary.Count != 0)
                        TimePassed += Time.deltaTime;
                    else
                        TimePassed = 0;
                    
                    Log.Debug($"Checking players {Player.Dictionary.Count} {TimePassed}", CedModMain.Singleton.Config.CedMod.ShowDebug);
                    
                    if (TimePassed >= CedModMain.Singleton.Config.CedMod.AutoUpdateWait * 60 && !Installing)
                    { 
                        Log.Debug($"Prepping install", CedModMain.Singleton.Config.CedMod.ShowDebug);
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
                    Log.Debug($"Checking players {TimePassedCheck}", CedModMain.Singleton.Config.CedMod.ShowDebug);
                    if (TimePassedCheck >= 300)
                    { 
                        TimePassedCheck = 0;
                        Log.Debug($"Prepping Check", CedModMain.Singleton.Config.CedMod.ShowDebug);
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
                    var data = CheckForUpdates();
                    Pending = data;
                });
            }
        }

        public CedModVersion CheckForUpdates()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(QuerySystem.CurrentMaster + $"/Version/UpdateAvailable?VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Exiled.Loader.Loader.Version.ToString()}&ScpSlVersions={GameCore.Version.Major}.{GameCore.Version.Minor}.{GameCore.Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}").Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Log.Error($"Failed to check for updates: {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                    return null;
                }
                else
                {
                    var dat = JsonConvert.DeserializeObject<CedModVersion>(response.Content.ReadAsStringAsync().Result);
                    Log.Info($"Update available: {dat.VersionString} - {dat.VersionCommit}\nCurrent: {CedModMain.Singleton.Version} - {dat.VersionCommit}");
                    return dat;
                }
            }
        }

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

        private void InstallUpdate()
        {
            if (Installing)
                return;
            Installing = true;
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(QuerySystem.CurrentMaster + $"/Version/TargetDownload?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Loader.Version.ToString()}&ScpSlVersions={Version.Major}.{Version.Minor}.{Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}").Result;
                    if (response.StatusCode != HttpStatusCode.Found)
                    {
                        Log.Error($"Failed to download update: {response.StatusCode} | {response.Content.ReadAsStringAsync().Result}");
                    }
                    else
                    {
                        Log.Info($"Downloading CedMod Version");
                        var responseData = response.Content.ReadAsStringAsync().Result;
                        var data = client.GetStreamAsync(responseData).Result;
                        var hash = CedModMain.GetHashCode(data, new MD5CryptoServiceProvider());
                        if (Pending.FileHash != hash)
                        {
                            Log.Error($"CedMod plugin dll does not match pending filehash {Pending.FileHash} | {hash} Please contact CedMod Staff with this versionId {Pending.CedModVersionIdentifier}");
                        }
                        else
                        {
                            var mem = new MemoryStream();
                            data.CopyTo(mem);
                            Log.Info($"Saving to: {CedModMain.Singleton.GetPath()}");
                            File.WriteAllBytes(CedModMain.Singleton.GetPath(), mem.GetBuffer());
                        
                            ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                            RoundRestarting.RoundRestart.ChangeLevel(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Installing = false;
        }
    }
}