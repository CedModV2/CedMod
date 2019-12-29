// Decompiled with JetBrains decompiler
// Type: UserGroup
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

[Serializable]
public class UserGroup
{
  public string BadgeColor;
  public string BadgeText;
  public ulong Permissions;
  public bool Cover;
  public bool HiddenByDefault;
  public bool Shared;
  public byte KickPower;
  public byte RequiredKickPower;

  public UserGroup Clone()
  {
    return new UserGroup()
    {
      BadgeColor = this.BadgeColor,
      BadgeText = this.BadgeText,
      Permissions = this.Permissions,
      Cover = this.Cover,
      HiddenByDefault = this.HiddenByDefault,
      Shared = this.Shared,
      KickPower = this.KickPower,
      RequiredKickPower = this.RequiredKickPower
    };
  }
}
