using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        public static readonly bool TestApiOnly = true; //this is used when the version contains code that will not work with the main API and so all requests will me made to the test API
        public static string GetCedModVersion()
        {
            int num = 3;
            int num2 = 0;
            int num3 = 2;
            string text = "B";
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
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                string text = webClient.DownloadString("https://api.cedmod.nl/api/scripts/cedmod/version.txt");
                if (text != Initializer.GetCedModVersion())
                {
                    if (GameCore.ConfigFile.ServerConfig.GetBool("cm_autodownload", true))
                    {
                        Initializer.logger.Info("INIT", "New version " + text + " available starting download of cedmod and changelog SERVER MAY FREEZE FOR A BIT");
                        using (WebClient webClient2 = new WebClient())
                        {
                            if (!FileManager.FileExists(Application.dataPath + "../../CedMod[CEDMOD-UPDATER-V" + text + "].dll"))
                            {
                                //Uri address = new Uri("https://api.cedmod.nl/api/scripts/cedmod/CedMod.dll");
                                //Uri address2 = new Uri("https://api.cedmod.nl/api/scripts/cedmod/changelog.txt");
                                //Uri address3 = new Uri("https://api.cedmod.nl/api/scripts/cedmod/Assembly-Csharp.dll");
                                //webClient2.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                                //webClient2.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                                //webClient2.DownloadFile(address, Application.dataPath + "../../CedMod[CEDMOD-UPDATER-V" + text + "].dll");
                                //webClient2.DownloadFile(address2, Application.dataPath + "../../changelog[CEDMOD-UPDATER-V" + text + "].txt");
                                //webClient2.DownloadFile(address2, Application.dataPath + "../../Assembly-Csharp[CEDMOD-UPDATER-V" + text + "].dll");
                                Initializer.logger.Info("INIT", "New version " + text + " Download complete changelog, CedMod.dll and assembly will appear in the folder where LocalAdmin is located shortly");
                            }
                            return;
                        }
                    }
                }
            }
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            Initializer.logger.Error("INIT" ,"X509Certificate [{0}] Policy Error: '{1}'" +
                cert.Subject +
                error.ToString());

            return false;
        }
        public static readonly CedModLogger logger = new CedModLogger();
    }
}
