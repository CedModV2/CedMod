// Decompiled with JetBrains decompiler
// Type: Grenades.GrenadeSettings
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace Grenades
{
  [Serializable]
  public class GrenadeSettings
  {
    public string apiName;
    public ItemType inventoryID;
    public float throwAnimationDuration;
    [Obsolete("See Grenade.fuseDuration instead. I just don't want to edit the player prefab.")]
    public float timeUnitilDetonation;
    [Obsolete("See Grenade.throwStartPositionOffset instead.")]
    public Vector3 startPointOffset;
    [Obsolete("See Grenade.throwStartRotation instead.")]
    public Vector3 startRotation;
    [Obsolete("See Grenade.throwAngularVelocity instead.")]
    public Vector3 angularVelocity;
    [Obsolete("See Grenade.throwForce instead.")]
    public float throwForce;
    public GameObject grenadeInstance;
  }
}
