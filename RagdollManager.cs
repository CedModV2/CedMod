// Decompiled with JetBrains decompiler
// Type: RagdollManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

public class RagdollManager : NetworkBehaviour
{
  public LayerMask inspectionMask;
  private Transform cam;
  private CharacterClassManager ccm;

  public void SpawnRagdoll(
    Vector3 pos,
    Quaternion rot,
    int classId,
    PlayerStats.HitInfo ragdollInfo,
    bool allowRecall,
    string ownerID,
    string ownerNick,
    int playerId)
  {
    Role c = this.ccm.Classes.SafeGet(classId);
    if ((Object) c.model_ragdoll != (Object) null)
    {
      GameObject gameObject = Object.Instantiate<GameObject>(c.model_ragdoll, pos + c.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + c.ragdoll_offset.rotation));
      NetworkServer.Spawn(gameObject);
      gameObject.GetComponent<Ragdoll>().Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, c, playerId);
      gameObject.GetComponent<Ragdoll>().NetworkallowRecall = allowRecall;
    }
    if (ragdollInfo.GetDamageType().isScp || ragdollInfo.GetDamageType() == DamageTypes.Pocket)
    {
      this.RegisterScpFrag();
    }
    else
    {
      if (ragdollInfo.GetDamageType() != DamageTypes.Grenade)
        return;
      ++RoundSummary.kills_by_frag;
    }
  }

  private void Start()
  {
    this.cam = this.GetComponent<Scp049PlayerScript>().plyCam.transform;
    this.ccm = this.GetComponent<CharacterClassManager>();
  }

  public void Update()
  {
  }

  public static string GetColor(Color c)
  {
    Color32 color32 = new Color32((byte) ((double) c.r * (double) byte.MaxValue), (byte) ((double) c.g * (double) byte.MaxValue), (byte) ((double) c.b * (double) byte.MaxValue), byte.MaxValue);
    return "#" + color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2");
  }

  public void RegisterScpFrag()
  {
    ++RoundSummary.kills_by_scp;
  }

  public static string GetCause(PlayerStats.HitInfo info, bool ragdoll)
  {
    string formatted = TranslationReader.Get("Death_Causes", 11, "NO_TRANSLATION");
    if (info.GetDamageType() == DamageTypes.Nuke)
      formatted = TranslationReader.Get("Death_Causes", 0, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Falldown)
      formatted = TranslationReader.Get("Death_Causes", 1, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Lure)
      formatted = TranslationReader.Get("Death_Causes", 2, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Pocket)
      formatted = TranslationReader.Get("Death_Causes", 3, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Contain)
      formatted = TranslationReader.Get("Death_Causes", 4, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Tesla || info.GetDamageType() == DamageTypes.MicroHid)
      formatted = TranslationReader.Get("Death_Causes", 5, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Wall)
      formatted = TranslationReader.Get("Death_Causes", 6, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Decont)
      formatted = TranslationReader.Get("Death_Causes", 15, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Grenade)
      formatted = TranslationReader.Get("Death_Causes", 16, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Scp207)
      formatted = TranslationReader.Get("Death_Causes", 17, "NO_TRANSLATION");
    else if (info.GetDamageType() == DamageTypes.Recontainment)
      formatted = TranslationReader.Get("Death_Causes", 18, "NO_TRANSLATION");
    else if (info.GetDamageType().isWeapon && info.GetDamageType().weaponId != -1)
    {
      GameObject gameObject = GameObject.Find("Host");
      formatted = TranslationReader.GetFormatted("Death_Causes", 7, "", (object) gameObject.GetComponent<AmmoBox>().types[gameObject.GetComponent<WeaponManager>().weapons[info.GetDamageType().weaponId].ammoType].label);
    }
    else if (info.GetDamageType().isScp)
    {
      if (info.GetDamageType() == DamageTypes.Scp173)
        formatted = TranslationReader.Get("Death_Causes", 8, "NO_TRANSLATION");
      else if (info.GetDamageType() == DamageTypes.Scp106)
        formatted = TranslationReader.Get("Death_Causes", 9, "NO_TRANSLATION");
      else if (info.GetDamageType() == DamageTypes.Scp096)
        formatted = TranslationReader.Get("Death_Causes", 13, "NO_TRANSLATION");
      else if (info.GetDamageType() == DamageTypes.Scp049 || info.GetDamageType() == DamageTypes.Scp0492)
        formatted = TranslationReader.Get("Death_Causes", 10, "NO_TRANSLATION");
      else if (info.GetDamageType() == DamageTypes.Scp939)
        formatted = TranslationReader.Get("Death_Causes", 14, "NO_TRANSLATION");
    }
    else if (info.Attacker != null && info.Attacker.StartsWith("*"))
      return info.Attacker.Substring(1);
    return formatted;
  }

  private void MirrorProcessed()
  {
  }
}
