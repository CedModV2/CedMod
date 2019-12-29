// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.MirrorConn
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;

namespace Dissonance.Integrations.MirrorIgnorance
{
  public struct MirrorConn : IEquatable<MirrorConn>
  {
    public readonly NetworkConnection Connection;

    public MirrorConn(NetworkConnection connection)
      : this()
    {
      this.Connection = connection;
    }

    public override int GetHashCode()
    {
      return this.Connection.GetHashCode();
    }

    public override string ToString()
    {
      return this.Connection.ToString();
    }

    public override bool Equals(object obj)
    {
      return obj != null && obj is MirrorConn other && this.Equals(other);
    }

    public bool Equals(MirrorConn other)
    {
      if (this.Connection != null)
        return this.Connection.Equals((object) other.Connection);
      return other.Connection == null;
    }
  }
}
