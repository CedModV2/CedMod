// Decompiled with JetBrains decompiler
// Type: DistanceTo
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceTo : NetworkBehaviour
{
  private static CharacterClassManager localPlayerCcm;
  public float distanceToLocalPlayer;
  public GameObject spectCamera;

  private IEnumerator Start()
  {
    DistanceTo distanceTo = this;
    distanceTo.spectCamera = Object.FindObjectOfType<SpectatorCamera>().gameObject;
    if (distanceTo.isLocalPlayer)
    {
      DistanceTo.localPlayerCcm = distanceTo.GetComponent<CharacterClassManager>();
      WaitForEndOfFrame wait = new WaitForEndOfFrame();
      while (true)
      {
        List<GameObject> players = PlayerManager.players;
        for (int i = 0; i < players.Count; ++i)
        {
          if ((Object) players[i] != (Object) null)
          {
            DistanceTo component = players[i].GetComponent<DistanceTo>();
            if (DistanceTo.localPlayerCcm.CurClass == RoleType.Scp079)
              component.distanceToLocalPlayer = 5f;
            else
              component.CalculateDistanceToLocalPlayer();
          }
          if (i % 4 == 0)
            yield return (object) wait;
        }
        yield return (object) wait;
        players = (List<GameObject>) null;
      }
    }
  }

  public void CalculateDistanceToLocalPlayer()
  {
    this.distanceToLocalPlayer = Vector3.Distance(this.transform.position, DistanceTo.localPlayerCcm.transform.position);
  }

  public bool IsInRange()
  {
    if ((Object) DistanceTo.localPlayerCcm != (Object) null && DistanceTo.localPlayerCcm.CurClass == RoleType.Spectator)
      return true;
    return (double) this.transform.position.y <= 800.0 ? (double) this.distanceToLocalPlayer < 70.0 : (double) this.distanceToLocalPlayer < 500.0;
  }

  private void MirrorProcessed()
  {
  }
}
