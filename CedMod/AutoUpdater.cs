using System;
using System.Drawing;
using System.IO;
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

namespace CedMod
{
    public class AutoUpdater: MonoBehaviour
    {
        public static CedModVersion Pending = new CedModVersion();
        
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
            
            if (CedModMain.Singleton.Config.CedMod.AutoUpdate && CedModMain.Singleton.Config.CedMod.AutoUpdateRoundEnd && Pending != null)
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
            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(QuerySystem.CurrentMaster + $"/Version/TargetDownload?TargetVersion={Pending.CedModVersionIdentifier}&VersionId={CedModMain.VersionIdentifier}&ExiledVersion={Exiled.Loader.Loader.Version.ToString()}&ScpSlVersions={GameCore.Version.Major}.{GameCore.Version.Minor}.{GameCore.Version.Revision}&OwnHash={CedModMain.FileHash}&token={QuerySystem.QuerySystemKey}").Result;
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
    }
}