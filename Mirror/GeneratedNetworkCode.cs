// Decompiled with JetBrains decompiler
// Type: Mirror.GeneratedNetworkCode
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Grenades;
using System.Runtime.InteropServices;

namespace Mirror
{
  [StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
  public class GeneratedNetworkCode
  {
    public static void _WriteSyncItemInfo_Inventory(
      NetworkWriter writer,
      Inventory.SyncItemInfo value)
    {
      writer.WritePackedInt32((int) value.id);
      writer.WriteSingle(value.durability);
      writer.WritePackedInt32(value.uniq);
      writer.WritePackedInt32(value.modSight);
      writer.WritePackedInt32(value.modBarrel);
      writer.WritePackedInt32(value.modOther);
    }

    public static Inventory.SyncItemInfo _ReadSyncItemInfo_Inventory(NetworkReader reader)
    {
      return new Inventory.SyncItemInfo()
      {
        id = (ItemType) reader.ReadPackedInt32(),
        durability = reader.ReadSingle(),
        uniq = reader.ReadPackedInt32(),
        modSight = reader.ReadPackedInt32(),
        modBarrel = reader.ReadPackedInt32(),
        modOther = reader.ReadPackedInt32()
      };
    }

    public static void _WriteBreakableWindowStatus_BreakableWindow(
      NetworkWriter writer,
      BreakableWindow.BreakableWindowStatus value)
    {
      writer.WriteVector3(value.position);
      writer.WriteQuaternion(value.rotation);
      writer.WriteBoolean(value.broken);
    }

    public static BreakableWindow.BreakableWindowStatus _ReadBreakableWindowStatus_BreakableWindow(
      NetworkReader reader)
    {
      return new BreakableWindow.BreakableWindowStatus()
      {
        position = reader.ReadVector3(),
        rotation = reader.ReadQuaternion(),
        broken = reader.ReadBoolean()
      };
    }

    public static void _WriteHitInfo_PlayerStats(NetworkWriter writer, PlayerStats.HitInfo value)
    {
      writer.WriteSingle(value.Amount);
      writer.WritePackedInt32(value.Tool);
      writer.WritePackedInt32(value.Time);
      writer.WriteString(value.Attacker);
      writer.WritePackedInt32(value.PlyId);
    }

    public static PlayerStats.HitInfo _ReadHitInfo_PlayerStats(NetworkReader reader)
    {
      return new PlayerStats.HitInfo()
      {
        Amount = reader.ReadSingle(),
        Tool = reader.ReadPackedInt32(),
        Time = reader.ReadPackedInt32(),
        Attacker = reader.ReadString(),
        PlyId = reader.ReadPackedInt32()
      };
    }

    public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
    {
      if (value == null)
      {
        writer.WritePackedInt32(-1);
      }
      else
      {
        int length = value.Length;
        writer.WritePackedInt32(length);
        for (int index = 0; index < value.Length; ++index)
          writer.WritePackedInt32(value[index]);
      }
    }

    public static void _WritePickupInfo_Pickup(NetworkWriter writer, Pickup.PickupInfo value)
    {
      writer.WriteVector3(value.position);
      writer.WriteQuaternion(value.rotation);
      writer.WritePackedInt32((int) value.itemId);
      writer.WriteSingle(value.durability);
      writer.WriteGameObject(value.ownerPlayer);
      GeneratedNetworkCode._WriteArrayInt32_None(writer, value.weaponMods);
      writer.WriteBoolean(value.locked);
    }

    public static int[] _ReadArrayInt32_None(NetworkReader reader)
    {
      int length = reader.ReadPackedInt32();
      if (length < 0)
        return (int[]) null;
      int[] numArray = new int[length];
      for (int index = 0; index < length; ++index)
        numArray[index] = reader.ReadPackedInt32();
      return numArray;
    }

    public static Pickup.PickupInfo _ReadPickupInfo_Pickup(NetworkReader reader)
    {
      return new Pickup.PickupInfo()
      {
        position = reader.ReadVector3(),
        rotation = reader.ReadQuaternion(),
        itemId = (ItemType) reader.ReadPackedInt32(),
        durability = reader.ReadSingle(),
        ownerPlayer = reader.ReadGameObject(),
        weaponMods = GeneratedNetworkCode._ReadArrayInt32_None(reader),
        locked = reader.ReadBoolean()
      };
    }

    public static void _WriteInfo_Ragdoll(NetworkWriter writer, Ragdoll.Info value)
    {
      writer.WriteString(value.ownerHLAPI_id);
      writer.WriteString(value.DeathCauseText);
      GeneratedNetworkCode._WriteHitInfo_PlayerStats(writer, value.DeathCause);
      writer.WritePackedInt32(value.PlayerId);
    }

    public static Ragdoll.Info _ReadInfo_Ragdoll(NetworkReader reader)
    {
      return new Ragdoll.Info()
      {
        ownerHLAPI_id = reader.ReadString(),
        DeathCauseText = reader.ReadString(),
        DeathCause = GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader),
        PlayerId = reader.ReadPackedInt32()
      };
    }

    public static void _WriteSumInfo_ClassList_RoundSummary(
      NetworkWriter writer,
      RoundSummary.SumInfo_ClassList value)
    {
      writer.WritePackedInt32(value.class_ds);
      writer.WritePackedInt32(value.scientists);
      writer.WritePackedInt32(value.chaos_insurgents);
      writer.WritePackedInt32(value.mtf_and_guards);
      writer.WritePackedInt32(value.scps_except_zombies);
      writer.WritePackedInt32(value.zombies);
      writer.WritePackedInt32(value.warhead_kills);
      writer.WritePackedInt32(value.time);
    }

    public static RoundSummary.SumInfo_ClassList _ReadSumInfo_ClassList_RoundSummary(
      NetworkReader reader)
    {
      return new RoundSummary.SumInfo_ClassList()
      {
        class_ds = reader.ReadPackedInt32(),
        scientists = reader.ReadPackedInt32(),
        chaos_insurgents = reader.ReadPackedInt32(),
        mtf_and_guards = reader.ReadPackedInt32(),
        scps_except_zombies = reader.ReadPackedInt32(),
        zombies = reader.ReadPackedInt32(),
        warhead_kills = reader.ReadPackedInt32(),
        time = reader.ReadPackedInt32()
      };
    }

    public static void _WriteOffset_None(NetworkWriter writer, Offset value)
    {
      writer.WriteVector3(value.position);
      writer.WriteVector3(value.rotation);
      writer.WriteVector3(value.scale);
    }

    public static Offset _ReadOffset_None(NetworkReader reader)
    {
      return new Offset()
      {
        position = reader.ReadVector3(),
        rotation = reader.ReadVector3(),
        scale = reader.ReadVector3()
      };
    }

    public static void _WriteRigidbodyVelocityPair_None(
      NetworkWriter writer,
      RigidbodyVelocityPair value)
    {
      writer.WriteVector3(value.linear);
      writer.WriteVector3(value.angular);
    }

    public static RigidbodyVelocityPair _ReadRigidbodyVelocityPair_None(
      NetworkReader reader)
    {
      return new RigidbodyVelocityPair()
      {
        linear = reader.ReadVector3(),
        angular = reader.ReadVector3()
      };
    }
  }
}
