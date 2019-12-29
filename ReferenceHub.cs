// Decompiled with JetBrains decompiler
// Type: ReferenceHub
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceHub : MonoBehaviour
{
  private static readonly Dictionary<GameObject, ReferenceHub> Hubs = new Dictionary<GameObject, ReferenceHub>(20, (IEqualityComparer<GameObject>) new ReferenceHub.GameObjectComparer());
  public CharacterClassManager characterClassManager;
  public PlayerStats playerStats;
  public Inventory inventory;
  public ServerRoles serverRoles;
  public QueryProcessor queryProcessor;
  public PlyMovementSync plyMovementSync;
  public NicknameSync nicknameSync;
  public SpectatorManager spectatorManager;
  public Scp079PlayerScript scp079PlayerScript;
  public WeaponManager weaponManager;
  public AnimationController animationController;
  public AmmoBox ammoBox;
  public Handcuffs handcuffs;
  public FallDamage falldamage;
  public PlayerInteract playerInteract;
  public PlayerEffectsController effectsController;
  public FootstepSync footstepSync;

  private void Awake()
  {
    if ((Object) this.characterClassManager == (Object) null)
      this.characterClassManager = this.GetComponent<CharacterClassManager>();
    if ((Object) this.playerStats == (Object) null)
      this.playerStats = this.GetComponent<PlayerStats>();
    if ((Object) this.inventory == (Object) null)
      this.inventory = this.GetComponent<Inventory>();
    if ((Object) this.serverRoles == (Object) null)
      this.serverRoles = this.GetComponent<ServerRoles>();
    if ((Object) this.queryProcessor == (Object) null)
      this.queryProcessor = this.GetComponent<QueryProcessor>();
    if ((Object) this.plyMovementSync == (Object) null)
      this.plyMovementSync = this.GetComponent<PlyMovementSync>();
    if ((Object) this.nicknameSync == (Object) null)
      this.nicknameSync = this.GetComponent<NicknameSync>();
    if ((Object) this.spectatorManager == (Object) null)
      this.spectatorManager = this.GetComponent<SpectatorManager>();
    if ((Object) this.scp079PlayerScript == (Object) null)
      this.scp079PlayerScript = this.GetComponent<Scp079PlayerScript>();
    if ((Object) this.weaponManager == (Object) null)
      this.weaponManager = this.GetComponent<WeaponManager>();
    if ((Object) this.animationController == (Object) null)
      this.animationController = this.GetComponent<AnimationController>();
    if ((Object) this.ammoBox == (Object) null)
      this.ammoBox = this.GetComponent<AmmoBox>();
    if ((Object) this.falldamage == (Object) null)
      this.falldamage = this.GetComponent<FallDamage>();
    if ((Object) this.handcuffs == (Object) null)
      this.handcuffs = this.GetComponent<Handcuffs>();
    if ((Object) this.playerInteract == (Object) null)
      this.playerInteract = this.GetComponent<PlayerInteract>();
    if ((Object) this.effectsController == (Object) null)
      this.effectsController = this.GetComponent<PlayerEffectsController>();
    if ((Object) this.footstepSync == (Object) null)
      this.footstepSync = this.GetComponent<FootstepSync>();
    ReferenceHub.Hubs[this.gameObject] = this;
  }

  private void OnDestroy()
  {
    ReferenceHub.Hubs.Remove(this.gameObject);
    RoleType curClass = this.gameObject.GetComponent<CharacterClassManager>().CurClass;
    if (curClass == RoleType.Spectator || !ConfigFile.ServerConfig.GetBool("neon_replacedisconnecteds", true))
      return;
    string data = ConfigFile.ServerConfig.GetString("neon_replace_message", "You've replaced a player that disconnected.");
    Inventory component = this.GetComponent<Inventory>();
    Vector3 realModelPosition = this.GetComponent<PlyMovementSync>().RealModelPosition;
    foreach (ReferenceHub referenceHub in ReferenceHub.Hubs.Values)
    {
      if (!((Object) PlayerManager.localPlayer == (Object) referenceHub.gameObject) && referenceHub.characterClassManager.CurClass == RoleType.Spectator)
      {
        referenceHub.characterClassManager.SetPlayersClass(curClass, referenceHub.gameObject, true, false);
        if (curClass == RoleType.Scp079)
          this.Copy079(referenceHub.scp079PlayerScript, this.GetComponent<Scp079PlayerScript>());
        Timing.RunCoroutine(this.ReallyFunCoroutine(realModelPosition, referenceHub.gameObject), 1);
        foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) component.items)
          referenceHub.inventory.AddNewItem(syncItemInfo.id, syncItemInfo.durability, syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther);
        PlayerManager.localPlayer.GetComponent<Broadcast>().TargetAddElement(referenceHub.characterClassManager.connectionToClient, data, 7U, false);
        break;
      }
    }
  }

  public static ReferenceHub GetHub(GameObject player)
  {
    ReferenceHub referenceHub;
    return !ReferenceHub.Hubs.TryGetValue(player, out referenceHub) ? player.GetComponent<ReferenceHub>() : referenceHub;
  }

  private void Copy079(Scp079PlayerScript dst, Scp079PlayerScript src)
  {
    dst.currentCamera = src.currentCamera;
    dst.currentRoom = src.currentRoom;
    dst.currentZone = src.currentZone;
    dst.lockedDoors = src.lockedDoors;
    dst.maxMana = src.maxMana;
    dst.nearbyInteractables = src.nearbyInteractables;
    dst.nearestCameras = src.nearestCameras;
    dst.NetworkcurExp = src.NetworkcurExp;
    dst.NetworkcurLvl = src.NetworkcurLvl;
    dst.NetworkcurMana = src.NetworkcurMana;
    dst.NetworkcurSpeaker = src.NetworkcurSpeaker;
    dst.NetworkmaxMana = src.NetworkmaxMana;
  }

  private IEnumerator<float> ReallyFunCoroutine(Vector3 pos, GameObject go)
  {
    yield return Timing.WaitForSeconds(0.3f);
    go.GetComponent<PlyMovementSync>().OverridePosition(pos, 0.0f, false);
  }

  private class GameObjectComparer : EqualityComparer<GameObject>
  {
    public override bool Equals(GameObject x, GameObject y)
    {
      return (Object) x == (Object) y;
    }

    public override int GetHashCode(GameObject obj)
    {
      return obj.GetHashCode();
    }
  }
}
