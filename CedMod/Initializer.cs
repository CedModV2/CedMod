// Decompiled with JetBrains decompiler
// Type: CedMod.Initializer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Net;
using UnityEngine;

namespace CedMod
{
  public class Initializer
  {
    public static readonly CedModLogger logger = new CedModLogger();

    public static void Setup()
    {
      Initializer.logger.Info("INIT", "CedMod™ " + Initializer.GetCedModVersion() + " Initialization");
      Initializer.logger.Info("INIT", "For the best experience use MultiAdmin");
      Initializer.logger.Info("Credits", "Used community releases: NeonMod By RogerFK.");
      Initializer.logger.Info("INIT", "Checking for updated");
      using (WebClient webClient1 = new WebClient())
      {
        webClient1.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
        webClient1.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
        string str = webClient1.DownloadString("http://83.82.126.185/api/scripts/cedmod/version.txt");
        if (!(str != Initializer.GetCedModVersion()))
          return;
        if (ConfigFile.ServerConfig.GetBool("cm_autodownload", true))
        {
          Initializer.logger.Info("INIT", "New version " + str + " available starting download of cedmod and changelog SERVER MAY FREEZE FOR A BIT");
          using (WebClient webClient2 = new WebClient())
          {
            Uri address1 = new Uri("http://83.82.126.185/api/scripts/cedmod/Assembly-CSharp.dll");
            Uri address2 = new Uri("http://83.82.126.185/api/scripts/cedmod/changelog.txt");
            webClient2.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
            webClient2.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
            webClient2.DownloadFile(address1, Application.dataPath + "../../Assembly-CSharp[CEDMOD-UPDATER-V" + str + "].dll");
            webClient2.DownloadFile(address2, Application.dataPath + "../../changelog[CEDMOD-UPDATER-V" + str + "].txt");
            Initializer.logger.Info("INIT", "New version " + str + " Download complete changelog and assembly will appear in the folder where LocalAdmin is located shortly");
          }
        }
        else
          Initializer.logger.Warn("INIT", "New version " + str + " Please download the latest version from https://thesecretlaboratory.ddns.net/api/scripts/cedmod/Assembly-CSharp.dll if asked for a password use your API key as username and password");
      }
    }

    public static string GetCedModVersion()
    {
      return string.Format("{0}.{1}.{2}-{3}", (object) 1, (object) 1, (object) 3, (object) "R");
    }
  }
}
