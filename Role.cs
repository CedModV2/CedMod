// Decompiled with JetBrains decompiler
// Type: Role
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

[Serializable]
public class Role
{
  public string fullName = "Chaos Insurgency";
  public Color classColor = Color.white;
  public RoleType roleId = RoleType.None;
  [Space]
  public int[] ammoTypes = new int[3]{ 100, 100, 100 };
  public int[] maxAmmo = new int[3]{ 160, 100, 200 };
  [Space]
  public int maxHP = 100;
  public float walkSpeed = 5f;
  public float runSpeed = 7f;
  public float jumpSpeed = 7f;
  public float classRecoil = 1f;
  public int forcedCrosshair = -1;
  public string nickname;
  [Multiline]
  public string description;
  [Multiline]
  public string bio;
  public Team team;
  public List<Ability> abilities;
  public Sprite profileSprite;
  public PostProcessingProfile postprocessingProfile;
  public GameObject model_player;
  public Offset model_offset;
  public GameObject model_ragdoll;
  public Offset ragdoll_offset;
  public ItemType[] startItems;
  [Space]
  public AudioClip[] stepClips;
  public bool banClass;
  public float iconHeightOffset;
  public bool useHeadBob;
  public int bloodType;
}
