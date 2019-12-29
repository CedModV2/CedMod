// Decompiled with JetBrains decompiler
// Type: Lift
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Grenades;
using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Lift : NetworkBehaviour
{
  public bool operative = true;
  private List<MeshRenderer> panels = new List<MeshRenderer>();
  public Lift.Elevator[] elevators;
  public string elevatorName;
  public float movingSpeed;
  public float maxDistance;
  public bool lockable;
  public Lift.Status status;
  public Material[] panelIcons;
  [SyncVar(hook = "SetLock")]
  private bool locked;
  [SyncVar(hook = "SetStatus")]
  public int statusID;
  private RateLimit _liftMusicRateLimit;

  private void Start()
  {
    this._liftMusicRateLimit = new RateLimit(5, 2.5f, (NetworkConnection) null);
    foreach (Lift.Elevator elevator in this.elevators)
    {
      foreach (MeshRenderer componentsInChild in elevator.door.transform.parent.GetComponentsInChildren<MeshRenderer>())
      {
        if (componentsInChild.CompareTag("ElevatorButton"))
          this.panels.Add(componentsInChild);
      }
    }
  }

  private void RefreshPanelIcons()
  {
    foreach (Renderer panel in this.panels)
      panel.sharedMaterial = this.panelIcons[this.statusID];
  }

  private void Awake()
  {
    foreach (Lift.Elevator elevator in this.elevators)
      elevator.target.tag = "LiftTarget";
  }

  private void FixedUpdate()
  {
    for (int index = 0; index < this.elevators.Length; ++index)
    {
      bool flag = this.statusID == index && this.status != Lift.Status.Moving;
      this.elevators[index].door.SetBool("isOpen", flag);
    }
  }

  private void LateUpdate()
  {
    this.RefreshPanelIcons();
  }

  private void SetStatus(int i)
  {
    this.NetworkstatusID = i;
    this.status = (Lift.Status) i;
  }

  private void SetLock(bool b)
  {
    this.Networklocked = b;
  }

  public void Lock()
  {
    if (!this.lockable)
      return;
    this.SetLock(true);
    Timing.RunCoroutine(this._LockdownUpdate(), Segment.FixedUpdate);
  }

  public bool UseLift()
  {
    if (!this.operative || (double) AlphaWarheadController.Host.timeToDetonation == 0.0 || this.locked)
      return false;
    Timing.RunCoroutine(this._LiftAnimation(), Segment.FixedUpdate);
    this.operative = false;
    return true;
  }

  private IEnumerator<float> _LiftAnimation()
  {
    Transform target = (Transform) null;
    foreach (Lift.Elevator elevator in this.elevators)
    {
      if (!elevator.door.GetBool("isOpen"))
        target = elevator.target;
    }
    Lift.Status previousStatus = this.status;
    this.SetStatus(2);
    int i;
    for (i = 0; i < 35; ++i)
      yield return 0.0f;
    this.RpcPlayMusic();
    for (i = 0; i < 100; ++i)
      yield return 0.0f;
    this.MovePlayers(target);
    for (i = 0; (double) i < ((double) this.movingSpeed - 2.0) * 50.0; ++i)
      yield return 0.0f;
    this.SetStatus(previousStatus == Lift.Status.Down ? 0 : 1);
    for (i = 0; i < 100; ++i)
      yield return 0.0f;
    this.operative = true;
  }

  private IEnumerator<float> _LockdownUpdate()
  {
    while (this.status == Lift.Status.Moving || !this.operative)
      yield return 0.0f;
    if (this.status == Lift.Status.Down)
      Timing.RunCoroutine(this._LiftAnimation(), Segment.FixedUpdate);
  }

  [ClientRpc]
  private void RpcPlayMusic()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Lift), nameof (RpcPlayMusic), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.red;
    foreach (Lift.Elevator elevator in this.elevators)
      Gizmos.DrawWireCube(elevator.target.transform.position, this.maxDistance * 2f * Vector3.one);
  }

  private void MovePlayers(Transform target)
  {
    foreach (GameObject player in PlayerManager.players)
    {
      GameObject which = (GameObject) null;
      if (this.InRange(player.transform.position, out which, 1f) && !((UnityEngine.Object) which.transform == (UnityEngine.Object) target))
      {
        PlyMovementSync component = player.GetComponent<PlyMovementSync>();
        RaycastHit hitInfo;
        if (Physics.Raycast(player.transform.position, Vector3.down, out hitInfo, 100f, (int) component.CollidableSurfaces))
          player.transform.position = hitInfo.point + Vector3.up * 1.23f;
        player.transform.parent = which.transform;
        Vector3 localPosition = player.transform.localPosition;
        player.transform.parent = target.transform;
        player.transform.localPosition = localPosition;
        player.transform.parent = (Transform) null;
        component.SetSafeTime(0.5f);
        component.OverridePosition(player.transform.position, target.transform.rotation.eulerAngles.y - which.transform.rotation.eulerAngles.y, false);
        player.transform.parent = (Transform) null;
      }
    }
    foreach (Lift.Elevator elevator in this.elevators)
    {
      foreach (Collider collider in Physics.OverlapBox(elevator.target.transform.position, this.maxDistance * 2f * Vector3.one))
      {
        if ((UnityEngine.Object) collider.GetComponent<Pickup>() != (UnityEngine.Object) null || (UnityEngine.Object) collider.GetComponent<Grenade>() != (UnityEngine.Object) null)
        {
          GameObject which = (GameObject) null;
          if (this.InRange(collider.transform.position, out which, 1.3f) && !((UnityEngine.Object) which.transform == (UnityEngine.Object) target))
          {
            collider.transform.parent = which.transform;
            Vector3 localPosition = collider.transform.localPosition;
            Quaternion localRotation = collider.transform.localRotation;
            collider.transform.parent = target.transform;
            collider.transform.localPosition = localPosition;
            collider.transform.localRotation = localRotation;
            collider.transform.parent = (Transform) null;
          }
        }
      }
    }
  }

  public bool InRange(Vector3 pos, out GameObject which, float maxDistanceMultiplier = 1f)
  {
    foreach (Lift.Elevator elevator in this.elevators)
    {
      bool flag = (double) Mathf.Abs(elevator.target.position.x - pos.x) <= (double) this.maxDistance * (double) maxDistanceMultiplier;
      if ((double) Mathf.Abs(elevator.target.position.y - pos.y) > (double) this.maxDistance * (double) maxDistanceMultiplier)
        flag = false;
      if ((double) Mathf.Abs(elevator.target.position.z - pos.z) > (double) this.maxDistance * (double) maxDistanceMultiplier)
        flag = false;
      if (flag)
      {
        which = elevator.target.gameObject;
        return true;
      }
    }
    which = (GameObject) null;
    return false;
  }

  private void MirrorProcessed()
  {
  }

  public bool Networklocked
  {
    get
    {
      return this.locked;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetLock(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.locked, 1UL);
    }
  }

  public int NetworkstatusID
  {
    get
    {
      return this.statusID;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(2UL))
      {
        this.setSyncVarHookGuard(2UL, true);
        this.SetStatus(value);
        this.setSyncVarHookGuard(2UL, false);
      }
      this.SetSyncVar<int>(value, ref this.statusID, 2UL);
    }
  }

  protected static void InvokeRpcRpcPlayMusic(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlayMusic called on server.");
    else
      ((Lift) obj).CallRpcPlayMusic();
  }

  public void CallRpcPlayMusic()
  {
  }

  static Lift()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (Lift), "RpcPlayMusic", new NetworkBehaviour.CmdDelegate(Lift.InvokeRpcRpcPlayMusic));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.locked);
      writer.WritePackedInt32(this.statusID);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.locked);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WritePackedInt32(this.statusID);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      bool b = reader.ReadBoolean();
      this.SetLock(b);
      this.Networklocked = b;
      int i = reader.ReadPackedInt32();
      this.SetStatus(i);
      this.NetworkstatusID = i;
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
      {
        bool b = reader.ReadBoolean();
        this.SetLock(b);
        this.Networklocked = b;
      }
      if ((num & 2L) == 0L)
        return;
      int i = reader.ReadPackedInt32();
      this.SetStatus(i);
      this.NetworkstatusID = i;
    }
  }

  [Serializable]
  public struct Elevator : IEquatable<Lift.Elevator>
  {
    public Transform target;
    public Animator door;
    public AudioSource musicSpeaker;
    private Vector3 pos;

    public void SetPosition()
    {
      this.pos = this.target.position;
    }

    public Vector3 GetPosition()
    {
      return this.pos;
    }

    public bool Equals(Lift.Elevator other)
    {
      return (UnityEngine.Object) this.target == (UnityEngine.Object) other.target && (UnityEngine.Object) this.door == (UnityEngine.Object) other.door && (UnityEngine.Object) this.musicSpeaker == (UnityEngine.Object) other.musicSpeaker && this.pos == other.pos;
    }

    public override bool Equals(object obj)
    {
      return obj is Lift.Elevator other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((((UnityEngine.Object) this.target != (UnityEngine.Object) null ? this.target.GetHashCode() : 0) * 397 ^ ((UnityEngine.Object) this.door != (UnityEngine.Object) null ? this.door.GetHashCode() : 0)) * 397 ^ ((UnityEngine.Object) this.musicSpeaker != (UnityEngine.Object) null ? this.musicSpeaker.GetHashCode() : 0)) * 397 ^ this.pos.GetHashCode();
    }

    public static bool operator ==(Lift.Elevator left, Lift.Elevator right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Lift.Elevator left, Lift.Elevator right)
    {
      return !left.Equals(right);
    }
  }

  public enum Status
  {
    Up,
    Down,
    Moving,
  }
}
