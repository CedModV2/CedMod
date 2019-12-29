// Decompiled with JetBrains decompiler
// Type: SECTR_Modules
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

public static class SECTR_Modules
{
  public static bool AUDIO = false;
  public static bool VIS = false;
  public static bool STREAM = false;
  public static bool DEV = false;
  public static string VERSION = "1.3.6";

  static SECTR_Modules()
  {
    SECTR_Modules.AUDIO = Type.GetType("SECTR_AudioSystem") != (Type) null;
    SECTR_Modules.VIS = Type.GetType("SECTR_CullingCamera") != (Type) null;
    SECTR_Modules.STREAM = Type.GetType("SECTR_Chunk") != (Type) null;
    SECTR_Modules.DEV = Type.GetType("SECTR_Tests") != (Type) null;
  }

  public static bool HasPro()
  {
    return true;
  }

  public static bool HasComplete()
  {
    return SECTR_Modules.AUDIO && SECTR_Modules.VIS && SECTR_Modules.STREAM;
  }
}
