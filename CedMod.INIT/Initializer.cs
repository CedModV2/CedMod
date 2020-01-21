using System;
using System.Net;
using GameCore;
using UnityEngine;


namespace CedMod.INIT
{
    // Token: 0x020006CA RID: 1738
    public class Initializer
    {
        // Token: 0x06002507 RID: 9479 RVA: 0x000B82DC File Offset: 0x000B64DC
        public static void Setup()
        {
            Initializer.logger.Info("INIT", string.Concat(new string[]
            {
                "CedMod™ ",
                Initializer.GetCedModVersion(),
                " Initialization"
            }));
            Initializer.logger.Info("INIT", "For the best experience use MultiAdmin");
            Initializer.logger.Info("INIT", "Checking for updated");
            Initializer.UpdateCheck();
        }


        // Token: 0x06002508 RID: 9480 RVA: 0x000B835C File Offset: 0x000B655C
        public static string GetCedModVersion()
        {
            int num = 2;
            int num2 = 0;
            int num3 = 0;
            string text = "R";
            return string.Format("{0}.{1}.{2}-{3}", new object[]
            {
                num,
                num2,
                num3,
                text
            });
        }

        public static void UpdateCheck()
        {
            Initializer.logger.Info("INIT", "Checking for updated");
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                string text = webClient.DownloadString("http://83.82.126.185/api/scripts/cedmod/version.txt");
                if (text != Initializer.GetCedModVersion())
                {
                    if (GameCore.ConfigFile.ServerConfig.GetBool("cm_autodownload", true))
                    {
                        Initializer.logger.Info("INIT", "New version " + text + " available starting download of cedmod and changelog SERVER MAY FREEZE FOR A BIT");
                        using (WebClient webClient2 = new WebClient())
                        {
                            if (!FileManager.FileExists(Application.dataPath + "../../Assembly-CSharp[CEDMOD-UPDATER-V" + text + "].dll"))
                            {
                                Uri address = new Uri("http://83.82.126.185/api/scripts/cedmod/CedMod.dll");
                                Uri address2 = new Uri("http://83.82.126.185/api/scripts/cedmod/changelog.txt");
                                webClient2.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                                webClient2.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                                //webClient2.DownloadFile(address, Application.dataPath + "../../CedMod[CEDMOD-UPDATER-V" + text + "].dll");
                                webClient2.DownloadFile(address2, Application.dataPath + "../../changelog[CEDMOD-UPDATER-V" + text + "].txt");
                                Initializer.logger.Info("INIT", "New version " + text + " Download complete changelog and assembly will appear in the folder where LocalAdmin is located shortly");
                            }
                            return;
                        }
                    }
                    Initializer.logger.Warn("INIT", "New version " + text + " Please download the latest version from https://thesecretlaboratory.ddns.net/api/scripts/cedmod/CedMod.dll if asked for a password use your API key as username and password");
                }
            }
        }
        public static readonly CedModLogger logger = new CedModLogger();
    }
}
