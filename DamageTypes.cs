// Decompiled with JetBrains decompiler
// Type: DamageTypes
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public static class DamageTypes
{
  public static DamageTypes.DamageType None = new DamageTypes.DamageType("NONE", false, false, -1);
  public static readonly DamageTypes.DamageType Lure = new DamageTypes.DamageType("LURE", false, false, -1);
  public static readonly DamageTypes.DamageType Nuke = new DamageTypes.DamageType("NUKE", false, false, -1);
  public static readonly DamageTypes.DamageType Wall = new DamageTypes.DamageType("WALL", false, false, -1);
  public static readonly DamageTypes.DamageType Decont = new DamageTypes.DamageType("DECONT", false, false, -1);
  public static readonly DamageTypes.DamageType Tesla = new DamageTypes.DamageType("TESLA", false, false, -1);
  public static readonly DamageTypes.DamageType Falldown = new DamageTypes.DamageType("FALLDOWN", false, false, -1);
  public static readonly DamageTypes.DamageType Flying = new DamageTypes.DamageType("Flying detection", false, false, -1);
  public static readonly DamageTypes.DamageType Recontainment = new DamageTypes.DamageType("RECONTAINMENT", false, false, -1);
  public static readonly DamageTypes.DamageType Contain = new DamageTypes.DamageType("CONTAIN", false, false, -1);
  public static readonly DamageTypes.DamageType Pocket = new DamageTypes.DamageType("POCKET", false, false, -1);
  public static readonly DamageTypes.DamageType RagdollLess = new DamageTypes.DamageType("RAGDOLL-LESS", false, false, -1);
  public static readonly DamageTypes.DamageType Com15 = new DamageTypes.DamageType(nameof (Com15), true, false, 0);
  public static readonly DamageTypes.DamageType P90 = new DamageTypes.DamageType(nameof (P90), true, false, 1);
  public static readonly DamageTypes.DamageType E11StandardRifle = new DamageTypes.DamageType("E11 Standard Rifle", true, false, 2);
  public static readonly DamageTypes.DamageType Mp7 = new DamageTypes.DamageType("MP7", true, false, 3);
  public static readonly DamageTypes.DamageType Logicer = new DamageTypes.DamageType("Logicier", true, false, 4);
  public static readonly DamageTypes.DamageType Usp = new DamageTypes.DamageType("USP", true, false, 5);
  public static readonly DamageTypes.DamageType MicroHid = new DamageTypes.DamageType("MicroHID", false, false, -1);
  public static readonly DamageTypes.DamageType Grenade = new DamageTypes.DamageType("GRENADE", false, false, -1);
  public static readonly DamageTypes.DamageType Scp049 = new DamageTypes.DamageType("SCP-049", false, true, -1);
  public static readonly DamageTypes.DamageType Scp0492 = new DamageTypes.DamageType("SCP-049-2", false, true, -1);
  public static readonly DamageTypes.DamageType Scp096 = new DamageTypes.DamageType("SCP-096", false, true, -1);
  public static readonly DamageTypes.DamageType Scp106 = new DamageTypes.DamageType("SCP-106", false, true, -1);
  public static readonly DamageTypes.DamageType Scp173 = new DamageTypes.DamageType("SCP-173", false, true, -1);
  public static readonly DamageTypes.DamageType Scp939 = new DamageTypes.DamageType("SCP-939", false, true, -1);
  public static readonly DamageTypes.DamageType Scp207 = new DamageTypes.DamageType("SCP-207", false, true, -1);
  private static readonly DamageTypes.DamageType[] damageTypes = new DamageTypes.DamageType[27]
  {
    DamageTypes.None,
    DamageTypes.Lure,
    DamageTypes.Nuke,
    DamageTypes.Wall,
    DamageTypes.Decont,
    DamageTypes.Tesla,
    DamageTypes.Falldown,
    DamageTypes.Flying,
    DamageTypes.Contain,
    DamageTypes.Pocket,
    DamageTypes.RagdollLess,
    DamageTypes.Com15,
    DamageTypes.P90,
    DamageTypes.E11StandardRifle,
    DamageTypes.Mp7,
    DamageTypes.Logicer,
    DamageTypes.Usp,
    DamageTypes.MicroHid,
    DamageTypes.Grenade,
    DamageTypes.Scp049,
    DamageTypes.Scp0492,
    DamageTypes.Scp096,
    DamageTypes.Scp106,
    DamageTypes.Scp173,
    DamageTypes.Scp939,
    DamageTypes.Scp207,
    DamageTypes.Recontainment
  };

  public static DamageTypes.DamageType FromIndex(int id)
  {
    return id >= 0 && id < DamageTypes.damageTypes.Length ? DamageTypes.damageTypes[id] : DamageTypes.None;
  }

  public static int ToIndex(DamageTypes.DamageType damageType)
  {
    for (int index = 0; index < DamageTypes.damageTypes.Length; ++index)
    {
      if (DamageTypes.damageTypes[index] == damageType)
        return index;
    }
    return 0;
  }

  public static DamageTypes.DamageType FromWeaponId(int weaponId)
  {
    foreach (DamageTypes.DamageType damageType in DamageTypes.damageTypes)
    {
      if (damageType.isWeapon && damageType.weaponId == weaponId)
        return damageType;
    }
    return DamageTypes.None;
  }

  public class DamageType
  {
    public readonly string name;
    public readonly bool isWeapon;
    public readonly bool isScp;
    public readonly int weaponId;

    public DamageType(string name, bool weapon = false, bool scp = false, int weaponId = -1)
    {
      this.name = name;
      this.isWeapon = weapon;
      this.isScp = scp;
      this.weaponId = weaponId;
    }
  }
}
