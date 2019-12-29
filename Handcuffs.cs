// Decompiled with JetBrains decompiler
// Type: Handcuffs
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using Security;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Handcuffs : NetworkBehaviour
{
  [SerializeField]
  private float raycastDistance = 1.2f;
  [SerializeField]
  private float maxCuffDistance = 130f;
  [SerializeField]
  private float uncuffDuration = 1.7f;
  [SyncVar]
  public int CufferId;
  private TextMeshProUGUI disarmerDisplayText;
  private GameObject beingCuffedInfo;
  private Image uncuffProgressImage;
  private float uncuffingProgress;
  private float breakTime;
  private KeyCode _keyPrimary;
  private KeyCode _keySecondary;
  private KeyCode _keyInteract;
  private RateLimit _interactRateLimit;
  [HideInInspector]
  public ReferenceHub MyReferenceHub;

  private void Start()
  {
    if (NetworkServer.active)
      this.NetworkCufferId = -1;
    this.MyReferenceHub = ReferenceHub.GetHub(this.gameObject);
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
  }

  private void Update()
  {
    if (!NetworkServer.active)
      return;
    this.UpdateCuffedPlayers();
  }

  [Server]
  private void UpdateCuffedPlayers()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Handcuffs::UpdateCuffedPlayers()' called on client");
    }
    else
    {
      if (this.CufferId < 0)
        return;
      if (this.MyReferenceHub.inventory.items.Count > 0 || this.MyReferenceHub.ammoBox.amount != "0:0:0")
        this.MyReferenceHub.inventory.ServerDropAll();
      GameObject cuffer = this.GetCuffer(this.CufferId);
      if (!this.IsAliveAndHasDisarmer(cuffer))
        this.NetworkCufferId = -1;
      if ((double) Vector3.Distance(cuffer.transform.position, this.transform.position) > (double) this.maxCuffDistance)
      {
        this.breakTime += Time.deltaTime;
        if ((double) this.breakTime < 1.0)
          return;
        this.NetworkCufferId = -1;
      }
      else
        this.breakTime = 0.0f;
    }
  }

  public GameObject GetCuffer(int id)
  {
    return PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (item => ReferenceHub.GetHub(item).queryProcessor.PlayerId == id));
  }

  public bool IsAliveAndHasDisarmer(GameObject player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return false;
    ReferenceHub hub = ReferenceHub.GetHub(player);
    return hub.characterClassManager.CurClass != RoleType.Spectator && !hub.inventory.items.All<Inventory.SyncItemInfo>((Func<Inventory.SyncItemInfo, bool>) (item => item.id != ItemType.Disarmer));
  }

  [Client]
  private Handcuffs GetTarget()
  {
    if (!NetworkClient.active)
    {
      Debug.LogWarning((object) "[Client] function 'Handcuffs Handcuffs::GetTarget()' called on server");
      return (Handcuffs) null;
    }
    Handcuffs handcuffs = (Handcuffs) null;
    RaycastHit hitInfo;
    if (Physics.Raycast(new Ray(this.transform.position, this.transform.forward), out hitInfo, this.raycastDistance, (int) this.MyReferenceHub.weaponManager.raycastMask))
      handcuffs = hitInfo.collider.GetComponentInParent<Handcuffs>();
    return handcuffs;
  }

  [Server]
  public void ClearTarget()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Handcuffs::ClearTarget()' called on client");
    }
    else
    {
      foreach (GameObject player in PlayerManager.players)
      {
        Handcuffs handcuffs = ReferenceHub.GetHub(player).handcuffs;
        if (handcuffs.CufferId == this.MyReferenceHub.queryProcessor.PlayerId)
        {
          handcuffs.NetworkCufferId = -1;
          break;
        }
      }
    }
  }

  [Command]
  private void CmdUncuffTarget()
  {
    if (this.isServer)
    {
      this.CallCmdUncuffTarget();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Handcuffs), nameof (CmdUncuffTarget), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdFreeTeammate(GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdFreeTeammate(target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Handcuffs), nameof (CmdFreeTeammate), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdCuffTarget(GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdCuffTarget(target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Handcuffs), nameof (CmdCuffTarget), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  public int NetworkCufferId
  {
    get
    {
      return this.CufferId;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.CufferId, 1UL);
    }
  }

  protected static void InvokeCmdCmdUncuffTarget(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUncuffTarget called on client.");
    else
      ((Handcuffs) obj).CallCmdUncuffTarget();
  }

  protected static void InvokeCmdCmdFreeTeammate(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdFreeTeammate called on client.");
    else
      ((Handcuffs) obj).CallCmdFreeTeammate(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdCuffTarget(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdCuffTarget called on client.");
    else
      ((Handcuffs) obj).CallCmdCuffTarget(reader.ReadGameObject());
  }

  public void CallCmdUncuffTarget()
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.ClearTarget();
  }

  public void CallCmdFreeTeammate(GameObject target)
  {
    if (!this._interactRateLimit.CanExecute(true) || (UnityEngine.Object) target == (UnityEngine.Object) null || ((double) Vector3.Distance(target.transform.position, this.transform.position) > (double) this.raycastDistance * 1.10000002384186 || this.MyReferenceHub.characterClassManager.Classes.SafeGet(this.MyReferenceHub.characterClassManager.CurClass).team == Team.SCP))
      return;
    ReferenceHub.GetHub(target).handcuffs.NetworkCufferId = -1;
  }

  public void CallCmdCuffTarget(GameObject target)
  {
    if (!this._interactRateLimit.CanExecute(true) || (UnityEngine.Object) target == (UnityEngine.Object) null || (double) Vector3.Distance(target.transform.position, this.transform.position) > (double) this.raycastDistance * 1.10000002384186)
      return;
    Handcuffs handcuffs = ReferenceHub.GetHub(target).handcuffs;
    if ((UnityEngine.Object) handcuffs == (UnityEngine.Object) null || this.MyReferenceHub.inventory.curItem != ItemType.Disarmer || (this.MyReferenceHub.characterClassManager.CurClass < RoleType.Scp173 || handcuffs.CufferId >= 0) || handcuffs.MyReferenceHub.inventory.curItem != ItemType.None)
      return;
    Team team1 = this.MyReferenceHub.characterClassManager.Classes.SafeGet(this.MyReferenceHub.characterClassManager.CurClass).team;
    Team team2 = this.MyReferenceHub.characterClassManager.Classes.SafeGet(handcuffs.MyReferenceHub.characterClassManager.CurClass).team;
    bool flag = false;
    switch (team1)
    {
      case Team.MTF:
        if (team2 == Team.CHI || team2 == Team.CDP)
          flag = true;
        if (team2 == Team.RSC && ConfigFile.ServerConfig.GetBool("mtf_can_cuff_researchers", false))
        {
          flag = true;
          break;
        }
        break;
      case Team.CHI:
        if (team2 == Team.MTF || team2 == Team.RSC)
          flag = true;
        if (team2 == Team.CDP && ConfigFile.ServerConfig.GetBool("ci_can_cuff_class_d", false))
        {
          flag = true;
          break;
        }
        break;
      case Team.RSC:
        if (team2 == Team.CHI || team2 == Team.CDP)
        {
          flag = true;
          break;
        }
        break;
      case Team.CDP:
        if (team2 == Team.MTF || team2 == Team.RSC)
        {
          flag = true;
          break;
        }
        break;
    }
    if (!flag)
      return;
    this.ClearTarget();
    handcuffs.NetworkCufferId = this.MyReferenceHub.queryProcessor.PlayerId;
  }

  static Handcuffs()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Handcuffs), "CmdUncuffTarget", new NetworkBehaviour.CmdDelegate(Handcuffs.InvokeCmdCmdUncuffTarget));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Handcuffs), "CmdFreeTeammate", new NetworkBehaviour.CmdDelegate(Handcuffs.InvokeCmdCmdFreeTeammate));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Handcuffs), "CmdCuffTarget", new NetworkBehaviour.CmdDelegate(Handcuffs.InvokeCmdCmdCuffTarget));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32(this.CufferId);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32(this.CufferId);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkCufferId = reader.ReadPackedInt32();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworkCufferId = reader.ReadPackedInt32();
    }
  }
}
