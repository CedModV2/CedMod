// Decompiled with JetBrains decompiler
// Type: Security.RateLimit
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

namespace Security
{
  public class RateLimit
  {
    private readonly int _usagesAllowed;
    private uint _usages;
    private readonly float _timeWindow;
    private float _usageTime;
    private readonly NetworkConnection _conn;

    public RateLimit(int usagesAllowed, float timeWindow, NetworkConnection conn = null)
    {
      this._usagesAllowed = usagesAllowed;
      this._timeWindow = timeWindow;
      this._conn = conn;
    }

    public bool CanExecute(bool countUsage = true)
    {
      if (this._usagesAllowed < 0)
        return true;
      if ((double) this._timeWindow >= 0.0 && (double) Time.fixedTime - (double) this._usageTime > (double) this._timeWindow)
      {
        this._usages = 1U;
        this._usageTime = Time.fixedTime;
        return true;
      }
      if ((long) this._usages >= (long) this._usagesAllowed)
      {
        if (ServerConsole.RateLimitKick && this._conn != null)
          ServerConsole.Disconnect(this._conn, "Reason: Exceeding rate limit.");
        return false;
      }
      if (countUsage)
        ++this._usages;
      return true;
    }

    public void Reset()
    {
      this._usages = 0U;
      this._usageTime = Time.fixedTime;
    }
  }
}
