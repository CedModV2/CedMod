// Decompiled with JetBrains decompiler
// Type: RoleExtensionMethods
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public static class RoleExtensionMethods
{
  public static Role Get(this Role[] klasy, int rtId)
  {
    return klasy[rtId];
  }

  public static Role Get(this Role[] klasy, RoleType rt)
  {
    return RoleExtensionMethods.Get(klasy, (int) rt);
  }

  public static Role SafeGet(this Role[] klasy, int rtId)
  {
    return klasy.CheckBounds(rtId) ? klasy[rtId] : klasy.Get(RoleType.Spectator);
  }

  public static Role SafeGet(this Role[] klasy, RoleType rt)
  {
    return klasy.SafeGet((int) rt);
  }

  public static bool TryGet(this Role[] klasy, int rtId, out Role role)
  {
    role = (Role) null;
    try
    {
      role = klasy[rtId];
      return true;
    }
    catch
    {
      return false;
    }
  }

  public static bool TryGet(this Role[] klasy, RoleType rt, out Role role)
  {
    return klasy.TryGet((int) rt, out role);
  }

  public static bool CheckBounds(this Role[] klasy, int rtId)
  {
    return rtId >= 0 && rtId < klasy.Length;
  }

  public static bool CheckBounds(this Role[] klasy, RoleType rt)
  {
    return klasy.CheckBounds((int) rt);
  }

  public static bool Is939(this RoleType roleType)
  {
    return roleType == RoleType.Scp93953 || roleType == RoleType.Scp93989;
  }
}
