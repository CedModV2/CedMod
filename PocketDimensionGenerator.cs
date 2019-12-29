// Decompiled with JetBrains decompiler
// Type: PocketDimensionGenerator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using System;
using System.Security.Cryptography;
using UnityEngine;

public class PocketDimensionGenerator : MonoBehaviour
{
  private static System.Random _random = new System.Random();

  public void GenerateMap(int seed)
  {
    UnityEngine.Random.InitState(seed);
    if (!NetworkServer.active)
      return;
    byte[] data = new byte[4];
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
      cryptoServiceProvider.GetBytes(data);
    PocketDimensionGenerator._random = new System.Random(BitConverter.ToInt32(data, 0));
    this.GenerateRandom();
  }

  public void GenerateRandom()
  {
    PocketDimensionTeleport[] pdtps = PocketDimensionGenerator.PrepTeleports();
    for (int index1 = 0; index1 < ConfigFile.ServerConfig.GetInt("pd_exit_count", 2) && PocketDimensionGenerator.ContainsKiller(pdtps); ++index1)
    {
      int index2 = -1;
      while ((index2 < 0 || pdtps[index2].GetTeleportType() == PocketDimensionTeleport.PDTeleportType.Exit) && PocketDimensionGenerator.ContainsKiller(pdtps))
        index2 = PocketDimensionGenerator._random.Next(0, pdtps.Length);
      pdtps[Mathf.Clamp(index2, 0, pdtps.Length - 1)].SetType(PocketDimensionTeleport.PDTeleportType.Exit);
    }
  }

  private static PocketDimensionTeleport[] PrepTeleports()
  {
    PocketDimensionTeleport[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PocketDimensionTeleport>();
    for (int index = 0; index < objectsOfType.Length; ++index)
      objectsOfType[index].SetType(PocketDimensionTeleport.PDTeleportType.Killer);
    return objectsOfType;
  }

  private static bool ContainsKiller(PocketDimensionTeleport[] pdtps)
  {
    for (int index = 0; index < pdtps.Length; ++index)
    {
      if (pdtps[index].GetTeleportType() == PocketDimensionTeleport.PDTeleportType.Killer)
        return true;
    }
    return false;
  }
}
