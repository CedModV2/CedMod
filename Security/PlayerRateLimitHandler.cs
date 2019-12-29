// Decompiled with JetBrains decompiler
// Type: Security.PlayerRateLimitHandler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;

namespace Security
{
  internal class PlayerRateLimitHandler : NetworkBehaviour
  {
    internal RateLimit[] RateLimits;

    private void Awake()
    {
      this.RateLimits = RateLimitCreator.CreateRateLimit(this.connectionToClient, this.isServer && this.isLocalPlayer);
    }

    private void MirrorProcessed()
    {
    }
  }
}
