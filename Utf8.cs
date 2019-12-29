// Decompiled with JetBrains decompiler
// Type: Utf8
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Text;

public static class Utf8
{
  public static byte[] GetBytes(string data)
  {
    return Encoding.UTF8.GetBytes(data);
  }

  public static string GetString(byte[] data)
  {
    return Encoding.UTF8.GetString(data);
  }
}
