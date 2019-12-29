// Decompiled with JetBrains decompiler
// Type: Security.RateLimitCreator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;

namespace Security
{
  internal static class RateLimitCreator
  {
    public static readonly string[] ServerRateLimits = new string[8]
    {
      "playerInteract",
      "modPref",
      "commands",
      "inventory",
      "itemSync",
      "footstep",
      "movementSync",
      "cameraSync"
    };
    private static readonly uint[] DefaultThresholds = new uint[8]
    {
      60U,
      30U,
      150U,
      60U,
      300U,
      20U,
      150U,
      60U
    };
    private static readonly uint[] DefaultWindows = new uint[8]
    {
      5U,
      3U,
      3U,
      5U,
      1U,
      5U,
      1U,
      5U
    };
    private static uint[][] _limits;
    private static int _limitsAmount;
    private static bool _init;
    private static RateLimit _dummy;
    private static RateLimit[] _dummyTable;

    internal static void Load()
    {
      RateLimitCreator._init = true;
      RateLimitCreator._limitsAmount = RateLimitCreator.ServerRateLimits.Length;
      RateLimitCreator._limits = new uint[RateLimitCreator._limitsAmount][];
      for (ushort index = 0; (int) index < RateLimitCreator._limitsAmount; ++index)
      {
        RateLimitCreator._limits[(int) index] = new uint[2];
        RateLimitCreator._limits[(int) index][0] = ConfigFile.ServerConfig.GetUInt("ratelimit_" + RateLimitCreator.ServerRateLimits[(int) index] + "_threshold", RateLimitCreator.DefaultThresholds[(int) index]);
        RateLimitCreator._limits[(int) index][1] = ConfigFile.ServerConfig.GetUInt("ratelimit_" + RateLimitCreator.ServerRateLimits[(int) index] + "_window", RateLimitCreator.DefaultWindows[(int) index]);
      }
      RateLimitCreator._dummy = (RateLimit) new DummyRateLimit();
      RateLimitCreator._dummyTable = new RateLimit[RateLimitCreator._limitsAmount];
      for (ushort index = 0; (int) index < RateLimitCreator._limitsAmount; ++index)
        RateLimitCreator._dummyTable[(int) index] = RateLimitCreator._dummy;
      ServerConsole.AddLog("Rate limiting loaded");
    }

    internal static RateLimit[] CreateRateLimit(NetworkConnection connection, bool dummy = false)
    {
      if (NetworkServer.active && !dummy)
      {
        RateLimit[] rateLimitArray = new RateLimit[RateLimitCreator._limitsAmount];
        for (ushort index = 0; (int) index < RateLimitCreator._limitsAmount; ++index)
          rateLimitArray[(int) index] = new RateLimit((int) RateLimitCreator._limits[(int) index][0], (float) RateLimitCreator._limits[(int) index][1], connection);
        return rateLimitArray;
      }
      if (!RateLimitCreator._init)
        RateLimitCreator.Load();
      return RateLimitCreator._dummyTable;
    }
  }
}
