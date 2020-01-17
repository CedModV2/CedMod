// Mirror.GeneratedNetworkCode
using Grenades;
using Mirror;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class GeneratedNetworkCode
{
	public static void _WriteSyncItemInfo_Inventory(NetworkWriter writer, Inventory.SyncItemInfo value)
	{
		writer.WritePackedInt32((int)value.id);
		writer.WriteSingle(value.durability);
		writer.WritePackedInt32(value.uniq);
		writer.WritePackedInt32(value.modSight);
		writer.WritePackedInt32(value.modBarrel);
		writer.WritePackedInt32(value.modOther);
	}

	public static Inventory.SyncItemInfo _ReadSyncItemInfo_Inventory(NetworkReader reader)
	{
		Inventory.SyncItemInfo result = default(Inventory.SyncItemInfo);
		result.id = (ItemType)reader.ReadPackedInt32();
		result.durability = reader.ReadSingle();
		result.uniq = reader.ReadPackedInt32();
		result.modSight = reader.ReadPackedInt32();
		result.modBarrel = reader.ReadPackedInt32();
		result.modOther = reader.ReadPackedInt32();
		return result;
	}

	public static void _WriteBreakableWindowStatus_BreakableWindow(NetworkWriter writer, BreakableWindow.BreakableWindowStatus value)
	{
		writer.WriteVector3(value.position);
		writer.WriteQuaternion(value.rotation);
		writer.WriteBoolean(value.broken);
	}

	public static BreakableWindow.BreakableWindowStatus _ReadBreakableWindowStatus_BreakableWindow(NetworkReader reader)
	{
		BreakableWindow.BreakableWindowStatus result = default(BreakableWindow.BreakableWindowStatus);
		result.position = reader.ReadVector3();
		result.rotation = reader.ReadQuaternion();
		result.broken = reader.ReadBoolean();
		return result;
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
		PlayerStats.HitInfo result = default(PlayerStats.HitInfo);
		result.Amount = reader.ReadSingle();
		result.Tool = reader.ReadPackedInt32();
		result.Time = reader.ReadPackedInt32();
		result.Attacker = reader.ReadString();
		result.PlyId = reader.ReadPackedInt32();
		return result;
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.WritePackedInt32(-1);
			return;
		}
		int i = value.Length;
		writer.WritePackedInt32(i);
		for (int j = 0; j < value.Length; j++)
		{
			writer.WritePackedInt32(value[j]);
		}
	}

	public static void _WritePickupInfo_Pickup(NetworkWriter writer, Pickup.PickupInfo value)
	{
		writer.WriteVector3(value.position);
		writer.WriteQuaternion(value.rotation);
		writer.WritePackedInt32((int)value.itemId);
		writer.WriteSingle(value.durability);
		writer.WriteGameObject(value.ownerPlayer);
		_WriteArrayInt32_None(writer, value.weaponMods);
		writer.WriteBoolean(value.locked);
	}

	public static int[] _ReadArrayInt32_None(NetworkReader reader)
	{
		int num = reader.ReadPackedInt32();
		if (num < 0)
		{
			return null;
		}
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = reader.ReadPackedInt32();
		}
		return array;
	}

	public static Pickup.PickupInfo _ReadPickupInfo_Pickup(NetworkReader reader)
	{
		Pickup.PickupInfo result = default(Pickup.PickupInfo);
		result.position = reader.ReadVector3();
		result.rotation = reader.ReadQuaternion();
		result.itemId = (ItemType)reader.ReadPackedInt32();
		result.durability = reader.ReadSingle();
		result.ownerPlayer = reader.ReadGameObject();
		result.weaponMods = _ReadArrayInt32_None(reader);
		result.locked = reader.ReadBoolean();
		return result;
	}

	public static void _WriteInfo_Ragdoll(NetworkWriter writer, Ragdoll.Info value)
	{
		writer.WriteString(value.ownerHLAPI_id);
		writer.WriteString(value.DeathCauseText);
		_WriteHitInfo_PlayerStats(writer, value.DeathCause);
		writer.WritePackedInt32(value.PlayerId);
	}

	public static Ragdoll.Info _ReadInfo_Ragdoll(NetworkReader reader)
	{
		Ragdoll.Info result = default(Ragdoll.Info);
		result.ownerHLAPI_id = reader.ReadString();
		result.DeathCauseText = reader.ReadString();
		result.DeathCause = _ReadHitInfo_PlayerStats(reader);
		result.PlayerId = reader.ReadPackedInt32();
		return result;
	}

	public static void _WriteSumInfo_ClassList_RoundSummary(NetworkWriter writer, RoundSummary.SumInfo_ClassList value)
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

	public static RoundSummary.SumInfo_ClassList _ReadSumInfo_ClassList_RoundSummary(NetworkReader reader)
	{
		RoundSummary.SumInfo_ClassList result = default(RoundSummary.SumInfo_ClassList);
		result.class_ds = reader.ReadPackedInt32();
		result.scientists = reader.ReadPackedInt32();
		result.chaos_insurgents = reader.ReadPackedInt32();
		result.mtf_and_guards = reader.ReadPackedInt32();
		result.scps_except_zombies = reader.ReadPackedInt32();
		result.zombies = reader.ReadPackedInt32();
		result.warhead_kills = reader.ReadPackedInt32();
		result.time = reader.ReadPackedInt32();
		return result;
	}

	public static void _WriteOffset_None(NetworkWriter writer, Offset value)
	{
		writer.WriteVector3(value.position);
		writer.WriteVector3(value.rotation);
		writer.WriteVector3(value.scale);
	}

	public static Offset _ReadOffset_None(NetworkReader reader)
	{
		Offset result = default(Offset);
		result.position = reader.ReadVector3();
		result.rotation = reader.ReadVector3();
		result.scale = reader.ReadVector3();
		return result;
	}

	public static void _WriteRigidbodyVelocityPair_None(NetworkWriter writer, RigidbodyVelocityPair value)
	{
		writer.WriteVector3(value.linear);
		writer.WriteVector3(value.angular);
	}

	public static RigidbodyVelocityPair _ReadRigidbodyVelocityPair_None(NetworkReader reader)
	{
		RigidbodyVelocityPair result = default(RigidbodyVelocityPair);
		result.linear = reader.ReadVector3();
		result.angular = reader.ReadVector3();
		return result;
	}
}
