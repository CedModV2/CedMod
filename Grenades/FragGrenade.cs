// Decompiled with JetBrains decompiler
// Type: Grenades.FragGrenade
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using System;
using UnityEngine;

namespace Grenades
{
  public class FragGrenade : EffectGrenade
  {
    public float absoluteDamageFalloff = 5f;
    [Header("Frag Chains")]
    [SerializeField]
    private float chainTriggerRadius = 8f;
    public Vector3 chainMovement = Vector3.up / 3f;
    public float chainSpeed = 16f;
    public int chainConcurrencyLimit = 10;
    public int chainLengthLimit = 4;
    [NonSerialized]
    private float sqrChainTriggerRadius;
    [Header("Frag Effect")]
    public LayerMask hurtLayerMask;
    public LayerMask damageLayerMask;
    public AnimationCurve damageOverDistance;
    public int[] chainSupportedGrenades;

    public float ChainTriggerRadius
    {
      get
      {
        return this.chainTriggerRadius;
      }
      set
      {
        this.sqrChainTriggerRadius = value * value;
        this.chainTriggerRadius = value;
      }
    }

    protected override void Awake()
    {
      base.Awake();
      if (!NetworkServer.active)
        return;
      this.ChainTriggerRadius = this.chainTriggerRadius;
      this.chainConcurrencyLimit = ConfigFile.ServerConfig.GetInt("grenade_chain_limit", this.chainConcurrencyLimit);
      this.chainLengthLimit = ConfigFile.ServerConfig.GetInt("grenade_chain_length_limit", this.chainLengthLimit);
      this.hurtLayerMask = (LayerMask) ~(int) this.hurtLayerMask;
    }

    public override bool ServersideExplosion()
    {
      bool flag = base.ServersideExplosion();
      Vector3 position = this.transform.position;
      int num = 0;
      foreach (Collider collider in Physics.OverlapSphere(position, this.chainTriggerRadius, (int) this.damageLayerMask))
      {
        BreakableWindow component = collider.GetComponent<BreakableWindow>();
        Vector3 vector3;
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          vector3 = component.transform.position - position;
          if ((double) vector3.sqrMagnitude <= (double) this.sqrChainTriggerRadius)
            component.ServerDamageWindow(500f);
        }
        else
        {
          Door componentInParent = collider.GetComponentInParent<Door>();
          if ((UnityEngine.Object) componentInParent != (UnityEngine.Object) null)
          {
            if (!componentInParent.GrenadesResistant && !componentInParent.commandlock && (!componentInParent.decontlock && !componentInParent.lockdown))
            {
              vector3 = componentInParent.transform.position - position;
              if ((double) vector3.sqrMagnitude <= (double) this.sqrChainTriggerRadius)
                componentInParent.DestroyDoor(true);
            }
          }
          else if ((this.chainLengthLimit == -1 || this.chainLengthLimit > this.currentChainLength) && (this.chainConcurrencyLimit == -1 || this.chainConcurrencyLimit > num))
          {
            Pickup componentInChildren = collider.GetComponentInChildren<Pickup>();
            if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null && this.ChangeIntoGrenade(componentInChildren))
              ++num;
          }
        }
      }
      foreach (GameObject player in PlayerManager.players)
      {
        if (ServerConsole.FriendlyFire || !((UnityEngine.Object) player != (UnityEngine.Object) this.thrower.gameObject) || player.GetComponent<WeaponManager>().GetShootPermission(this.throwerTeam, false))
        {
          PlayerStats component = player.GetComponent<PlayerStats>();
          if (!((UnityEngine.Object) component == (UnityEngine.Object) null) && component.ccm.InWorld)
          {
            float amnt = this.damageOverDistance.Evaluate(Vector3.Distance(position, component.transform.position)) * (component.ccm.IsHuman() ? ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f) : ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f));
            if ((double) amnt > (double) this.absoluteDamageFalloff)
            {
              foreach (Transform grenadePoint in component.grenadePoints)
              {
                if (!Physics.Linecast(position, grenadePoint.position, (int) this.hurtLayerMask))
                {
                  component.HurtPlayer(new PlayerStats.HitInfo(amnt, (UnityEngine.Object) this.thrower != (UnityEngine.Object) null ? this.thrower.nick.MyNick + " (" + this.thrower.ccm.UserId + ")" : "(UNKNOWN)", DamageTypes.Grenade, this.thrower.query.PlayerId), player);
                  break;
                }
              }
            }
          }
        }
      }
      return flag;
    }

    [Server]
    private bool ChangeIntoGrenade(Pickup item)
    {
      if (!NetworkServer.active)
      {
        Debug.LogWarning((object) "[Server] function 'System.Boolean Grenades.FragGrenade::ChangeIntoGrenade(Pickup)' called on client");
        return false;
      }
      GrenadeSettings grenadeSettings = (GrenadeSettings) null;
      for (int index = 0; index < this.thrower.availableGrenades.Length; ++index)
      {
        GrenadeSettings availableGrenade = this.thrower.availableGrenades[index];
        if (availableGrenade.inventoryID == item.ItemId)
        {
          if (!this.chainSupportedGrenades.Contains<int>(index))
            return false;
          grenadeSettings = availableGrenade;
          break;
        }
      }
      if (grenadeSettings == null)
        return false;
      Transform transform = item.transform;
      Grenade component = UnityEngine.Object.Instantiate<GameObject>(grenadeSettings.grenadeInstance, transform.position, transform.rotation).GetComponent<Grenade>();
      component.InitData(this, item);
      NetworkServer.Spawn(component.gameObject);
      item.Delete();
      return true;
    }

    private void MirrorProcessed()
    {
    }
  }
}
