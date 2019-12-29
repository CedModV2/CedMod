// Decompiled with JetBrains decompiler
// Type: PermissionsHandler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;

public class PermissionsHandler
{
  private readonly string _overridePassword;
  private readonly string _overrideRole;
  private readonly Dictionary<string, UserGroup> _groups;
  private readonly Dictionary<string, string> _members;
  private readonly Dictionary<string, ulong> _permissions;
  private readonly HashSet<ulong> _raPermissions;
  private readonly YamlConfig _config;
  private readonly YamlConfig _sharedGroups;
  private readonly YamlConfig _sharedGroupsMembers;
  private ulong _lastPerm;
  private readonly bool _managerAccess;
  private readonly bool _banTeamAccess;
  private readonly bool _banTeamSlots;
  private readonly bool _banTeamGeoBypass;

  public PermissionsHandler(
    ref YamlConfig configuration,
    ref YamlConfig sharedGroups,
    ref YamlConfig sharedGroupsMembers)
  {
    this._config = new YamlConfig(configuration.Path);
    this._sharedGroups = sharedGroups;
    this._sharedGroupsMembers = sharedGroupsMembers;
    this._overridePassword = configuration.GetString("override_password", "none");
    this._overrideRole = configuration.GetString("override_password_role", "owner");
    this.StaffAccess = configuration.GetBool("enable_staff_access", false);
    this._managerAccess = configuration.GetBool("enable_manager_access", true);
    this._banTeamAccess = configuration.GetBool("enable_banteam_access", true);
    this._banTeamSlots = configuration.GetBool("enable_banteam_reserved_slots", true);
    this._banTeamGeoBypass = configuration.GetBool("enable_banteam_bypass_geoblocking", true);
    this._groups = new Dictionary<string, UserGroup>();
    this._raPermissions = new HashSet<ulong>();
    List<string> stringList1 = configuration.GetStringList("Roles");
    List<string> stringList2 = configuration.GetStringList("Roles");
    if (sharedGroups != null)
    {
      List<string> stringList3 = sharedGroups.GetStringList("SharedRoles");
      string str = ConfigFile.SharingConfig.GetString("groups_sharing_mode", "");
      if (!(str == "all"))
      {
        if (!(str == "opt-in"))
        {
          if (str == "opt-out")
          {
            List<string> optOut = ConfigFile.SharingConfig.GetStringList("groups_opt_out_list");
            stringList1.AddRange(stringList3.Where<string>((Func<string, bool>) (group => !optOut.Contains(group))));
          }
          else
            ServerConsole.AddLog("Invalid group sharing mode set!");
        }
        else
        {
          List<string> optIn = ConfigFile.SharingConfig.GetStringList("groups_opt_in_list");
          stringList1.AddRange(stringList3.Where<string>((Func<string, bool>) (group => optIn.Contains(group))));
        }
      }
      else
        stringList1.AddRange((IEnumerable<string>) stringList3);
    }
    string[] array = configuration.GetKeys().ToArray<string>();
    foreach (string key in stringList1)
    {
      string str1 = array.Contains<string>(key + "_badge") || sharedGroups == null ? configuration.GetString(key + "_badge", "") : sharedGroups.GetString(key + "_badge", "");
      string str2 = array.Contains<string>(key + "_color") || sharedGroups == null ? configuration.GetString(key + "_color", "") : sharedGroups.GetString(key + "_color", "");
      bool flag1 = array.Contains<string>(key + "_cover") || sharedGroups == null ? configuration.GetBool(key + "_cover", true) : sharedGroups.GetBool(key + "_cover", true);
      bool flag2 = array.Contains<string>(key + "_hidden") || sharedGroups == null ? configuration.GetBool(key + "_hidden", false) : sharedGroups.GetBool(key + "_hidden", false);
      byte num1 = array.Contains<string>(key + "_kick_power") || sharedGroups == null ? configuration.GetByte(key + "_kick_power", (byte) 0) : sharedGroups.GetByte(key + "_kick_power", (byte) 0);
      byte num2 = array.Contains<string>(key + "_required_kick_power") || sharedGroups == null ? configuration.GetByte(key + "_required_kick_power", (byte) 0) : sharedGroups.GetByte(key + "_required_kick_power", (byte) 0);
      if (!(str1 == "") && !(str2 == ""))
      {
        if (this._groups.ContainsKey(key))
          ServerConsole.AddLog("Duplicated group definition: " + key + ".");
        else
          this._groups.Add(key, new UserGroup()
          {
            BadgeColor = str2,
            BadgeText = str1,
            Permissions = 0UL,
            Cover = flag1,
            HiddenByDefault = flag2,
            Shared = !stringList2.Contains(key),
            KickPower = num1,
            RequiredKickPower = num2
          });
      }
    }
    this._members = configuration.GetStringDictionary("Members");
    Dictionary<string, string> stringDictionary = this._sharedGroupsMembers?.GetStringDictionary("SharedMembers");
    if (stringDictionary != null)
    {
      foreach (KeyValuePair<string, string> keyValuePair in stringDictionary)
      {
        if (this._members.ContainsKey(keyValuePair.Key))
          ServerConsole.AddLog("Duplicated group member: " + keyValuePair.Key + ". Is member of " + this._members[keyValuePair.Key] + " and " + keyValuePair.Value + ".");
        else
          this._members.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }
    this._lastPerm = 1UL;
    HashSet<string> stringSet = new HashSet<string>();
    if (this._members != null)
    {
      foreach (KeyValuePair<string, string> member in this._members)
      {
        if (!this._groups.ContainsKey(member.Value))
          stringSet.Add(member.Key);
      }
    }
    if (stringSet.Count > 0 && this._members != null)
    {
      foreach (string key in stringSet)
        this._members.Remove(key);
    }
    stringSet.Clear();
    this._permissions = new Dictionary<string, ulong>();
    foreach (string name in Enum.GetNames(typeof (PlayerPermissions)))
    {
      ulong num = (ulong) Enum.Parse(typeof (PlayerPermissions), name);
      this.FullPerm |= num;
      this._permissions.Add(name, num);
      if (num != 4096UL && num != 131072UL && (num != 2097152UL && num != 4194304UL))
        this._raPermissions.Add(num);
      if (num > this._lastPerm)
        this._lastPerm = num;
    }
    this.RefreshPermissions();
  }

  public ulong RegisterPermission(string name, bool remoteAdmin, bool refresh = true)
  {
    this._lastPerm = (ulong) Math.Pow(2.0, Math.Log((double) this._lastPerm, 2.0) + 1.0);
    this.FullPerm |= this._lastPerm;
    this._permissions.Add(name, this._lastPerm);
    if (remoteAdmin)
      this._raPermissions.Add(this._lastPerm);
    if (refresh)
      this.RefreshPermissions();
    return this._lastPerm;
  }

  public void RefreshPermissions()
  {
    foreach (KeyValuePair<string, UserGroup> group in this._groups)
      group.Value.Permissions = 0UL;
    Dictionary<string, string> stringDictionary1 = this._config.GetStringDictionary("Permissions");
    Dictionary<string, string> stringDictionary2 = this._sharedGroups?.GetStringDictionary("SharedPermissions");
    foreach (string key1 in this._permissions.Keys)
    {
      ulong permission = this._permissions[key1];
      if (stringDictionary1.ContainsKey(key1))
      {
        string[] commaSeparatedString = YamlConfig.ParseCommaSeparatedString(stringDictionary1[key1]);
        if (commaSeparatedString == null)
        {
          ServerConsole.AddLog("Failed to process group permissions in remote admin config! Make sure there is no typo.");
        }
        else
        {
          foreach (string key2 in commaSeparatedString)
          {
            if (this._groups.ContainsKey(key2))
              this._groups[key2].Permissions |= permission;
          }
        }
      }
      else
        ServerConsole.AddLog("RemoteAdmin config is missing permission definition: " + key1);
      if (stringDictionary2 != null && stringDictionary2.ContainsKey(key1))
      {
        string[] commaSeparatedString = YamlConfig.ParseCommaSeparatedString(stringDictionary2[key1]);
        if (commaSeparatedString != null)
        {
          foreach (string key2 in commaSeparatedString)
          {
            if (this._groups.ContainsKey(key2))
              this._groups[key2].Permissions |= permission;
          }
        }
      }
      else
        ServerConsole.AddLog("Shared groups config is missing permission definition: " + key1);
    }
  }

  public bool IsRaPermitted(ulong permissions)
  {
    return this._raPermissions.Any<ulong>((Func<ulong, bool>) (perm => PermissionsHandler.IsPermitted(permissions, perm)));
  }

  public UserGroup GetGroup(string name)
  {
    return this._groups.ContainsKey(name) ? this._groups[name].Clone() : (UserGroup) null;
  }

  public List<string> GetAllGroupsNames()
  {
    return this._groups.Keys.ToList<string>();
  }

  public Dictionary<string, UserGroup> GetAllGroups()
  {
    return this._groups.Keys.ToDictionary<string, string, UserGroup>((Func<string, string>) (gr => gr), (Func<string, UserGroup>) (gr => this._groups[gr]));
  }

  public string GetPermissionName(ulong value)
  {
    return this._permissions.FirstOrDefault<KeyValuePair<string, ulong>>((Func<KeyValuePair<string, ulong>, bool>) (x => (long) x.Value == (long) value)).Key;
  }

  public ulong GetPermissionValue(string name)
  {
    return this._permissions.FirstOrDefault<KeyValuePair<string, ulong>>((Func<KeyValuePair<string, ulong>, bool>) (x => x.Key == name)).Value;
  }

  public List<string> GetAllPermissions()
  {
    return this._permissions.Keys.ToList<string>();
  }

  public void SetServerAsVerified()
  {
    this.IsVerified = true;
  }

  public bool BanTeamSlots
  {
    get
    {
      return this._banTeamSlots || this.IsVerified;
    }
  }

  public bool BanTeamBypassGeo
  {
    get
    {
      return this._banTeamGeoBypass || this.IsVerified;
    }
  }

  public static bool IsPermitted(ulong permissions, PlayerPermissions check)
  {
    return PermissionsHandler.IsPermitted(permissions, (ulong) check);
  }

  public bool IsPermitted(ulong permissions, PlayerPermissions[] check)
  {
    if (check.Length == 0)
      return true;
    ulong check1 = ((IEnumerable<PlayerPermissions>) check).Aggregate<PlayerPermissions, ulong>(0UL, (Func<ulong, PlayerPermissions, ulong>) ((current, c) => (ulong) ((PlayerPermissions) current | c)));
    return PermissionsHandler.IsPermitted(permissions, check1);
  }

  public bool IsPermitted(ulong permissions, string check)
  {
    return this._permissions.ContainsKey(check) && PermissionsHandler.IsPermitted(permissions, this._permissions[check]);
  }

  public bool IsPermitted(ulong permissions, string[] check)
  {
    if (check.Length == 0)
      return true;
    ulong check1 = ((IEnumerable<string>) check).Where<string>((Func<string, bool>) (c => this._permissions.ContainsKey(c))).Aggregate<string, ulong>(0UL, (Func<ulong, string, ulong>) ((current, c) => current | this._permissions[c]));
    return PermissionsHandler.IsPermitted(permissions, check1);
  }

  public static bool IsPermitted(ulong permissions, ulong check)
  {
    return (permissions & check) > 0UL;
  }

  public byte[] DerivePassword(byte[] serverSalt, byte[] clientSalt)
  {
    return QueryProcessor.DerivePassword(this._overridePassword, serverSalt, clientSalt);
  }

  public UserGroup OverrideGroup
  {
    get
    {
      if (!this.OverrideEnabled)
        return (UserGroup) null;
      return this._groups.ContainsKey(this._overrideRole) ? this._groups[this._overrideRole] : (UserGroup) null;
    }
  }

  public bool OverrideEnabled
  {
    get
    {
      if (string.IsNullOrEmpty(this._overridePassword) || this._overridePassword == "none")
        return false;
      if (!this.IsVerified)
        return true;
      if (this._overridePassword.Length < 8)
      {
        ServerConsole.AddLog("Override password refused, because it's too short (requirement for verified servers only).");
        return false;
      }
      if (this._overridePassword.ToLower() == this._overridePassword || this._overridePassword.ToUpper() == this._overridePassword)
      {
        ServerConsole.AddLog("Override password refused, because it must contain mixed case chars (requirement for verified servers only).");
        return false;
      }
      if (this._overridePassword.Any<char>((Func<char, bool>) (c => !char.IsLetter(c))))
        return true;
      ServerConsole.AddLog("Override password refused, because it must contain digit or special symbol (requirement for verified servers only).");
      return false;
    }
  }

  public UserGroup GetUserGroup(string playerId)
  {
    return this._members.ContainsKey(playerId) ? this._groups[this._members[playerId]] : (UserGroup) null;
  }

  public bool IsVerified { get; private set; }

  public ulong FullPerm { get; private set; }

  public bool StaffAccess { get; }

  public bool ManagersAccess
  {
    get
    {
      return this._managerAccess || this.StaffAccess || this.IsVerified;
    }
  }

  public bool BanningTeamAccess
  {
    get
    {
      return this._banTeamAccess || this.StaffAccess || this.IsVerified;
    }
  }
}
