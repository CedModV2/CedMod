// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.DoorPrinter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace RemoteAdmin
{
  internal class DoorPrinter : MonoBehaviour
  {
    public static readonly string[] SpecialValues = new string[2]
    {
      "*",
      "!*"
    };
    public static readonly string[] SpecialTexts = new string[2]
    {
      "(All listed)",
      "(All not listed)"
    };
  }
}
