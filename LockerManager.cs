// Decompiled with JetBrains decompiler
// Type: LockerManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LockerManager : NetworkBehaviour
{
  private static readonly int IsOpen = Animator.StringToHash("isOpen");
  public SyncListUshort openLockers = new SyncListUshort();
  public Locker[] lockers;
  public LockerSpawnpoint[] spawnpoints;
  public SpawnableItem[] items;
  public static LockerManager singleton;
  public Material defaultKeycardMat;
  public Material greenKeycardMat;
  public Material redKeycardMat;
  public AudioClip greenClip;
  public AudioClip redClip;

  public void ModifyOpen(int locker, int chamber, bool newState)
  {
    ushort num = (ushort) (1 << chamber);
    if (newState)
      this.openLockers[locker] |= (ushort) (uint) num;
    else
      this.openLockers[locker] &= (ushort) (uint) ~num;
  }

  private void Awake()
  {
    LockerManager.singleton = this;
  }

  private void Start()
  {
    this.openLockers.Callback += new SyncList<ushort>.SyncListChanged(this.OnOpenLocker);
    if (NetworkServer.active)
    {
      foreach (Locker locker in this.lockers)
      {
        ushort num = 0;
        for (int index = 0; index < locker.chambers.Length; ++index)
          num |= (ushort) ((!((Object) locker.chambers[index].doorAnimator != (Object) null) || !locker.chambers[index].doorAnimator.GetBool(LockerManager.IsOpen) ? 0 : 1) << index);
        this.openLockers.Add(num);
      }
    }
    else
    {
      for (int index = 0; index < this.lockers.Length; ++index)
        LockerManager.RefreshLocker(this.lockers[index], this.openLockers[index]);
    }
  }

  private void OnOpenLocker(SyncList<ushort>.Operation op, int itemindex, ushort item)
  {
    LockerManager.RefreshLocker(this.lockers[itemindex], this.openLockers[itemindex]);
  }

  private static void RefreshLocker(Locker locker, ushort bits)
  {
    if (!locker.supportsStandarizedAnimation)
      return;
    for (int index = 0; index < locker.chambers.Length; ++index)
    {
      Animator doorAnimator = locker.chambers[index].doorAnimator;
      if ((Object) doorAnimator != (Object) null)
        doorAnimator.SetBool(LockerManager.IsOpen, ((int) bits & 1 << index) == 1 << index);
    }
  }

  [ClientRpc]
  public void RpcChangeMaterial(int locker, int chamber, bool error)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(locker);
    writer.WritePackedInt32(chamber);
    writer.WriteBoolean(error);
    this.SendRPCInternal(typeof (LockerManager), nameof (RpcChangeMaterial), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcDoSound(int locker, int chamber, bool open)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(locker);
    writer.WritePackedInt32(chamber);
    writer.WriteBoolean(open);
    this.SendRPCInternal(typeof (LockerManager), nameof (RpcDoSound), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void Generate(int seed)
  {
    Random.InitState(seed);
    List<int> intList1 = new List<int>();
    List<int> intList2 = new List<int>();
    for (int index1 = 0; index1 < this.lockers.Length; ++index1)
    {
      if ((double) Random.value * 100.0 <= (double) this.lockers[index1].chanceOfSpawn)
      {
        intList2.Clear();
        for (int index2 = 0; index2 < this.spawnpoints.Length; ++index2)
        {
          if (this.spawnpoints[index2].IsFree() && this.spawnpoints[index2].lockerTag == this.lockers[index1].lockerTag)
            intList2.Add(index2);
        }
        if (intList2.Count == 0)
        {
          RandomSeedSync.DebugError("Not enough spawnpoints for: '" + this.lockers[index1].name + "'", true);
        }
        else
        {
          this.lockers[index1].gameObject.gameObject.SetActive(true);
          intList1.Add(index1);
          int index2 = intList2[Random.Range(0, intList2.Count)];
          this.lockers[index1].gameObject.position = this.spawnpoints[index2].spawnpoint.position;
          this.lockers[index1].gameObject.rotation = this.spawnpoints[index2].spawnpoint.rotation;
          foreach (LockerSpawnpoint spawnpoint in this.spawnpoints)
          {
            if ((double) Vector3.Distance(this.spawnpoints[index2].spawnpoint.position, spawnpoint.spawnpoint.position) < 4.0)
              spawnpoint.Obtain();
          }
        }
      }
    }
    if (!NetworkServer.active)
      return;
    List<int[]> numArrayList = new List<int[]>();
    foreach (SpawnableItem spawnableItem in this.items)
    {
      if ((double) Random.value * 100.0 <= (double) spawnableItem.chanceOfSpawn)
      {
        numArrayList.Clear();
        for (int index1 = 0; index1 < intList1.Count; ++index1)
        {
          for (int index2 = 0; index2 < this.lockers[intList1[index1]].chambers.Length; ++index2)
          {
            if (this.lockers[intList1[index1]].chambers[index2].IsFree() && this.lockers[intList1[index1]].chambers[index2].itemTag == spawnableItem.itemTag)
              numArrayList.Add(new int[2]
              {
                intList1[index1],
                index2
              });
          }
        }
        if (numArrayList.Count != 0)
        {
          int index1 = Random.Range(0, numArrayList.Count);
          Locker locker = this.lockers[numArrayList[index1][0]];
          try
          {
            locker.chambers[numArrayList[index1][1]].Obtain();
            Transform spawnpoint = locker.chambers[numArrayList[index1][1]].spawnpoint;
            for (int index2 = 0; index2 <= spawnableItem.copies; ++index2)
              Timing.RunCoroutine(this.SpawnItem(spawnableItem.inventoryId, spawnpoint.position, spawnpoint.rotation, locker), Segment.FixedUpdate);
          }
          catch
          {
            RandomSeedSync.DebugError("Error:", false);
            RandomSeedSync.DebugError("Locker: " + locker.name, false);
            RandomSeedSync.DebugError("Chamber: " + (object) numArrayList[index1][1], false);
          }
        }
      }
    }
  }

  private IEnumerator<float> SpawnItem(
    ItemType id,
    Vector3 pos,
    Quaternion rot,
    Locker locker)
  {
    Pickup p = PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(id, 0.0f, Vector3.zero, Quaternion.identity, 0, 0, 0);
    p.RefreshDurability(true, true);
    p.info.locked = true;
    locker.AssignPickup(p);
    Rigidbody rb = p.GetComponent<Rigidbody>();
    int i;
    for (i = 0; i < 500; ++i)
    {
      rb.velocity = Vector3.zero;
      p.transform.position = pos;
      p.transform.rotation = rot;
      yield return 0.0f;
    }
    for (i = 0; i < 250; ++i)
      yield return 0.0f;
    if ((double) locker.sortingDuration > 0.0)
      rb.useGravity = false;
    if ((Object) locker.sortingTarget != (Object) null)
      rb.velocity = (p.transform.position - locker.sortingTarget.transform.position).normalized * locker.sortingForce;
    rb.angularVelocity = locker.sortingTorque;
    for (i = 0; (double) i < 50.0 * (double) locker.sortingDuration; ++i)
      yield return 0.0f;
    rb.useGravity = true;
  }

  public LockerManager()
  {
    this.InitSyncObject((SyncObject) this.openLockers);
  }

  static LockerManager()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (LockerManager), "RpcChangeMaterial", new NetworkBehaviour.CmdDelegate(LockerManager.InvokeRpcRpcChangeMaterial));
    NetworkBehaviour.RegisterRpcDelegate(typeof (LockerManager), "RpcDoSound", new NetworkBehaviour.CmdDelegate(LockerManager.InvokeRpcRpcDoSound));
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcChangeMaterial(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcChangeMaterial called on server.");
    else
      ((LockerManager) obj).CallRpcChangeMaterial(reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadBoolean());
  }

  protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDoSound called on server.");
    else
      ((LockerManager) obj).CallRpcDoSound(reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadBoolean());
  }

  public void CallRpcChangeMaterial(int locker, int chamber, bool error)
  {
    this.lockers[locker].chambers[chamber].DoMaterial(error ? this.redKeycardMat : this.greenKeycardMat, this.defaultKeycardMat);
  }

  public void CallRpcDoSound(int locker, int chamber, bool open)
  {
  }
}
