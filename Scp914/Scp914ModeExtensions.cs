// Decompiled with JetBrains decompiler
// Type: Scp914.Scp914ModeExtensions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Scp914
{
  public static class Scp914ModeExtensions
  {
    public static bool HasFlagFast(this Scp914Mode value, Scp914Mode flag)
    {
      return (value & flag) == flag;
    }
  }
}
