// Decompiled with JetBrains decompiler
// Type: CustomVisibility
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CustomVisibility : NetworkBehaviour
{
  public float visRange;

  private void Start()
  {
    Timing.RunCoroutine(this._Start(), Segment.Update);
  }

  private IEnumerator<float> _Start()
  {
    CustomVisibility customVisibility = this;
    NetworkIdentity _myNetworkId = customVisibility.GetComponent<NetworkIdentity>();
    while (NetworkServer.active)
    {
      if (customVisibility.GetComponent<CharacterClassManager>().CurClass < RoleType.Scp173)
        yield return Timing.WaitForSeconds(20f);
      _myNetworkId.RebuildObservers(false);
      yield return Timing.WaitForSeconds(0.5f);
    }
  }

  public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
  {
    foreach (Component component1 in Physics.OverlapSphere(this.transform.position, this.visRange))
    {
      NetworkIdentity component2 = component1.GetComponent<NetworkIdentity>();
      if ((Object) component2 != (Object) null && component2.connectionToClient != null)
        observers.Add(component2.connectionToClient);
    }
    return true;
  }

  private void MirrorProcessed()
  {
  }
}
