// Decompiled with JetBrains decompiler
// Type: Ragdoll
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Ragdoll : NetworkBehaviour
{
  [SyncVar]
  public Ragdoll.Info owner;
  [SyncVar]
  public bool allowRecall;

  private void Start()
  {
    this.Invoke("Unfr", 0.1f);
    this.Invoke("Refreeze", 7f);
  }

  private void Refreeze()
  {
    foreach (UnityEngine.Object componentsInChild in this.GetComponentsInChildren<CharacterJoint>())
      UnityEngine.Object.Destroy(componentsInChild);
    foreach (UnityEngine.Object componentsInChild in this.GetComponentsInChildren<Rigidbody>())
      UnityEngine.Object.Destroy(componentsInChild);
  }

  private void Unfr()
  {
    foreach (Rigidbody componentsInChild in this.GetComponentsInChildren<Rigidbody>())
      componentsInChild.isKinematic = false;
    foreach (Collider componentsInChild in this.GetComponentsInChildren<Collider>())
      componentsInChild.enabled = true;
  }

  private void MirrorProcessed()
  {
  }

  public Ragdoll.Info Networkowner
  {
    get
    {
      return this.owner;
    }
    [param: In] set
    {
      this.SetSyncVar<Ragdoll.Info>(value, ref this.owner, 1UL);
    }
  }

  public bool NetworkallowRecall
  {
    get
    {
      return this.allowRecall;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.allowRecall, 2UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      GeneratedNetworkCode._WriteInfo_Ragdoll(writer, this.owner);
      writer.WriteBoolean(this.allowRecall);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      GeneratedNetworkCode._WriteInfo_Ragdoll(writer, this.owner);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.allowRecall);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networkowner = GeneratedNetworkCode._ReadInfo_Ragdoll(reader);
      this.NetworkallowRecall = reader.ReadBoolean();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.Networkowner = GeneratedNetworkCode._ReadInfo_Ragdoll(reader);
      if ((num & 2L) == 0L)
        return;
      this.NetworkallowRecall = reader.ReadBoolean();
    }
  }

  [Serializable]
  public struct Info : IEquatable<Ragdoll.Info>
  {
    public string ownerHLAPI_id;
    public string DeathCauseText;
    public PlayerStats.HitInfo DeathCause;
    public int PlayerId;

    public Info(string owner, string nick, PlayerStats.HitInfo info, Role c, int playerId)
    {
      this.ownerHLAPI_id = owner;
      this.PlayerId = playerId;
      this.DeathCause = info;
      this.DeathCauseText = TranslationReader.GetFormatted("Death_Causes", 12, "", (object) nick, (object) ("<color=" + RagdollManager.GetColor(c.classColor) + ">" + c.fullName + "</color>"), (object) RagdollManager.GetCause(info, false));
    }

    public bool Equals(Ragdoll.Info other)
    {
      return string.Equals(this.ownerHLAPI_id, other.ownerHLAPI_id) && string.Equals(this.DeathCauseText, other.DeathCauseText) && this.DeathCause == other.DeathCause && this.PlayerId == other.PlayerId;
    }

    public override bool Equals(object obj)
    {
      return obj is Ragdoll.Info other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((this.ownerHLAPI_id != null ? this.ownerHLAPI_id.GetHashCode() : 0) * 397 ^ (this.DeathCauseText != null ? this.DeathCauseText.GetHashCode() : 0)) * 397 ^ this.DeathCause.GetHashCode()) * 397 ^ this.PlayerId;
    }

    public static bool operator ==(Ragdoll.Info left, Ragdoll.Info right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Ragdoll.Info left, Ragdoll.Info right)
    {
      return !left.Equals(right);
    }
  }
}
