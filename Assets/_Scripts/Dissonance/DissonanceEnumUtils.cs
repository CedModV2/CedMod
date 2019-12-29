// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.DissonanceEnumUtils
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Assets._Scripts.Dissonance
{
  public static class DissonanceEnumUtils
  {
    public static string TriggerTypeToString(this TriggerType type)
    {
      switch (type)
      {
        case TriggerType.Proximity:
          return "Proximity";
        case TriggerType.Role:
          return "Role";
        case TriggerType.Intercom:
          return "Intercom";
        default:
          return (string) null;
      }
    }

    public static string RoleTypeToString(this RoleType type)
    {
      if (type == RoleType.Ghost)
        return "Ghost";
      return type == RoleType.SCP ? "SCP" : (string) null;
    }

    public static bool HasFlagNonAlloc(this SpeakingFlags value, SpeakingFlags check)
    {
      return (value & check) == check;
    }
  }
}
