// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.PrivateImplementationDetails
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace RemoteAdmin
{
  internal sealed class PrivateImplementationDetails
  {
    internal static uint ComputeStringHash(string s)
    {
      uint num = 0;
      if (s != null)
      {
        num = 2166136261U;
        for (int index = 0; index < s.Length; ++index)
          num = (uint) (((int) s[index] ^ (int) num) * 16777619);
      }
      return num;
    }
  }
}
