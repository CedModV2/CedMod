// Decompiled with JetBrains decompiler
// Type: BreakableWindow
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class BreakableWindow : NetworkBehaviour
{
  public float health = 30f;
  public GameObject template;
  public Transform parent;
  public Vector3 size;
  private BreakableWindow.BreakableWindowStatus prevStatus;
  [SyncVar]
  public BreakableWindow.BreakableWindowStatus syncStatus;
  public bool isBroken;
  private MeshRenderer[] meshRenderers;

  [ServerCallback]
  private void UpdateStatus(BreakableWindow.BreakableWindowStatus s)
  {
    if (!NetworkServer.active)
      return;
    this.NetworksyncStatus = s;
  }

  [ServerCallback]
  public void ServerDamageWindow(float damage)
  {
    if (!NetworkServer.active)
      return;
    this.health -= damage;
    if ((double) this.health > 0.0)
      return;
    this.StartCoroutine(this.BreakWindow());
  }

  private void Awake()
  {
    this.meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
    this.GetComponent<Collider>().enabled = false;
    this.Invoke("EnableColliders", 1f);
  }

  private void EnableColliders()
  {
    this.GetComponent<Collider>().enabled = true;
  }

  private void Update()
  {
    if (this.transform.position == this.syncStatus.position && this.transform.rotation == this.syncStatus.rotation && this.isBroken == this.syncStatus.broken)
      return;
    if (NetworkServer.active)
    {
      this.UpdateStatus(new BreakableWindow.BreakableWindowStatus()
      {
        position = this.transform.position,
        rotation = this.transform.rotation,
        broken = this.isBroken
      });
    }
    else
    {
      if (!this.isBroken && this.syncStatus.broken)
        this.StartCoroutine(this.BreakWindow());
      this.transform.position = this.syncStatus.position;
      this.transform.rotation = this.syncStatus.rotation;
      this.isBroken = this.syncStatus.broken;
    }
  }

  private void LateUpdate()
  {
    foreach (MeshRenderer meshRenderer in this.meshRenderers)
    {
      meshRenderer.shadowCastingMode = this.isBroken ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.Off;
      meshRenderer.enabled = !this.isBroken;
      meshRenderer.gameObject.layer = this.isBroken ? 28 : 14;
    }
  }

  private IEnumerator BreakWindow()
  {
    BreakableWindow breakableWindow = this;
    breakableWindow.isBroken = true;
    if (!ServerStatic.IsDedicated)
    {
      foreach (Collider componentsInChild in breakableWindow.GetComponentsInChildren<Collider>())
        componentsInChild.enabled = false;
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(breakableWindow.template, breakableWindow.parent);
      gameObject.transform.localScale = Vector3.one;
      gameObject.transform.localPosition = Vector3.zero;
      gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
      Rigidbody[] rbs = gameObject.GetComponentsInChildren<Rigidbody>();
      List<Vector3> scales = new List<Vector3>();
      foreach (Rigidbody rigidbody in rbs)
      {
        rigidbody.angularVelocity = new Vector3((float) UnityEngine.Random.Range(-360, 360), (float) UnityEngine.Random.Range(-360, 360), (float) UnityEngine.Random.Range(-360, 360));
        rigidbody.velocity = new Vector3((float) UnityEngine.Random.Range(-2, 2), (float) UnityEngine.Random.Range(-2, 2), (float) UnityEngine.Random.Range(-2, 2));
        scales.Add(rigidbody.transform.localScale);
      }
      for (int i = 0; i < 250; ++i)
      {
        for (int index = 0; index < scales.Count; ++index)
          rbs[index].transform.localScale = Vector3.Lerp(scales[index], scales[index] / 2f, (float) i / 75f);
        yield return (object) null;
      }
      for (float i = 0.0f; (double) i < 150.0; ++i)
      {
        for (int index = 0; index < scales.Count; ++index)
          rbs[index].transform.localScale = Vector3.Lerp(scales[index] / 2f, Vector3.zero, i / 150f);
        yield return (object) null;
      }
      foreach (Component component in rbs)
        UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject, 1f);
    }
  }

  private void MirrorProcessed()
  {
  }

  public BreakableWindow.BreakableWindowStatus NetworksyncStatus
  {
    get
    {
      return this.syncStatus;
    }
    [param: In] set
    {
      this.SetSyncVar<BreakableWindow.BreakableWindowStatus>(value, ref this.syncStatus, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      GeneratedNetworkCode._WriteBreakableWindowStatus_BreakableWindow(writer, this.syncStatus);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      GeneratedNetworkCode._WriteBreakableWindowStatus_BreakableWindow(writer, this.syncStatus);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworksyncStatus = GeneratedNetworkCode._ReadBreakableWindowStatus_BreakableWindow(reader);
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworksyncStatus = GeneratedNetworkCode._ReadBreakableWindowStatus_BreakableWindow(reader);
    }
  }

  public struct BreakableWindowStatus : IEquatable<BreakableWindow.BreakableWindowStatus>
  {
    public Vector3 position;
    public Quaternion rotation;
    public bool broken;

    public bool IsEqual(BreakableWindow.BreakableWindowStatus stat)
    {
      return this.position == stat.position && this.rotation == stat.rotation && this.broken == stat.broken;
    }

    public bool Equals(BreakableWindow.BreakableWindowStatus other)
    {
      return this.position == other.position && this.rotation == other.rotation && this.broken == other.broken;
    }

    public override bool Equals(object obj)
    {
      return obj is BreakableWindow.BreakableWindowStatus other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (this.position.GetHashCode() * 397 ^ this.rotation.GetHashCode()) * 397 ^ this.broken.GetHashCode();
    }

    public static bool operator ==(
      BreakableWindow.BreakableWindowStatus left,
      BreakableWindow.BreakableWindowStatus right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      BreakableWindow.BreakableWindowStatus left,
      BreakableWindow.BreakableWindowStatus right)
    {
      return !left.Equals(right);
    }
  }
}
