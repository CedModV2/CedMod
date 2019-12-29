// Decompiled with JetBrains decompiler
// Type: TeslaGate
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class TeslaGate : NetworkBehaviour
{
  private List<PlayerStats> playerStats = new List<PlayerStats>();
  public Vector3 localPosition;
  public Vector3 localRotation;
  public Vector3 sizeOfKiller;
  public float sizeOfTrigger;
  public GameObject[] killers;
  public AudioSource source;
  public AudioClip clip_warmup;
  public AudioClip clip_shock;
  public LayerMask killerMask;
  public bool showGizmos;
  private bool inProgress;
  private bool next079burst;
  public GameObject particles;

  private void ServerSideCode()
  {
    if (this.inProgress || this.PlayersInRange(false).Count <= 0)
      return;
    this.RpcPlayAnimation();
  }

  private void ClientSideCode()
  {
    this.transform.localPosition = this.localPosition;
    this.transform.localRotation = Quaternion.Euler(this.localRotation);
    this.GetComponent<Renderer>().enabled = true;
  }

  [ClientRpc]
  private void RpcPlayAnimation()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (TeslaGate), nameof (RpcPlayAnimation), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcInstantBurst()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (TeslaGate), nameof (RpcInstantBurst), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcInstantTesla()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (TeslaGate), nameof (RpcInstantTesla), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private IEnumerator<float> _InstantAnimation()
  {
    this.inProgress = true;
    this.source.PlayOneShot(this.clip_shock);
    this.particles.SetActive(true);
    for (int i = 1; i <= 30; ++i)
    {
      foreach (PlayerStats playerStats in this.PlayersInRange(true))
      {
        if (playerStats.isLocalPlayer)
        {
          AchievementManager.Achieve("electrocuted", true);
          if (ReferenceHub.GetHub(playerStats.gameObject).inventory.curItem == ItemType.MicroHID)
            AchievementManager.Achieve("attemptedrecharge", true);
          playerStats.CmdTesla();
        }
      }
      yield return float.NegativeInfinity;
    }
    this.particles.SetActive(false);
    this.inProgress = false;
  }

  private IEnumerator<float> _PlayAnimation()
  {
    this.inProgress = true;
    this.source.PlayOneShot(this.clip_warmup);
    int i;
    for (i = 1; i <= (this.next079burst ? 5 : 30); ++i)
      yield return float.NegativeInfinity;
    this.source.PlayOneShot(this.clip_shock);
    this.particles.SetActive(true);
    bool wasIn079 = this.next079burst;
    for (i = 1; i <= 30; ++i)
    {
      foreach (PlayerStats playerStats in this.PlayersInRange(true))
      {
        if (playerStats.isLocalPlayer)
        {
          AchievementManager.Achieve("electrocuted", true);
          if (ReferenceHub.GetHub(playerStats.gameObject).inventory.curItem == ItemType.MicroHID)
            AchievementManager.Achieve("attemptedrecharge", true);
          playerStats.CmdTesla();
        }
      }
      yield return float.NegativeInfinity;
    }
    this.particles.SetActive(false);
    for (i = 1; i < (this.next079burst ? 5 : 20); ++i)
      yield return float.NegativeInfinity;
    if (wasIn079)
      this.next079burst = false;
    this.inProgress = false;
  }

  private List<PlayerStats> PlayersInRange(bool hurtRange)
  {
    this.playerStats.Clear();
    if (hurtRange)
    {
      foreach (GameObject killer in this.killers)
      {
        foreach (Component component in Physics.OverlapBox(killer.transform.position + Vector3.up * (this.sizeOfKiller.y / 2f), this.sizeOfKiller / 2f, new Quaternion(), (int) this.killerMask))
        {
          PlayerStats componentInParent = component.GetComponentInParent<PlayerStats>();
          if ((Object) componentInParent != (Object) null && componentInParent.ccm.CurClass != RoleType.Spectator)
            this.playerStats.Add(componentInParent);
        }
      }
    }
    else
    {
      foreach (GameObject player in PlayerManager.players)
      {
        if ((double) Vector3.Distance(this.transform.position, player.transform.position) < (double) this.sizeOfTrigger && player.GetComponent<CharacterClassManager>().CurClass != RoleType.Spectator)
          this.playerStats.Add(player.GetComponent<PlayerStats>());
      }
    }
    return this.playerStats;
  }

  private void OnDrawGizmosSelected()
  {
    if (!this.showGizmos)
      return;
    Gizmos.color = new Color(1f, 0.0f, 0.0f, 0.2f);
    foreach (GameObject killer in this.killers)
      Gizmos.DrawCube(killer.transform.position + Vector3.up * (this.sizeOfKiller.y / 2f), this.sizeOfKiller);
    Gizmos.color = new Color(1f, 1f, 0.0f, 0.2f);
    Gizmos.DrawSphere(this.transform.position, this.sizeOfTrigger);
  }

  private void Update()
  {
    if (NetworkServer.active)
      this.ServerSideCode();
    else
      this.ClientSideCode();
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcPlayAnimation(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlayAnimation called on server.");
    else
      ((TeslaGate) obj).CallRpcPlayAnimation();
  }

  protected static void InvokeRpcRpcInstantBurst(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcInstantBurst called on server.");
    else
      ((TeslaGate) obj).CallRpcInstantBurst();
  }

  protected static void InvokeRpcRpcInstantTesla(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcInstantTesla called on server.");
    else
      ((TeslaGate) obj).CallRpcInstantTesla();
  }

  public void CallRpcPlayAnimation()
  {
    if (this.inProgress)
      return;
    Timing.RunCoroutine(this._PlayAnimation(), Segment.FixedUpdate);
  }

  public void CallRpcInstantBurst()
  {
    this.next079burst = true;
    if (this.inProgress)
      return;
    Timing.RunCoroutine(this._PlayAnimation(), Segment.FixedUpdate);
  }

  public void CallRpcInstantTesla()
  {
    if (this.inProgress)
      return;
    Timing.RunCoroutine(this._InstantAnimation(), Segment.FixedUpdate);
  }

  static TeslaGate()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (TeslaGate), "RpcPlayAnimation", new NetworkBehaviour.CmdDelegate(TeslaGate.InvokeRpcRpcPlayAnimation));
    NetworkBehaviour.RegisterRpcDelegate(typeof (TeslaGate), "RpcInstantBurst", new NetworkBehaviour.CmdDelegate(TeslaGate.InvokeRpcRpcInstantBurst));
    NetworkBehaviour.RegisterRpcDelegate(typeof (TeslaGate), "RpcInstantTesla", new NetworkBehaviour.CmdDelegate(TeslaGate.InvokeRpcRpcInstantTesla));
  }
}
