// Decompiled with JetBrains decompiler
// Type: Recontainer079
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System.Collections.Generic;
using UnityEngine;

public class Recontainer079 : MonoBehaviour
{
  public static Recontainer079 singleton;
  public static bool isLocked;

  private void Awake()
  {
    Recontainer079.isLocked = false;
    Recontainer079.singleton = this;
  }

  public static void BeginContainment(bool forced)
  {
    Timing.RunCoroutine(Recontainer079.singleton._Recontain(forced), Segment.FixedUpdate);
  }

  private IEnumerator<float> _Recontain(bool forced)
  {
    MTFRespawn mtf = PlayerManager.localPlayer.GetComponent<MTFRespawn>();
    PlayerStats ps = PlayerManager.localPlayer.GetComponent<PlayerStats>();
    NineTailedFoxAnnouncer annc = NineTailedFoxAnnouncer.singleton;
    while (annc.queue.Count > 0 || AlphaWarheadController.Host.inProgress)
      yield return float.NegativeInfinity;
    if (!forced)
      mtf.RpcPlayCustomAnnouncement("JAM_" + Random.Range(0, 70).ToString("000") + "_" + (object) Random.Range(2, 5) + " SCP079RECON5", false, true);
    int i;
    for (i = 0; i < 2750; ++i)
      yield return float.NegativeInfinity;
    while (annc.queue.Count > 0 || AlphaWarheadController.Host.inProgress)
      yield return float.NegativeInfinity;
    mtf.RpcPlayCustomAnnouncement("JAM_" + Random.Range(0, 70).ToString("000") + "_" + (object) Random.Range(1, 4) + " SCP079RECON6", true, true);
    mtf.RpcPlayCustomAnnouncement(Scp079PlayerScript.instances.Count > 0 ? "SCP 0 7 9 SUCCESSFULLY TERMINATED USING GENERATOR RECONTAINMENT SEQUENCE" : "FACILITY IS BACK IN OPERATIONAL MODE", false, true);
    for (i = 0; i < 350; ++i)
      yield return float.NegativeInfinity;
    Generator079.generators[0].RpcOvercharge();
    foreach (Door door in Object.FindObjectsOfType<Door>())
    {
      if (door.GetComponent<Scp079Interactable>().currentZonesAndRooms[0].currentZone == "HeavyRooms" && door.isOpen && !door.locked)
        door.ChangeState(true);
    }
    Recontainer079.isLocked = true;
    foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
      ps.HurtPlayer(new PlayerStats.HitInfo(1000001f, "WORLD", DamageTypes.Recontainment, 0), instance.gameObject);
    for (i = 0; i < 500; ++i)
      yield return float.NegativeInfinity;
    Recontainer079.isLocked = false;
  }
}
