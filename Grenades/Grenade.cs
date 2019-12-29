// Decompiled with JetBrains decompiler
// Type: Grenades.Grenade
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Grenades
{
  [RequireComponent(typeof (AudioSource), typeof (Rigidbody))]
  public abstract class Grenade : NetworkBehaviour
  {
    public float collisionSpeedNeededToPlaySound = 1f;
    public float fuseDuration = 3f;
    [Header("Throws")]
    public float throwForce = 17f;
    public Vector3 throwLinearVelocityOffset = Vector3.up / 4f;
    public Vector3 throwAngularVelocity = new Vector3(10f, 20f, 5f);
    public Vector3 throwStartPositionOffset = new Vector3(0.0715f, 0.0225f, 0.45f);
    public Vector3 throwStartAngle = new Vector3(-100f, 40f, 20f);
    protected CoroutineHandle fuse;
    protected AudioSource source;
    protected Rigidbody rb;
    [Header("Collisions")]
    public AudioClip[] collisionSounds;
    [Header("Fuse")]
    public AudioSource fuseSource;
    [Header("Info")]
    public string logName;
    [SyncVar(hook = "SetThrowerGameObject")]
    public GameObject throwerGameObject;
    [SyncVar]
    public Team throwerTeam;
    [SyncVar(hook = "SetFuseTime")]
    public double fuseTime;
    [SyncVar(hook = "SetRigidbodyVelocities")]
    public RigidbodyVelocityPair serverVelocities;
    [NonSerialized]
    public GrenadeManager thrower;
    [NonSerialized]
    public int currentChainLength;
    private uint ___throwerGameObjectNetId;

    protected virtual void Awake()
    {
      this.source = this.GetComponent<AudioSource>();
      this.rb = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
      this.OnSpeedCollisionEnter(collision, collision.relativeVelocity.magnitude);
    }

    protected virtual void OnSpeedCollisionEnter(Collision collision, float relativeSpeed)
    {
    }

    private void SetThrowerGameObject(GameObject go)
    {
      this.thrower = (UnityEngine.Object) go == (UnityEngine.Object) null ? (GrenadeManager) null : go.GetComponent<GrenadeManager>();
    }

    private void SetFuseTime(double time)
    {
      if (this.fuse.IsValid)
      {
        Timing.KillCoroutines(this.fuse);
        if ((UnityEngine.Object) this.fuseSource != (UnityEngine.Object) null)
          this.fuseSource.Stop();
      }
      float duration = (float) (time - NetworkTime.time);
      this.fuse = Timing.RunCoroutine(this._Fuse(duration), Segment.FixedUpdate);
      if (!((UnityEngine.Object) this.fuseSource != (UnityEngine.Object) null))
        return;
      this.fuseSource.time = this.fuseDuration - duration;
      this.fuseSource.Play();
    }

    private void SetRigidbodyVelocities(RigidbodyVelocityPair velocities)
    {
      this.rb.velocity = velocities.linear;
      this.rb.angularVelocity = velocities.angular;
    }

    public virtual bool ServersideExplosion()
    {
      ServerLogs.AddLog(ServerLogs.Modules.Logger, "Player " + ((UnityEngine.Object) this.thrower != (UnityEngine.Object) null ? this.thrower.ccm.UserId + " (" + this.thrower.nick.MyNick + ")" : "(UNKNOWN)") + "'s " + this.logName + " grenade exploded.", ServerLogs.ServerLogType.GameEvent);
      return true;
    }

    [Server]
    private void FullInitData(
      GrenadeManager player,
      Vector3 position,
      Quaternion rotation,
      Vector3 linearVelocity,
      Vector3 angularVelocity)
    {
      if (!NetworkServer.active)
      {
        Debug.LogWarning((object) "[Server] function 'System.Void Grenades.Grenade::FullInitData(Grenades.GrenadeManager,UnityEngine.Vector3,UnityEngine.Quaternion,UnityEngine.Vector3,UnityEngine.Vector3)' called on client");
      }
      else
      {
        this.NetworkthrowerGameObject = player.gameObject;
        this.NetworkthrowerTeam = player.ccm.Classes.SafeGet(player.ccm.CurClass).team;
        this.NetworkfuseTime = NetworkTime.time + (double) this.fuseDuration;
        this.NetworkserverVelocities = new RigidbodyVelocityPair()
        {
          linear = linearVelocity,
          angular = angularVelocity
        };
        Transform transform = this.transform;
        transform.position = position;
        transform.rotation = rotation;
      }
    }

    [Server]
    public void InitData(
      GrenadeManager player,
      Vector3 relativeVelocity,
      Vector3 normalDirection,
      float forceMultiplier = 1f)
    {
      if (!NetworkServer.active)
        Debug.LogWarning((object) "[Server] function 'System.Void Grenades.Grenade::InitData(Grenades.GrenadeManager,UnityEngine.Vector3,UnityEngine.Vector3,System.Single)' called on client");
      else
        this.FullInitData(player, player._049Cam.TransformPoint(this.throwStartPositionOffset), Quaternion.Euler(this.throwStartAngle), relativeVelocity + this.throwForce * forceMultiplier * (normalDirection + this.throwLinearVelocityOffset).normalized, this.throwAngularVelocity);
    }

    [Server]
    public void InitData(FragGrenade original, Pickup item)
    {
      if (!NetworkServer.active)
      {
        Debug.LogWarning((object) "[Server] function 'System.Void Grenades.Grenade::InitData(Grenades.FragGrenade,Pickup)' called on client");
      }
      else
      {
        Transform transform = item.transform;
        Vector3 position = transform.position;
        Vector3 velocity = item.Rb.velocity;
        this.FullInitData(original.thrower, position, transform.rotation, velocity + original.chainSpeed * (position + Quaternion.LookRotation(velocity.normalized, Vector3.up) * original.chainMovement), item.Rb.angularVelocity);
        this.currentChainLength = original.currentChainLength + 1;
      }
    }

    private IEnumerator<float> _Fuse(float duration)
    {
      Grenade grenade = this;
      for (uint i = 0; (double) i < (double) duration * 50.0; ++i)
        yield return 0.0f;
      if (NetworkServer.active && grenade.ServersideExplosion())
      {
        grenade.gameObject.SetActive(false);
        UnityEngine.Object.Destroy((UnityEngine.Object) grenade.gameObject, 1f);
      }
    }

    private void MirrorProcessed()
    {
    }

    public GameObject NetworkthrowerGameObject
    {
      get
      {
        return this.GetSyncVarGameObject(this.___throwerGameObjectNetId, ref this.throwerGameObject);
      }
      [param: In] set
      {
        if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
        {
          this.setSyncVarHookGuard(1UL, true);
          this.SetThrowerGameObject(value);
          this.setSyncVarHookGuard(1UL, false);
        }
        this.SetSyncVarGameObject(value, ref this.throwerGameObject, 1UL, ref this.___throwerGameObjectNetId);
      }
    }

    public Team NetworkthrowerTeam
    {
      get
      {
        return this.throwerTeam;
      }
      [param: In] set
      {
        this.SetSyncVar<Team>(value, ref this.throwerTeam, 2UL);
      }
    }

    public double NetworkfuseTime
    {
      get
      {
        return this.fuseTime;
      }
      [param: In] set
      {
        if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(4UL))
        {
          this.setSyncVarHookGuard(4UL, true);
          this.SetFuseTime(value);
          this.setSyncVarHookGuard(4UL, false);
        }
        this.SetSyncVar<double>(value, ref this.fuseTime, 4UL);
      }
    }

    public RigidbodyVelocityPair NetworkserverVelocities
    {
      get
      {
        return this.serverVelocities;
      }
      [param: In] set
      {
        if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(8UL))
        {
          this.setSyncVarHookGuard(8UL, true);
          this.SetRigidbodyVelocities(value);
          this.setSyncVarHookGuard(8UL, false);
        }
        this.SetSyncVar<RigidbodyVelocityPair>(value, ref this.serverVelocities, 8UL);
      }
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WriteGameObject(this.NetworkthrowerGameObject);
        NetworkWriterExtensions.WriteByte(writer, (byte) this.throwerTeam);
        writer.WriteDouble(this.fuseTime);
        GeneratedNetworkCode._WriteRigidbodyVelocityPair_None(writer, this.serverVelocities);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WriteGameObject(this.NetworkthrowerGameObject);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 2L) != 0L)
      {
        NetworkWriterExtensions.WriteByte(writer, (byte) this.throwerTeam);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 4L) != 0L)
      {
        writer.WriteDouble(this.fuseTime);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 8L) != 0L)
      {
        GeneratedNetworkCode._WriteRigidbodyVelocityPair_None(writer, this.serverVelocities);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        uint netId = reader.ReadPackedUInt32();
        this.SetThrowerGameObject(this.GetSyncVarGameObject(netId, ref this.throwerGameObject));
        this.___throwerGameObjectNetId = netId;
        this.NetworkthrowerTeam = (Team) NetworkReaderExtensions.ReadByte(reader);
        double time = reader.ReadDouble();
        this.SetFuseTime(time);
        this.NetworkfuseTime = time;
        RigidbodyVelocityPair velocities = GeneratedNetworkCode._ReadRigidbodyVelocityPair_None(reader);
        this.SetRigidbodyVelocities(velocities);
        this.NetworkserverVelocities = velocities;
      }
      else
      {
        long num = (long) reader.ReadPackedUInt64();
        if ((num & 1L) != 0L)
        {
          uint netId = reader.ReadPackedUInt32();
          this.SetThrowerGameObject(this.GetSyncVarGameObject(netId, ref this.throwerGameObject));
          this.___throwerGameObjectNetId = netId;
        }
        if ((num & 2L) != 0L)
          this.NetworkthrowerTeam = (Team) NetworkReaderExtensions.ReadByte(reader);
        if ((num & 4L) != 0L)
        {
          double time = reader.ReadDouble();
          this.SetFuseTime(time);
          this.NetworkfuseTime = time;
        }
        if ((num & 8L) == 0L)
          return;
        RigidbodyVelocityPair velocities = GeneratedNetworkCode._ReadRigidbodyVelocityPair_None(reader);
        this.SetRigidbodyVelocities(velocities);
        this.NetworkserverVelocities = velocities;
      }
    }
  }
}
