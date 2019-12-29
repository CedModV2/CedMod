// Decompiled with JetBrains decompiler
// Type: Scp914.Scp914Machine
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils.ConfigHandler;

namespace Scp914
{
  public class Scp914Machine : NetworkBehaviour
  {
    [SerializeField]
    private float timeFinish = 8.37f;
    [SerializeField]
    private float timeDoorOpenStart = 13.73f;
    [SerializeField]
    private float timeDoorCloseStart = 1f;
    [SerializeField]
    private float timeEnd = 16.36f;
    private readonly List<CharacterClassManager> players = new List<CharacterClassManager>();
    private readonly List<Pickup> items = new List<Pickup>();
    public static Scp914Machine singleton;
    public static readonly Scp914Knob knobStateMin;
    public static readonly Scp914Knob knobStateMax;
    public static readonly int knobStateCount;
    private Quaternion knobRotation;
    [Header("Audio")]
    public AudioSource soundMachineSource;
    public AudioSource soundKnobSource;
    [Header("Booths")]
    public Transform intake;
    public Transform output;
    public Vector3 inputSize;
    [Header("Doors")]
    public Transform doors;
    public float doorMovementDistance;
    public float doorMovementDuration;
    [Header("User Input")]
    public Transform button;
    public Transform knob;
    public float knobCooldown;
    public float knobSpeed;
    [Header("Recipes")]
    public Scp914Recipe[] recipes;
    public Texture itemBurnedTexture;
    [NonSerialized]
    public Dictionary<ItemType, Dictionary<Scp914Knob, ItemType[]>> recipesDict;
    [SyncVar(hook = "SetKnobState")]
    public Scp914Knob knobState;
    [NonSerialized]
    public ConfigEntry<Scp914Mode> configMode;
    [NonSerialized]
    public bool working;
    [NonSerialized]
    public float curKnobCooldown;

    static Scp914Machine()
    {
      Array values = Enum.GetValues(typeof (Scp914Knob));
      Scp914Machine.knobStateMin = (Scp914Knob) values.GetLowerBound(0);
      Scp914Machine.knobStateMax = (Scp914Knob) values.GetUpperBound(0);
      Scp914Machine.knobStateCount = Scp914Machine.knobStateMax - Scp914Machine.knobStateMin;
      NetworkBehaviour.RegisterRpcDelegate(typeof (Scp914Machine), "RpcActivate", new NetworkBehaviour.CmdDelegate(Scp914Machine.InvokeRpcRpcActivate));
    }

    private void Awake()
    {
      Scp914Machine.singleton = this;
    }

    private void Start()
    {
      this.configMode = new ConfigEntry<Scp914Mode>("914_mode", Scp914Mode.DroppedAndPlayerTeleport, "SCP-914 Mode", "The behavior SCP-914 should use when upgrading items.");
      ConfigFile.ServerConfig.RegisterConfig((ConfigEntry) this.configMode, true);
      this.knobRotation = this.UpdateKnobRotation(this.knobState);
      this.recipesDict = new Dictionary<ItemType, Dictionary<Scp914Knob, ItemType[]>>();
      foreach (Scp914Recipe recipe in this.recipes)
        this.recipesDict.Add(recipe.itemID, new Dictionary<Scp914Knob, ItemType[]>()
        {
          {
            Scp914Knob.Rough,
            recipe.rough
          },
          {
            Scp914Knob.Coarse,
            recipe.coarse
          },
          {
            Scp914Knob.OneToOne,
            recipe.oneToOne
          },
          {
            Scp914Knob.Fine,
            recipe.fine
          },
          {
            Scp914Knob.VeryFine,
            recipe.veryFine
          }
        });
    }

    private void OnDestroy()
    {
      ConfigFile.ServerConfig.UnRegisterConfig((ConfigEntry) this.configMode);
    }

    private void Update()
    {
      if ((double) this.curKnobCooldown > 0.0)
        this.curKnobCooldown = Mathf.Max(this.curKnobCooldown - Time.deltaTime, 0.0f);
      this.knob.localRotation = Quaternion.Lerp(this.knob.localRotation, this.knobRotation, Time.deltaTime * this.knobSpeed);
    }

    private void SetKnobState(Scp914Knob status)
    {
      this.UpdateKnobRotation(status);
      this.soundKnobSource.Play();
    }

