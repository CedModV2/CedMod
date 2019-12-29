// Decompiled with JetBrains decompiler
// Type: Grenades.GrenadeManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grenades
{
  [RequireComponent(typeof (Inventory))]
  [RequireComponent(typeof (NicknameSync))]
  [RequireComponent(typeof (QueryProcessor))]
  [RequireComponent(typeof (WeaponManager))]
  [RequireComponent(typeof (CharacterClassManager))]
  [RequireComponent(typeof (Scp049PlayerScript))]
  [RequireComponent(typeof (PlyMovementSync))]
  public class GrenadeManager : NetworkBehaviour
  {
    [NonSerialized]
    public float velocityAuditPeriod = 0.1f;
    [NonSerialized]
    public Rigidbody rb;
    [NonSerialized]
    public Inventory inv;
    [NonSerialized]
    public NicknameSync nick;
    [NonSerialized]
    public QueryProcessor query;
    [NonSerialized]
    public WeaponManager weapons;
    [NonSerialized]
    public CharacterClassManager ccm;
    [NonSerialized]
    private PlyMovementSync _pms;
    [NonSerialized]
    public Transform _049Cam;
    private bool isThrowing;
    public GrenadeSettings[] availableGrenades;
    private RateLimit _iawRateLimit;

    private void Start()
    {
      this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[3];
      this.inv = this.GetComponent<Inventory>();
      this.nick = this.GetComponent<NicknameSync>();
      this.query = this.GetComponent<QueryProcessor>();
      this.weapons = this.GetComponent<WeaponManager>();
      this.ccm = this.GetComponent<CharacterClassManager>();
      this._pms = this.GetComponent<PlyMovementSync>();
      this._049Cam = this.GetComponent<Scp049PlayerScript>().plyCam.transform;
    }

    private void Update()
    {
    }

    [Command]
    private void CmdThrowGrenade(int id, bool slowThrow, double time)
    {
      if (this.isServer)
      {
        this.CallCmdThrowGrenade(id, slowThrow, time);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WritePackedInt32(id);
        writer.WriteBoolean(slowThrow);
        writer.WriteDouble(time);
        this.SendCommandInternal(typeof (GrenadeManager), nameof (CmdThrowGrenade), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    private IEnumerator<float> _ServerThrowGrenade(
      GrenadeSettings settings,
      float forceMultiplier,
      int itemIndex,
      float delay)
    {
      GrenadeManager player = this;
      if (itemIndex >= 0 && itemIndex < player.inv.items.Count)
      {
        player.weapons.scp268.ServerDisable();
        float networkDelay = Mathf.Max(delay - player.velocityAuditPeriod, 0.0f);
        ushort i;
        if ((double) networkDelay > 0.0)
        {
          for (i = (ushort) 0; (double) i < 50.0 * (double) networkDelay; ++i)
            yield return 0.0f;
          if (player.ccm.CurClass == RoleType.Spectator)
            yield break;
        }
        float auditDelay = Mathf.Min(delay, player.velocityAuditPeriod);
        Vector3 relativeVelocity;
        if ((double) auditDelay > 0.0)
        {
          Transform localTransform = player.transform;
          Vector3 initialPosition = localTransform.position;
          float initialTime = Time.time;
          for (i = (ushort) 0; (double) i < 50.0 * (double) auditDelay; ++i)
            yield return 0.0f;
          if (player.ccm.CurClass == RoleType.Spectator)
          {
            yield break;
          }
          else
          {
            relativeVelocity = (localTransform.position - initialPosition) / (Time.time - initialTime);
            localTransform = (Transform) null;
            initialPosition = new Vector3();
          }
        }
        else
          relativeVelocity = Vector3.zero;
        Grenade component = UnityEngine.Object.Instantiate<GameObject>(settings.grenadeInstance).GetComponent<Grenade>();
        component.InitData(player, relativeVelocity, player._049Cam.forward, forceMultiplier);
        NetworkServer.Spawn(component.gameObject);
        player.inv.items.RemoveAt(itemIndex);
      }
    }

    private void MirrorProcessed()
    {
    }

    protected static void InvokeCmdCmdThrowGrenade(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdThrowGrenade called on client.");
      else
        ((GrenadeManager) obj).CallCmdThrowGrenade(reader.ReadPackedInt32(), reader.ReadBoolean(), reader.ReadDouble());
    }

    public void CallCmdThrowGrenade(int id, bool slowThrow, double time)
    {
      if (!this._iawRateLimit.CanExecute(true) || id < 0 || this.availableGrenades.Length <= id)
        return;
      GrenadeSettings availableGrenade = this.availableGrenades[id];
      if (availableGrenade.inventoryID != this.inv.curItem)
        return;
      float delay = Mathf.Clamp((float) (time - NetworkTime.time), 0.0f, availableGrenade.throwAnimationDuration);
      float forceMultiplier = slowThrow ? 0.5f : 1f;
      Timing.RunCoroutine(this._ServerThrowGrenade(availableGrenade, forceMultiplier, this.inv.GetItemIndex(), delay), Segment.FixedUpdate);
    }

    static GrenadeManager()
    {
      NetworkBehaviour.RegisterCommandDelegate(typeof (GrenadeManager), "CmdThrowGrenade", new NetworkBehaviour.CmdDelegate(GrenadeManager.InvokeCmdCmdThrowGrenade));
    }
  }
}
