// Decompiled with JetBrains decompiler
// Type: NoammoTrigger
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class NoammoTrigger : MonoBehaviour
{
  public int filter = -1;
  public bool disableOnEnd = true;
  public int triggerID;
  public string alias;
  public int prioirty;
  public int[] optionalForcedID;

  public bool Trigger(int item)
  {
    return false;
  }
}
