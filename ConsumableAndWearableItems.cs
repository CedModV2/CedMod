// Decompiled with JetBrains decompiler
// Type: ConsumableAndWearableItems
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConsumableAndWearableItems : NetworkBehaviour
{
  public float cooldown;
  [FormerlySerializedAs("medicalItems")]
  public ConsumableAndWearableItems.UsableItem[] usableItems;
  public float[] usableCooldowns;
  public AudioSource soundSource;
  private Inventory inv;
  private PlayerStats ps;
  private KeyCode fireCode;
  private KeyCode cancelCode;
  private string cooldownString;
  private RateLimit _interactRateLimit;
  private bool cursorVisible;
  private bool cancel;
  public float hpToHeal;
  public bool healInProgress;

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this.inv = this.GetComponent<Inventory>();
    this.ps = this.GetComponent<PlayerStats>();
    this.usableCooldowns = new float[this.usableItems.Length];
  }

  private void Update()
  {
    if (NetworkServer.active)
      this.RefreshArtificial();
    int num = this.isLocalPlayer ? 1 : 0;
    if (this.isLocalPlayer || NetworkServer.active)
    {
      for (int index = 0; index < this.usableCooldowns.Length; ++index)
      {
        if ((double) this.usableCooldowns[index] >= 0.0)
          this.usableCooldowns[index] -= Time.deltaTime;
      }
    }
    if ((double) this.cooldown <= 0.0)
      return;
    this.cooldown -= Time.deltaTime;
  }

  private void LateUpdate()
  {
    if (NetworkServer.active && (double) this.hpToHeal > 1.0)
    {
      if ((double) this.ps.health < (double) this.ps.ccm.Classes.SafeGet(this.ps.ccm.CurClass).maxHP)
        this.ps.HealHPAmount(1f);
      --this.hpToHeal;
    }
    if (!this.isLocalPlayer)
      return;
    this.cursorVisible = Cursor.visible;
  }

  [Server]
  private void RefreshArtificial()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void ConsumableAndWearableItems::RefreshArtificial()' called on client");
    }
    else
    {
      if ((double) this.ps.unsyncedArtificialHealth + (double) this.ps.syncArtificialHealth <= 0.0)
        return;
      this.ps.unsyncedArtificialHealth = Mathf.Clamp(this.ps.unsyncedArtificialHealth - Time.deltaTime * 0.75f, 0.0f, (float) this.ps.maxArtificialHealth);
      this.ps.NetworksyncArtificialHealth = (byte) this.ps.unsyncedArtificialHealth;
    }
  }

  [Command]
  private void CmdCancelMedicalItem()
  {
    if (this.isServer)
    {
      this.CallCmdCancelMedicalItem();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (ConsumableAndWearableItems), nameof (CmdCancelMedicalItem), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdUseMedicalItem()
  {
    if (this.isServer)
    {
      this.CallCmdUseMedicalItem();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (ConsumableAndWearableItems), nameof (CmdUseMedicalItem), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcDoAnimations(int healAnimationEnumId, int medId)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(healAnimationEnumId);
    writer.WritePackedInt32(medId);
    this.SendRPCInternal(typeof (ConsumableAndWearableItems), nameof (RpcDoAnimations), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcSetCooldown(int mid, float seconds)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(mid);
    writer.WriteSingle(seconds);
    this.SendRPCInternal(typeof (ConsumableAndWearableItems), nameof (RpcSetCooldown), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Server]
  private IEnumerator<float> UseMedicalItem(int mid)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Collections.Generic.IEnumerator`1<System.Single> ConsumableAndWearableItems::UseMedicalItem(System.Int32)' called on client");
      return (IEnumerator<float>) null;
    }
    // ISSUE: object of a compiler-generated type is created
    return (IEnumerator<float>) new ConsumableAndWearableItems.<UseMedicalItem>d__24(0)
    {
      <>4__this = this,
      mid = mid
    };
  }

  [Server]
  private void SendRpc(ConsumableAndWearableItems.HealAnimation animation, int mid)
  {
    if (!NetworkServer.active)
      Debug.LogWarning((object) "[Server] function 'System.Void ConsumableAndWearableItems::SendRpc(ConsumableAndWearableItems/HealAnimation,System.Int32)' called on client");
    else
      this.RpcDoAnimations((int) animation, mid);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdCancelMedicalItem(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdCancelMedicalItem called on client.");
    else
      ((ConsumableAndWearableItems) obj).CallCmdCancelMedicalItem();
  }

  protected static void InvokeCmdCmdUseMedicalItem(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUseMedicalItem called on client.");
    else
      ((ConsumableAndWearableItems) obj).CallCmdUseMedicalItem();
  }

  public void CallCmdCancelMedicalItem()
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    foreach (ConsumableAndWearableItems.UsableItem usableItem in this.usableItems)
    {
      if (usableItem.inventoryID == this.inv.curItem && (double) usableItem.cancelableTime > 0.0)
        this.cancel = true;
    }
  }

  public void CallCmdUseMedicalItem()
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.cancel = false;
    if ((double) this.cooldown > 0.0)
      return;
    for (int mid = 0; mid < this.usableItems.Length; ++mid)
    {
      if (this.usableItems[mid].inventoryID == this.inv.curItem && (double) this.usableCooldowns[mid] <= 0.0)
        Timing.RunCoroutine(this.UseMedicalItem(mid), Segment.FixedUpdate);
    }
  }

  protected static void InvokeRpcRpcDoAnimations(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDoAnimations called on server.");
    else
      ((ConsumableAndWearableItems) obj).CallRpcDoAnimations(reader.ReadPackedInt32(), reader.ReadPackedInt32());
  }

  protected static void InvokeRpcRpcSetCooldown(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcSetCooldown called on server.");
    else
      ((ConsumableAndWearableItems) obj).CallRpcSetCooldown(reader.ReadPackedInt32(), reader.ReadSingle());
  }

  public void CallRpcDoAnimations(int healAnimationEnumId, int medId)
  {
    ConsumableAndWearableItems.HealAnimation healAnimation = (ConsumableAndWearableItems.HealAnimation) healAnimationEnumId;
    switch (healAnimation)
    {
      case ConsumableAndWearableItems.HealAnimation.StartHealing:
        this.cooldown = this.usableItems[medId].animationDuration;
        if (!((UnityEngine.Object) this.usableItems[medId].sound != (UnityEngine.Object) null))
          break;
        this.soundSource.PlayOneShot(this.usableItems[medId].sound);
        break;
      case ConsumableAndWearableItems.HealAnimation.CancelHealing:
      case ConsumableAndWearableItems.HealAnimation.DequipMedicalItem:
        this.cooldown = 0.0f;
        if (healAnimation != ConsumableAndWearableItems.HealAnimation.CancelHealing)
          break;
        this.soundSource.Stop();
        break;
    }
  }

  public void CallRpcSetCooldown(int mid, float seconds)
  {
  }

  static ConsumableAndWearableItems()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (ConsumableAndWearableItems), "CmdCancelMedicalItem", new NetworkBehaviour.CmdDelegate(ConsumableAndWearableItems.InvokeCmdCmdCancelMedicalItem));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ConsumableAndWearableItems), "CmdUseMedicalItem", new NetworkBehaviour.CmdDelegate(ConsumableAndWearableItems.InvokeCmdCmdUseMedicalItem));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ConsumableAndWearableItems), "RpcDoAnimations", new NetworkBehaviour.CmdDelegate(ConsumableAndWearableItems.InvokeRpcRpcDoAnimations));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ConsumableAndWearableItems), "RpcSetCooldown", new NetworkBehaviour.CmdDelegate(ConsumableAndWearableItems.InvokeRpcRpcSetCooldown));
  }

  [Serializable]
  public struct UsableItem : IEquatable<ConsumableAndWearableItems.UsableItem>
  {
    public string label;
    public ItemType inventoryID;
    public float animationDuration;
    public float cancelableTime;
    public float cooldownAfterUse;
    public AudioClip sound;
    [Space]
    public int instantHealth;
    public int artificialHealth;
    public string[] effectsToInitialize;
    public AnimationCurve regenOverTime;

    public bool Equals(ConsumableAndWearableItems.UsableItem other)
    {
      return string.Equals(this.label, other.label) && this.inventoryID == other.inventoryID && ((double) this.animationDuration == (double) other.animationDuration && (double) this.cancelableTime == (double) other.cancelableTime) && ((double) this.cooldownAfterUse == (double) other.cooldownAfterUse && (UnityEngine.Object) this.sound == (UnityEngine.Object) other.sound && (this.instantHealth == other.instantHealth && this.artificialHealth == other.artificialHealth)) && this.effectsToInitialize == other.effectsToInitialize && this.regenOverTime.Equals(other.regenOverTime);
    }

    public override bool Equals(object obj)
    {
      return obj is ConsumableAndWearableItems.UsableItem other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((((((((int) ((ItemType) ((this.label != null ? this.label.GetHashCode() : 0) * 397) ^ this.inventoryID) * 397 ^ this.animationDuration.GetHashCode()) * 397 ^ this.cancelableTime.GetHashCode()) * 397 ^ this.cooldownAfterUse.GetHashCode()) * 397 ^ ((UnityEngine.Object) this.sound != (UnityEngine.Object) null ? this.sound.GetHashCode() : 0)) * 397 ^ this.instantHealth) * 397 ^ this.artificialHealth) * 397 ^ (this.effectsToInitialize != null ? this.effectsToInitialize.GetHashCode() : 0)) * 397 ^ (this.regenOverTime != null ? this.regenOverTime.GetHashCode() : 0);
    }

    public static bool operator ==(
      ConsumableAndWearableItems.UsableItem left,
      ConsumableAndWearableItems.UsableItem right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      ConsumableAndWearableItems.UsableItem left,
      ConsumableAndWearableItems.UsableItem right)
    {
      return !left.Equals(right);
    }

    public enum ItemSlot
    {
      Unwearable,
      Head,
      Hands,
      Suit,
    }
  }

  [Serializable]
  public struct HealthRegenerationProcess : IEquatable<ConsumableAndWearableItems.HealthRegenerationProcess>
  {
    public AnimationCurve regenerationOverTime;
    private float maxTime;
    private int id;
    private static int idAutoIncrement;

    public int GetId()
    {
      return this.id;
    }

    public HealthRegenerationProcess(AnimationCurve curve)
    {
      ++ConsumableAndWearableItems.HealthRegenerationProcess.idAutoIncrement;
      this.id = ConsumableAndWearableItems.HealthRegenerationProcess.idAutoIncrement;
      this.regenerationOverTime = curve;
      this.maxTime = curve.keys[curve.length - 1].time;
    }

    public float GetMaxTime()
    {
      return this.maxTime;
    }

    public bool Equals(
      ConsumableAndWearableItems.HealthRegenerationProcess other)
    {
      return this.regenerationOverTime.Equals(other.regenerationOverTime) && (double) this.maxTime == (double) other.maxTime && this.id == other.id;
    }

    public override bool Equals(object obj)
    {
      return obj is ConsumableAndWearableItems.HealthRegenerationProcess other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((this.regenerationOverTime != null ? this.regenerationOverTime.GetHashCode() : 0) * 397 ^ this.maxTime.GetHashCode()) * 397 ^ this.id;
    }

    public static bool operator ==(
      ConsumableAndWearableItems.HealthRegenerationProcess left,
      ConsumableAndWearableItems.HealthRegenerationProcess right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      ConsumableAndWearableItems.HealthRegenerationProcess left,
      ConsumableAndWearableItems.HealthRegenerationProcess right)
    {
      return !left.Equals(right);
    }
  }

  public enum HealAnimation
  {
    StartHealing,
    CancelHealing,
    DequipMedicalItem,
    ErrorAlreadyEquiped,
  }
}
