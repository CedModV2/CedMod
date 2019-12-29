// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.VersionInfo
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace AmplifyBloom
{
  [Serializable]
  public class VersionInfo
  {
    private static string StageSuffix = "_dev001";
    public const byte Major = 1;
    public const byte Minor = 2;
    public const byte Release = 0;
    [SerializeField]
    private int m_major;
    [SerializeField]
    private int m_minor;
    [SerializeField]
    private int m_release;

    public static string StaticToString()
    {
      return string.Format("{0}.{1}.{2}", (object) (byte) 1, (object) (byte) 2, (object) (byte) 0) + VersionInfo.StageSuffix;
    }

    public override string ToString()
    {
      return string.Format("{0}.{1}.{2}", (object) this.m_major, (object) this.m_minor, (object) this.m_release) + VersionInfo.StageSuffix;
    }

    public int Number
    {
      get
      {
        return this.m_major * 100 + this.m_minor * 10 + this.m_release;
      }
    }

    private VersionInfo()
    {
      this.m_major = 1;
      this.m_minor = 2;
      this.m_release = 0;
    }

    private VersionInfo(byte major, byte minor, byte release)
    {
      this.m_major = (int) major;
      this.m_minor = (int) minor;
      this.m_release = (int) release;
    }

    public static VersionInfo Current()
    {
      return new VersionInfo((byte) 1, (byte) 2, (byte) 0);
    }

    public static bool Matches(VersionInfo version)
    {
      return 1 == version.m_major && 2 == version.m_minor && version.m_release == 0;
    }
  }
}
