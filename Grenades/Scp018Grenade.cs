// Decompiled with JetBrains decompiler
// Type: Grenades.Scp018Grenade
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

namespace Grenades
{
  public class Scp018Grenade : FragGrenade
  {
    private int layerGlass;
    private int layerDoor;
    private int layerHitbox;
    private int layerIgnoreRaycast;
    private int bounce;
    private float cooldown;
    private bool actionAllowed;
    private bool hasHitMaxSpeed;
    [Header("Speed")]
    public float[] topSpeedPerBounce;
    public float bounceSpeedMultiplier;
    [Header("Audio")]
    public AudioClip[] collisionSoundsLow;
    public AudioClip[] collisionSoundsMed;
    public AudioClip[] collisionSoundsFast;
    [Header("Speed Breakpoints")]
    public float breakpointGlass;
    public float breakpointDoor;
    public float breakpointHurt;
    [Header("Collision Cooldowns")]
    public float cooldownGlass;
    public float cooldownDoor;
    public float cooldownHurt;
    [Header("Damage Multipliers")]
    public float damageGlass;
    public float damageHurt;
    public float damageScpMultiplier;

    protected override void Awake()
    {
      base.Awake();
      this.layerGlass = LayerMask.GetMask("Glass");
      this.layerDoor = LayerMask.GetMask("Door");
      this.layerHitbox = LayerMask.GetMask("Hitbox");
      this.layerIgnoreRaycast = LayerMask.GetMask("Ignore Raycast");
      this.collisionSounds = this.collisionSoundsLow;
    }

    protected virtual void FixedUpdate()
    {
      this.cooldown = Mathf.Max(this.cooldown - Time.fixedDeltaTime, 0.0f);
      this.actionAllowed = (double) this.cooldown == 0.0;
    }

    protected override void OnSpeedCollisionEnter(Collision collision, float relativeSpeed)
    {
      Vector3 vector3 = this.rb.velocity * this.bounceSpeedMultiplier;
      float num1 = this.topSpeedPerBounce[this.bounce];
      if ((double) relativeSpeed > (double) num1)
      {
        this.rb.velocity = vector3.normalized * num1;
        if (this.actionAllowed)
          this.bounce = Mathf.Min(this.bounce + 1, this.topSpeedPerBounce.Length - 1);
      }
      else
      {
        if ((double) relativeSpeed > (double) this.source.maxDistance)
          this.source.maxDistance = relativeSpeed;
        this.rb.velocity = vector3;
      }
      if (NetworkServer.active)
      {
        Collider collider = collision.collider;
        int num2 = 1 << collider.gameObject.layer;
        if (num2 == this.layerGlass)
        {
          if (this.actionAllowed && (double) relativeSpeed >= (double) this.breakpointGlass)
          {
            this.cooldown = this.cooldownGlass;
            BreakableWindow component = collider.GetComponent<BreakableWindow>();
            if ((Object) component != (Object) null)
              component.ServerDamageWindow(relativeSpeed * this.damageGlass);
          }
        }
        else if (num2 == this.layerDoor)
        {
          if ((double) relativeSpeed >= (double) this.breakpointDoor)
          {
            this.cooldown = this.cooldownDoor;
            Door componentInParent = collider.GetComponentInParent<Door>();
            if ((Object) componentInParent != (Object) null && !componentInParent.GrenadesResistant)
              componentInParent.DestroyDoor(true);
          }
        }
        else if ((num2 == this.layerHitbox || num2 == this.layerIgnoreRaycast) && (this.actionAllowed && (double) relativeSpeed >= (double) this.breakpointHurt))
        {
          this.cooldown = this.cooldownHurt;
          ReferenceHub componentInParent = collider.GetComponentInParent<ReferenceHub>();
          if ((Object) componentInParent != (Object) null && (ServerConsole.FriendlyFire || (Object) componentInParent.gameObject == (Object) this.thrower.gameObject || componentInParent.weaponManager.GetShootPermission(this.throwerTeam, false)))
          {
            float amnt = relativeSpeed * this.damageHurt;
            if (componentInParent.playerStats.ccm.CurClass != RoleType.Scp106 && componentInParent.playerStats.ccm.Classes.SafeGet(componentInParent.playerStats.ccm.CurClass).team == Team.SCP)
              amnt *= this.damageScpMultiplier;
            componentInParent.playerStats.HurtPlayer(new PlayerStats.HitInfo(amnt, this.logName, DamageTypes.Wall, 0), componentInParent.playerStats.gameObject);
          }
        }
        if (this.bounce >= this.topSpeedPerBounce.Length - 1 && (double) relativeSpeed >= (double) num1 && !this.hasHitMaxSpeed)
        {
          this.NetworkfuseTime = NetworkTime.time + 10.0;
          this.hasHitMaxSpeed = true;
        }
      }
      base.OnSpeedCollisionEnter(collision, relativeSpeed);
    }

    private void MirrorProcessed()
    {
    }
  }
}
