// Decompiled with JetBrains decompiler
// Type: DoorColor
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class DoorColor : MonoBehaviour
{
  public Color Open;
  public Color Close;
  public Color LockedUnselected;
  public Color LockedSelected;
  public Color UnlockedUnselected;
  public Color UnlockedSelected;
  public static DoorColor singleton;

  private void Start()
  {
    DoorColor.singleton = this;
  }
}
