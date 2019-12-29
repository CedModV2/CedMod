// Decompiled with JetBrains decompiler
// Type: PlyMovementSync
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using GameCore;
using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlyMovementSync : NetworkBehaviour
{
  private static readonly Collider[] AntiFlyBuffer = new Collider[1];
  public float MaxLatency = 1.5f;
  public float Tolerance = 1.05f;
  private readonly RaycastHit[] _hits = new RaycastHit[3];
  [SyncVar]
  public Vector2 Rotations;
  public Vector3 RealModelPosition;
  private Vector3 _receivedPosition;
  public LayerMask CollidableSurfaces;
  private float _debugDistance;
  [NonSerialized]
  internal bool WhitelistPlayer;
  public float AverageMovementSpeed;
  public bool InSafeTime;
  private Transform _playerCamera;
  private Transform _049Camera;
  private Vector3 _prevPos;
  private Scp207 _scp207;
  private SinkHole _sinkhole;
  private Corroding _corroding;
  private FirstPersonController _fpc;
  private float _speedCounter;
  private float _distanceTraveled;
  private float _flyTime;
  private float _groundedY;
  private float _safeTime;
  private bool _isGrounded;
  private bool _noclipWl;
  private bool _noclipPrevWl;
  private bool _successfullySpawned;
  private int _spawnProcessId;
  public float AFKTime;
  private bool _isAFK;
  private Vector3 _AFKLastPosition;
  private float _timeAFK;
  private RateLimit _syncRateLimit;
  private ReferenceHub _hub;

  public bool IsAFK
  {
    get
    {
      return this._isAFK;
    }
    set
    {
      this._isAFK = value;
      if (!value)
        return;
      this._timeAFK = 0.0f;
      this._AFKLastPosition = this.GetRealPosition();
    }
  }

  public bool NoclipWhitelisted
  {
    get
    {
      return this._noclipWl;
    }
    set
    {
      if (this._noclipWl && !value)
      {
        this._flyTime = 0.0f;
        this._noclipPrevWl = true;
      }
      this._noclipWl = value;
    }
  }

  private void Start()
  {
    this._hub = ReferenceHub.GetHub(this.gameObject);
    this._syncRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[6];
    this._playerCamera = this._hub.playerInteract.playerCamera.transform;
    this._049Camera = this._hub.characterClassManager.Scp049.plyCam.transform;
    this.AFKTime = ConfigFile.ServerConfig.GetFloat("afk_time", 90f);
    if (NetworkServer.active || this.isLocalPlayer)
    {
      this._scp207 = this._hub.effectsController.GetEffect<Scp207>("SCP-207");
      this._corroding = this._hub.effectsController.GetEffect<Corroding>("Corroding");
      this._sinkhole = this._hub.effectsController.GetEffect<SinkHole>("SinkHole");
    }
    if (NetworkServer.active && this._sinkhole != null)
    {
      this._sinkhole.slowAmount = ConfigFile.ServerConfig.GetFloat("sinkhole_slow_amount", 30f);
    }
    else
    {
      if (this._sinkhole == null)
        return;
      this.CmdForceUpdateSinkholeSlow();
    }
  }

  [Command]
  public void CmdForceUpdateSinkholeSlow()
  {
    if (this.isServer)
    {
      this.CallCmdForceUpdateSinkholeSlow();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlyMovementSync), nameof (CmdForceUpdateSinkholeSlow), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  public void RpcForceUpdateSinkholeSlow(float slowAmount)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(slowAmount);
    this.SendRPCInternal(typeof (PlyMovementSync), nameof (RpcForceUpdateSinkholeSlow), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void FixedUpdate()
  {
    if ((double) this.AFKTime <= 0.0 || !this._isAFK)
      return;
    if (this._hub.characterClassManager.CurClass == RoleType.Scp079)
    {
      if ((double) this._hub.characterClassManager.AliveTime <= 13.0)
        return;
      Vector3 realPosition = this.GetRealPosition();
      if ((double) realPosition.x - (double) this._AFKLastPosition.x >= 0.109999999403954 || (double) realPosition.y - (double) this._AFKLastPosition.y >= 0.109999999403954 || (double) realPosition.z - (double) this._AFKLastPosition.z >= 0.109999999403954)
      {
        this.IsAFK = false;
      }
      else
      {
        this._timeAFK += Time.deltaTime;
        if ((double) this._timeAFK < (double) this.AFKTime)
          return;
        ServerConsole.Disconnect(this.connectionToClient, "AFK");
      }
    }
    else
    {
      if (this._hub.characterClassManager.CurClass == RoleType.Spectator)
        return;
      Vector3 realPosition = this.GetRealPosition();
      if ((double) realPosition.x - (double) this._AFKLastPosition.x < 0.109999999403954 && (double) realPosition.y - (double) this._AFKLastPosition.y < 0.109999999403954 && (double) realPosition.z - (double) this._AFKLastPosition.z < 0.109999999403954)
      {
        this._timeAFK += Time.deltaTime;
        if ((double) this._timeAFK < (double) this.AFKTime)
          return;
        ServerConsole.Disconnect(this.connectionToClient, "AFK");
      }
      else
        this.IsAFK = false;
    }
  }

  private Vector3 GetRealPosition()
  {
    if (this._hub.characterClassManager.CurClass != RoleType.Scp079)
      return this.RealModelPosition;
    return (double) this._hub.characterClassManager.AliveTime > 13.0 ? this._hub.characterClassManager.Scp079.currentCamera.transform.position : Interface079.singleton.defaultCamera.transform.position;
  }

  [Command(channel = 1)]
  private void CmdSendRotations(Vector2 rot)
  {
    if (this.isServer)
    {
      this.CallCmdSendRotations(rot);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteVector2(rot);
      this.SendCommandInternal(typeof (PlyMovementSync), nameof (CmdSendRotations), writer, 1);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command(channel = 1)]
  private void CmdSendPosition(Vector3 pos)
  {
    if (this.isServer)
    {
      this.CallCmdSendPosition(pos);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteVector3(pos);
      this.SendCommandInternal(typeof (PlyMovementSync), nameof (CmdSendPosition), writer, 1);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public void SetSafeTime(float newTime)
  {
    this._safeTime = Mathf.Clamp(this._safeTime + newTime, 0.0f, 3f);
    this.InSafeTime = true;
  }

  public void OverridePosition(Vector3 pos, float rot, bool forceGround = false)
  {
    this.SetSafeTime(0.5f);
    RaycastHit hitInfo;
    if (forceGround && Physics.Raycast(pos, Vector3.down, out hitInfo, 100f, (int) this.CollidableSurfaces))
      pos = hitInfo.point + Vector3.up * 1.23f;
    this.PlayScp173SoundIfTeleported();
    if (this._corroding.Enabled && (double) pos.y > -1900.0)
      this._corroding.ServerDisable();
    this._groundedY = pos.y;
    this._receivedPosition = pos;
    this._hub.falldamage._previousHeight = pos.y;
    this.RealModelPosition = pos;
    this.TargetForcePosition(this.connectionToClient, pos);
    this.TargetSetRotation(this.connectionToClient, rot);
  }

  private void Update()
  {
    if (!NetworkServer.active)
      return;
    this.ServerUpdateRealModel();
    this.AntiFly(this.RealModelPosition);
    if ((double) this._safeTime > 0.0)
    {
      this._safeTime = Mathf.Clamp(this._safeTime - Time.deltaTime, 0.0f, this._safeTime);
    }
    else
    {
      if (!this.InSafeTime)
        return;
      this.InSafeTime = false;
    }
  }

  [ServerCallback]
  private void ServerUpdateRealModel()
  {
    if (!NetworkServer.active)
      return;
    if (this.WhitelistPlayer || this.NoclipWhitelisted)
    {
      this.RealModelPosition = this._receivedPosition;
    }
    else
    {
      if (!this._successfullySpawned)
        return;
      float num1 = this._hub.characterClassManager.Classes.SafeGet(this._hub.characterClassManager.CurClass).runSpeed;
      if (this._sinkhole != null && this._sinkhole.Enabled)
        num1 *= (float) (1.0 - (double) this._sinkhole.slowAmount / 100.0);
      if ((double) this._receivedPosition.y > 1500.0)
      {
        if (this._hub.characterClassManager.CurClass != RoleType.Spectator)
        {
          this._receivedPosition = this.RealModelPosition;
          this.TargetForcePosition(this._hub.characterClassManager.connectionToClient, this.RealModelPosition);
        }
        else
          this.RealModelPosition = Vector3.up * 2048f;
      }
      else
      {
        if (this._hub.characterClassManager.CurClass == RoleType.Scp079)
        {
          this.RealModelPosition = Vector3.up * 2080f;
        }
        else
        {
          this._hub.falldamage.CalculateGround();
          if (!this._hub.falldamage.isGrounded)
            this.RealModelPosition.y = this._receivedPosition.y;
          Vector3 vector3_1 = this._receivedPosition - this.RealModelPosition;
          this._debugDistance = vector3_1.magnitude;
          if (this._hub.characterClassManager.CurClass == RoleType.Scp173)
          {
            this.RealModelPosition = this._receivedPosition;
          }
          else
          {
            float num2;
            if (this._hub.animationController.sneaking)
            {
              float walkSpeed = this._hub.characterClassManager.Classes.SafeGet(this._hub.characterClassManager.CurClass).walkSpeed;
              num2 = walkSpeed * 0.4f * this.MaxLatency * this.Tolerance;
              num1 = 0.4f * walkSpeed;
              if (this._scp207.Enabled)
                num1 = 0.8f * walkSpeed;
            }
            else
            {
              num2 = num1 * this.MaxLatency * this.Tolerance;
              if (this._hub.characterClassManager.Scp096.iAm096)
                num1 = this._hub.characterClassManager.Scp096.ServerGetTopSpeed();
              if (this._scp207.Enabled)
                num1 *= 1.2f;
            }
            if ((double) this._debugDistance > (double) num2)
            {
              if ((double) this._safeTime > 0.0)
                return;
              this.TargetForcePosition(this.connectionToClient, this.RealModelPosition);
              return;
            }
            Vector3 vector3_2 = num1 * this.Tolerance * Time.deltaTime * vector3_1.normalized;
            int num3 = Physics.RaycastNonAlloc(new Ray(this.RealModelPosition, this._receivedPosition - this.RealModelPosition), this._hits, this._debugDistance, (int) this.CollidableSurfaces);
            for (int index = 0; index < num3; ++index)
            {
              if ((this._hub.characterClassManager.CurClass == RoleType.Scp106 ? 1 : (this._hits[index].collider.gameObject.layer != 27 ? 0 : ((double) this._hits[index].collider.GetComponentInParent<Door>().curCooldown > 0.0 ? 1 : 0))) == 0)
              {
                this.TargetForcePosition(this.connectionToClient, this.RealModelPosition);
                return;
              }
            }
            if ((double) this._debugDistance < (double) num2)
            {
              if ((double) vector3_2.magnitude > (double) this._debugDistance)
              {
                this.RealModelPosition = this._receivedPosition;
                this._distanceTraveled += this._debugDistance;
              }
              else
              {
                this.RealModelPosition += vector3_2;
                this._distanceTraveled += vector3_2.magnitude;
              }
            }
          }
        }
        this._speedCounter += Time.deltaTime * 2f;
        if ((double) this._speedCounter < 1.0)
          return;
        --this._speedCounter;
        this.AverageMovementSpeed = this._distanceTraveled * 2f;
        this._distanceTraveled = 0.0f;
      }
    }
  }

  private void PlayScp173SoundIfTeleported()
  {
    if (!this._hub.characterClassManager.Scp173.iAm173 || this._hub.characterClassManager.Scp173.CanMove(false))
      return;
    this._hub.footstepSync.RpcPlayLandingFootstep(false);
  }

  [ServerCallback]
  private void AntiFly(Vector3 pos)
  {
    if (!NetworkServer.active || this.WhitelistPlayer || (this.NoclipWhitelisted || !this._successfullySpawned))
      return;
    bool flag = this._hub.characterClassManager.CurClass == RoleType.Spectator || this._hub.characterClassManager.CurClass == RoleType.None || this._hub.characterClassManager.CurClass == RoleType.Scp079;
    this._isGrounded = this._hub.falldamage.isCloseToGround;
    if (this._noclipPrevWl)
    {
      if (this._isGrounded)
      {
        this._noclipPrevWl = false;
        this._flyTime = 0.0f;
      }
      else
      {
        this._flyTime += Time.deltaTime;
        if ((double) this._flyTime < 5.0)
          return;
        this._noclipPrevWl = false;
      }
    }
    if (flag || this._isGrounded)
    {
      this._flyTime = 0.0f;
      this._groundedY = pos.y;
    }
    else
    {
      this._flyTime += Time.deltaTime;
      if (!this._isGrounded)
      {
        if ((double) this._groundedY < (double) pos.y - 3.0)
        {
          if ((double) this._safeTime > 0.0)
            return;
          this.AntiCheatKillPlayer("Killed by the anti-cheat system for flying\n(debug code: 1.3 - vertical flying)");
          return;
        }
        if ((double) this._groundedY > (double) pos.y)
          this._groundedY = pos.y;
      }
      Vector3 vector3 = pos;
      vector3.y -= 50f;
      if (this._receivedPosition != Vector3.up * 2048f && !Physics.Linecast(pos, vector3, (int) this.CollidableSurfaces))
      {
        vector3.y += 23.8f;
        if (Physics.OverlapBoxNonAlloc(vector3, new Vector3(0.5f, 25f, 0.5f), PlyMovementSync.AntiFlyBuffer, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f), (int) this.CollidableSurfaces) == 0)
        {
          if ((double) this._safeTime > 0.0)
            return;
          this.AntiCheatKillPlayer("Killed by the anti-cheat system for flying\n(debug code: 1.2 - no surface detected underneath the player)");
          return;
        }
      }
      if ((double) this._flyTime < 2.20000004768372)
        return;
      this.AntiCheatKillPlayer("Killed by the anti-cheat system for flying\n(debug code: 1.1 - flying time limit exceeded)");
    }
  }

  [TargetRpc(channel = 1)]
  public void TargetSetRotation(NetworkConnection conn, float rot)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(rot);
    this.SendTargetRPCInternal(conn, typeof (PlyMovementSync), nameof (TargetSetRotation), writer, 1);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc(channel = 1)]
  public void TargetForcePosition(NetworkConnection conn, Vector3 pos)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteVector3(pos);
    this.SendTargetRPCInternal(conn, typeof (PlyMovementSync), nameof (TargetForcePosition), writer, 1);
    NetworkWriterPool.Recycle(writer);
  }

  public void OnPlayerClassChange(Vector3 spawnPosition, float rotY)
  {
    Timing.RunCoroutine(this.SafelySpawnPlayer(spawnPosition, rotY), Segment.FixedUpdate);
  }

  private IEnumerator<float> SafelySpawnPlayer(Vector3 position, float rot)
  {
    this._successfullySpawned = false;
    ++this._spawnProcessId;
    int thisProcessId = this._spawnProcessId;
    RaycastHit hitInfo;
    if (Physics.Raycast(position, Vector3.down, out hitInfo, 100f, (int) this.CollidableSurfaces))
    {
      position = hitInfo.point + Vector3.up * 1.23f;
      for (int i = 0; i < 50; ++i)
      {
        if (thisProcessId != this._spawnProcessId)
          yield break;
        else if ((double) Mathf.Abs(this._receivedPosition.y - position.y) < 0.300000011920929 && (double) Vector3.Distance(position, this._receivedPosition) < 2.0 && !Physics.Linecast(position, this._receivedPosition, (int) this.CollidableSurfaces))
        {
          this._successfullySpawned = true;
          yield break;
        }
        else
        {
          Debug.DrawLine(position, this._receivedPosition, Color.magenta, 0.3f);
          this.OverridePosition(position, rot, true);
          for (int w = 0; w < 10; ++w)
            yield return 0.0f;
        }
      }
      this.AntiCheatKillPlayer("Client has failed to spawn.");
    }
    else
      this.AntiCheatKillPlayer("No surface was detected under the requested respawn position. This error should never occur.");
  }

  private void AntiCheatKillPlayer(string message)
  {
    this._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(2000000f, "*" + message, DamageTypes.Flying, 0), this.gameObject);
  }

  private void OnDrawGizmos()
  {
  }

  static PlyMovementSync()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlyMovementSync), "CmdForceUpdateSinkholeSlow", new NetworkBehaviour.CmdDelegate(PlyMovementSync.InvokeCmdCmdForceUpdateSinkholeSlow));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlyMovementSync), "CmdSendRotations", new NetworkBehaviour.CmdDelegate(PlyMovementSync.InvokeCmdCmdSendRotations));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlyMovementSync), "CmdSendPosition", new NetworkBehaviour.CmdDelegate(PlyMovementSync.InvokeCmdCmdSendPosition));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlyMovementSync), "RpcForceUpdateSinkholeSlow", new NetworkBehaviour.CmdDelegate(PlyMovementSync.InvokeRpcRpcForceUpdateSinkholeSlow));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlyMovementSync), "TargetSetRotation", new NetworkBehaviour.CmdDelegate(PlyMovementSync.InvokeRpcTargetSetRotation));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlyMovementSync), "TargetForcePosition", new NetworkBehaviour.CmdDelegate(PlyMovementSync.InvokeRpcTargetForcePosition));
  }

  private void MirrorProcessed()
  {
  }

  public Vector2 NetworkRotations
  {
    get
    {
      return this.Rotations;
    }
    [param: In] set
    {
      this.SetSyncVar<Vector2>(value, ref this.Rotations, 1UL);
    }
  }

  protected static void InvokeCmdCmdForceUpdateSinkholeSlow(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdForceUpdateSinkholeSlow called on client.");
    else
      ((PlyMovementSync) obj).CallCmdForceUpdateSinkholeSlow();
  }

  protected static void InvokeCmdCmdSendRotations(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSendRotations called on client.");
    else
      ((PlyMovementSync) obj).CallCmdSendRotations(reader.ReadVector2());
  }

  protected static void InvokeCmdCmdSendPosition(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSendPosition called on client.");
    else
      ((PlyMovementSync) obj).CallCmdSendPosition(reader.ReadVector3());
  }

  public void CallCmdForceUpdateSinkholeSlow()
  {
    if (this._sinkhole == null)
      return;
    this.RpcForceUpdateSinkholeSlow(this._sinkhole.slowAmount);
  }

  public void CallCmdSendRotations(Vector2 rot)
  {
    if (!this._syncRateLimit.CanExecute(true))
      return;
    this.NetworkRotations = rot;
    this._049Camera.rotation = Quaternion.Euler(this.Rotations.x, this.Rotations.y, 0.0f);
  }

  public void CallCmdSendPosition(Vector3 pos)
  {
    if (!this._syncRateLimit.CanExecute(true))
      return;
    this._receivedPosition = pos;
  }

  protected static void InvokeRpcRpcForceUpdateSinkholeSlow(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcForceUpdateSinkholeSlow called on server.");
    else
      ((PlyMovementSync) obj).CallRpcForceUpdateSinkholeSlow(reader.ReadSingle());
  }

  protected static void InvokeRpcTargetSetRotation(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSetRotation called on server.");
    else
      ((PlyMovementSync) obj).CallTargetSetRotation(ClientScene.readyConnection, reader.ReadSingle());
  }

  protected static void InvokeRpcTargetForcePosition(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetForcePosition called on server.");
    else
      ((PlyMovementSync) obj).CallTargetForcePosition(ClientScene.readyConnection, reader.ReadVector3());
  }

  public void CallRpcForceUpdateSinkholeSlow(float slowAmount)
  {
    if (this._sinkhole == null)
      return;
    this._sinkhole.slowAmount = slowAmount;
  }

  public void CallTargetSetRotation(NetworkConnection conn, float rot)
  {
  }

  public void CallTargetForcePosition(NetworkConnection conn, Vector3 pos)
  {
    this.transform.position = pos;
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteVector2(this.Rotations);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteVector2(this.Rotations);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkRotations = reader.ReadVector2();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworkRotations = reader.ReadVector2();
    }
  }
}