    [ClientRpc]
    public void RpcActivate(double time)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteDouble(time);
      this.SendRPCInternal(typeof (Scp914Machine), nameof (RpcActivate), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    private Quaternion UpdateKnobRotation(Scp914Knob status)
    {
      return this.knobRotation = Quaternion.Euler(Vector3.forward * Mathf.Lerp(-89.99f, 89.99f, (float) status / (float) Scp914Machine.knobStateCount));
    }

    public void ChangeKnobStatus()
    {
      if (this.working || (double) Math.Abs(this.curKnobCooldown) > 1.0 / 1000.0)
        return;
      this.curKnobCooldown = this.knobCooldown;
      Scp914Knob scp914Knob = this.knobState + 1;
      this.NetworkknobState = scp914Knob;
      if (scp914Knob <= Scp914Machine.knobStateMax)
        return;
      this.NetworkknobState = Scp914Machine.knobStateMin;
    }

    private IEnumerator<float> _Animation(double time)
    {
      this.soundMachineSource.Play();
      float timeDoorCloseEnd = this.timeDoorCloseStart + this.doorMovementDuration;
      double realtimeSinceStartup = (double) Time.realtimeSinceStartup;
      int i;
      for (i = 0; (double) i < 50.0 * (double) this.timeDoorCloseStart; ++i)
        yield return 0.0f;
      float localTime = 0.0f;
      Transform doorsTransform = this.doors.transform;
      while ((double) localTime < (double) timeDoorCloseEnd)
      {
        doorsTransform.localPosition = Vector3.right * Mathf.Lerp(0.0f, this.doorMovementDistance, (timeDoorCloseEnd - localTime) / this.doorMovementDuration);
        localTime += 0.02f;
        yield return 0.0f;
      }
      doorsTransform.localPosition = Vector3.zero;
      for (i = 0; (double) i < 50.0 * (double) this.timeFinish; ++i)
        yield return 0.0f;
      Exception upgradeException = (Exception) null;
      try
      {
        this.ProcessItems();
      }
      catch (Exception ex)
      {
        upgradeException = ex;
      }
      for (i = 0; (double) i < 50.0 * (double) this.timeDoorOpenStart; ++i)
        yield return 0.0f;
      localTime = 0.0f;
      while ((double) localTime < (double) timeDoorCloseEnd)
      {
        doorsTransform.localPosition = Vector3.right * Mathf.Lerp(this.doorMovementDistance, 0.0f, (timeDoorCloseEnd - localTime) / this.doorMovementDuration);
        localTime += 0.02f;
        yield return 0.0f;
      }
      doorsTransform.localPosition = Vector3.right * this.doorMovementDistance;
      for (i = 0; (double) i < 50.0 * (double) this.timeEnd; ++i)
        yield return 0.0f;
      this.working = false;
      if (upgradeException != null)
        throw upgradeException;
    }

    [ServerCallback]
    private void ProcessItems()
    {
      if (!NetworkServer.active)
        return;
      Collider[] colliderArray = Physics.OverlapBox(this.intake.position, this.inputSize / 2f);
      this.players.Clear();
      this.items.Clear();
      foreach (Collider collider in colliderArray)
      {
        CharacterClassManager component1 = collider.GetComponent<CharacterClassManager>();
        if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        {
          this.players.Add(component1);
        }
        else
        {
          Pickup component2 = collider.GetComponent<Pickup>();
          if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
            this.items.Add(component2);
        }
      }
      try
      {
        this.MoveObjects((IEnumerable<Pickup>) this.items, (IEnumerable<CharacterClassManager>) this.players);
      }
      finally
      {
        this.UpgradeObjects((IEnumerable<Pickup>) this.items, (IReadOnlyCollection<CharacterClassManager>) this.players);
      }
    }

    [ServerCallback]
    private void UpgradeObjects(
      IEnumerable<Pickup> items,
      IReadOnlyCollection<CharacterClassManager> players)
    {
      if (!NetworkServer.active)
        return;
      if (this.configMode.Value.HasFlagFast(Scp914Mode.Dropped))
      {
        foreach (Pickup pickup in items)
        {
          this.UpgradeItem(pickup);
          Scp914Machine.TryFriendshipAchievement(pickup.ItemId, pickup.info.ownerPlayer.GetComponent<CharacterClassManager>(), (IEnumerable<CharacterClassManager>) players);
        }
      }
      if (!this.configMode.Value.HasFlagFast(Scp914Mode.Inventory))
        return;
      bool flag = this.configMode.Value.HasFlagFast(Scp914Mode.Held);
      foreach (CharacterClassManager player in (IEnumerable<CharacterClassManager>) players)
      {
        Inventory component = player.GetComponent<Inventory>();
        if (flag)
          this.UpgradeHeldItem(component, player, (IEnumerable<CharacterClassManager>) players);
        else
          this.UpgradePlayer(component, player, (IEnumerable<CharacterClassManager>) players);
      }
    }

    [ServerCallback]
    private void MoveObjects(IEnumerable<Pickup> items, IEnumerable<CharacterClassManager> players)
    {
      if (!NetworkServer.active)
        return;
      Vector3 vector3 = this.output.position - this.intake.position;
      if (this.configMode.Value.HasFlagFast(Scp914Mode.Dropped))
      {
        foreach (Component component in items)
          component.transform.position += vector3;
      }
      if (!this.configMode.Value.HasFlagFast(Scp914Mode.DroppedAndPlayerTeleport) && !this.configMode.Value.HasFlagFast(Scp914Mode.Inventory))
        return;
      foreach (CharacterClassManager player in players)
        player.GetComponent<PlyMovementSync>().OverridePosition(player.transform.position + vector3, 0.0f, false);
    }

    private static void TryFriendshipAchievement(
      ItemType itemID,
      CharacterClassManager possibleScientist,
      IEnumerable<CharacterClassManager> players)
    {
      if (possibleScientist.CurClass != RoleType.Scientist)
        return;
      Item itemById = possibleScientist.GetComponent<Inventory>().GetItemByID(itemID);
      if ((itemById != null ? (itemById.itemCategory != ItemCategory.Keycard ? 1 : 0) : 1) != 0)
        return;
      foreach (CharacterClassManager player in players)
      {
        if (player.CurClass == RoleType.ClassD)
          possibleScientist.GetComponent<PlayerStats>().TargetAchieve(possibleScientist.connectionToClient, "friendship");
      }
    }

    public ItemType[] UpgradeItemIDs(ItemType itemID)
    {
      Dictionary<Scp914Knob, ItemType[]> dictionary;
      ItemType[] itemTypeArray;
      return !this.recipesDict.TryGetValue(itemID, out dictionary) || !dictionary.TryGetValue(this.knobState, out itemTypeArray) ? (ItemType[]) null : itemTypeArray;
    }

    public ItemType UpgradeItemID(ItemType itemID)
    {
      ItemType[] itemTypeArray = this.UpgradeItemIDs(itemID);
      return itemTypeArray == null ? itemID : itemTypeArray[UnityEngine.Random.Range(0, itemTypeArray.Length)];
    }

    public bool UpgradeItem(Pickup item)
    {
      ItemType id = this.UpgradeItemID(item.info.itemId);
      if (id < ItemType.KeycardJanitor)
      {
        item.Delete();
        return false;
      }
      item.SetIDFull(id);
      return true;
    }

    public void UpgradePlayer(
      Inventory inventory,
      CharacterClassManager player,
      IEnumerable<CharacterClassManager> players)
    {
      for (int index = inventory.items.Count - 1; index > -1; --index)
      {
        Inventory.SyncItemInfo syncItemInfo = inventory.items[index];
        ItemType itemID = this.UpgradeItemID(syncItemInfo.id);
        if (itemID < ItemType.KeycardJanitor)
        {
          inventory.items.RemoveAt(index);
        }
        else
        {
          syncItemInfo.id = itemID;
          inventory.items[index] = syncItemInfo;
          Scp914Machine.TryFriendshipAchievement(itemID, player, players);
        }
      }
    }

    private void UpgradeHeldItem(
      Inventory inventory,
      CharacterClassManager player,
      IEnumerable<CharacterClassManager> players)
    {
      if (inventory.curItem == ItemType.None)
        return;
      ItemType itemID = this.UpgradeItemID(inventory.curItem);
      int itemIndex = inventory.GetItemIndex();
      if (itemIndex < 0 || itemIndex >= inventory.items.Count)
        return;
      if (itemID == ItemType.None)
      {
        inventory.items.RemoveAt(itemIndex);
      }
      else
      {
        Inventory.SyncItemInfo syncItemInfo = inventory.items[itemIndex];
        syncItemInfo.id = itemID;
        inventory.items[itemIndex] = syncItemInfo;
        Scp914Machine.TryFriendshipAchievement(itemID, player, players);
      }
    }

    private void MirrorProcessed()
    {
    }

    public Scp914Knob NetworkknobState
    {
      get
      {
        return this.knobState;
      }
      [param: In] set
      {
        if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
        {
          this.setSyncVarHookGuard(1UL, true);
          this.SetKnobState(value);
          this.setSyncVarHookGuard(1UL, false);
        }
        this.SetSyncVar<Scp914Knob>(value, ref this.knobState, 1UL);
      }
    }

    protected static void InvokeRpcRpcActivate(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "RPC RpcActivate called on server.");
      else
        ((Scp914Machine) obj).CallRpcActivate(reader.ReadDouble());
    }

    public void CallRpcActivate(double time)
    {
      if (this.working)
        return;
      this.working = true;
      Timing.RunCoroutine(this._Animation(time), Segment.FixedUpdate);
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WritePackedInt32((int) this.knobState);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WritePackedInt32((int) this.knobState);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        Scp914Knob status = (Scp914Knob) reader.ReadPackedInt32();
        this.SetKnobState(status);
        this.NetworkknobState = status;
      }
      else
      {
        if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
          return;
        Scp914Knob status = (Scp914Knob) reader.ReadPackedInt32();
        this.SetKnobState(status);
        this.NetworkknobState = status;
      }
    }
  }
}
