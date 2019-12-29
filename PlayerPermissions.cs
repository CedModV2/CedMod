// Decompiled with JetBrains decompiler
// Type: PlayerPermissions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public enum PlayerPermissions : ulong
{
  KickingAndShortTermBanning = 1,
  BanningUpToDay = 2,
  LongTermBanning = 4,
  ForceclassSelf = 8,
  ForceclassToSpectator = 16, // 0x0000000000000010
  ForceclassWithoutRestrictions = 32, // 0x0000000000000020
  GivingItems = 64, // 0x0000000000000040
  WarheadEvents = 128, // 0x0000000000000080
  RespawnEvents = 256, // 0x0000000000000100
  RoundEvents = 512, // 0x0000000000000200
  SetGroup = 1024, // 0x0000000000000400
  GameplayData = 2048, // 0x0000000000000800
  Overwatch = 4096, // 0x0000000000001000
  FacilityManagement = 8192, // 0x0000000000002000
  PlayersManagement = 16384, // 0x0000000000004000
  PermissionsManagement = 32768, // 0x0000000000008000
  ServerConsoleCommands = 65536, // 0x0000000000010000
  ViewHiddenBadges = 131072, // 0x0000000000020000
  ServerConfigs = 262144, // 0x0000000000040000
  Broadcasting = 524288, // 0x0000000000080000
  PlayerSensitiveDataAccess = 1048576, // 0x0000000000100000
  Noclip = 2097152, // 0x0000000000200000
  AFKImmunity = 4194304, // 0x0000000000400000
}
