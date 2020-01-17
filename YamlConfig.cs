// Decompiled with JetBrains decompiler
// Type: YamlConfig
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Scp914;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using Utils.ConfigHandler;

public class YamlConfig : ConfigRegister
{
  private static readonly string[] _rolevars = new string[4]
  {
    "color",
    "badge",
    "cover",
    "hidden"
  };
  private static readonly string[] _deprecatedconfigs = new string[1]
  {
    "administrator_password"
  };
  private bool _afteradding;
  private bool _virtual;
  private string[] _rawDataUnfiltered;
  private string[] _rawData;
  public string Path;

  public string[] RawData
  {
    get
    {
      return !this._virtual ? this._rawData : this._rawDataUnfiltered;
    }
    set
    {
      if (this._virtual)
        this._rawDataUnfiltered = value;
      else
        this._rawData = value;
    }
  }

  public bool IsVirtual
  {
    get
    {
      return this._virtual;
    }
    set
    {
      if (!value || this._virtual)
        return;
      this._virtual = true;
      this._rawDataUnfiltered = this.RawData;
    }
  }

  public YamlConfig()
  {
    this.RawData = new string[0];
  }

  public YamlConfig(string path)
  {
    this.Path = path;
    this.LoadConfigFile(path);
  }

  public override void UpdateConfigValue(ConfigEntry configEntry)
  {
    if (configEntry == null)
      throw new NullReferenceException("Config type unsupported (Config: Null).");
    switch (configEntry)
    {
      case ConfigEntry<bool> configEntry1:
        configEntry1.Value = this.GetBool(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<byte> configEntry1:
        configEntry1.Value = this.GetByte(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<char> configEntry1:
        configEntry1.Value = this.GetChar(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<Decimal> configEntry1:
        configEntry1.Value = this.GetDecimal(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<double> configEntry1:
        configEntry1.Value = this.GetDouble(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<float> configEntry1:
        configEntry1.Value = this.GetFloat(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<int> configEntry1:
        configEntry1.Value = this.GetInt(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<long> configEntry1:
        configEntry1.Value = this.GetLong(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<sbyte> configEntry1:
        configEntry1.Value = this.GetSByte(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<short> configEntry1:
        configEntry1.Value = this.GetShort(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<string> configEntry1:
        configEntry1.Value = this.GetString(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<uint> configEntry1:
        configEntry1.Value = this.GetUInt(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<ulong> configEntry1:
        configEntry1.Value = this.GetULong(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<ushort> configEntry1:
        configEntry1.Value = this.GetUShort(configEntry1.Key, configEntry1.Default);
        break;
      case ConfigEntry<List<bool>> configEntry1:
        configEntry1.Value = this.GetBoolList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<byte>> configEntry1:
        configEntry1.Value = this.GetByteList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<char>> configEntry1:
        configEntry1.Value = this.GetCharList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<Decimal>> configEntry1:
        configEntry1.Value = this.GetDecimalList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<double>> configEntry1:
        configEntry1.Value = this.GetDoubleList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<float>> configEntry1:
        configEntry1.Value = this.GetFloatList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<int>> configEntry1:
        configEntry1.Value = this.GetIntList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<long>> configEntry1:
        configEntry1.Value = this.GetLongList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<sbyte>> configEntry1:
        configEntry1.Value = this.GetSByteList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<short>> configEntry1:
        configEntry1.Value = this.GetShortList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<string>> configEntry1:
        configEntry1.Value = this.GetStringList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<uint>> configEntry1:
        configEntry1.Value = this.GetUIntList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<ulong>> configEntry1:
        configEntry1.Value = this.GetULongList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<List<ushort>> configEntry1:
        configEntry1.Value = this.GetUShortList(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<Dictionary<string, string>> configEntry1:
        configEntry1.Value = this.GetStringDictionary(configEntry1.Key);
        if (configEntry1.Value.Count > 0 || !string.Equals(this.GetRawString(configEntry1.Key), "default", StringComparison.OrdinalIgnoreCase))
          break;
        configEntry1.Value = configEntry1.Default;
        break;
      case ConfigEntry<Scp914Mode> configEntry1:
        string str = this.GetString(configEntry1.Key, "");
        Scp914Mode result;
        if (str == "default" || !Enum.TryParse<Scp914Mode>(str, out result))
        {
          configEntry1.Value = configEntry1.Default;
          break;
        }
        configEntry1.Value = result;
        break;
      default:
        throw new Exception("Config type unsupported (Config: Key = \"" + (configEntry.Key ?? "Null") + "\" Type = \"" + (configEntry.ValueType.FullName ?? "Null") + "\" Name = \"" + (configEntry.Name ?? "Null") + "\" Description = \"" + (configEntry.Description ?? "Null") + "\").");
    }
  }

  private static string[] Filter(IEnumerable<string> lines)
  {
    return lines.Where<string>((Func<string, bool>) (line =>
    {
      if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
        return false;
      return line.StartsWith(" - ") || line.Contains<char>(':');
    })).ToArray<string>();
  }

  public void LoadConfigFile(string path)
  {
    if (string.IsNullOrEmpty(path))
      return;
    this.Path = path;
    if (!ServerStatic.DisableConfigValidation)
      YamlConfig.RemoveInvalid(path);
    if (!ServerStatic.DisableConfigValidation && this.Path.EndsWith("config_gameplay.txt") && (!this._afteradding && FileManager.FileExists("ConfigTemplates/config_gameplay.template.txt")))
      YamlConfig.AddMissingTemplateKeys("ConfigTemplates/config_gameplay.template.txt", path, ref this._afteradding);
    else if (!ServerStatic.DisableConfigValidation && this.Path.EndsWith("config_remoteadmin.txt") && !this._afteradding)
    {
      YamlConfig.AddMissingRoleVars(path);
      YamlConfig.AddMissingPerms(path, ref this._afteradding);
    }
    this._rawDataUnfiltered = FileManager.ReadAllLines(path);
    this.RawData = YamlConfig.Filter((IEnumerable<string>) this._rawDataUnfiltered);
    if (ServerStatic.IsDedicated && this.Path.EndsWith("config_remoteadmin.txt"))
      Application.targetFrameRate = this.GetInt("server_tickrate", 60);
    this.UpdateRegisteredConfigValues();
  }

  private static void RemoveDeprecated(string path)
  {
    List<string> stringList = FileManager.ReadAllLinesList(path);
    for (int index1 = stringList.Count - 1; index1 >= 0; --index1)
    {
      for (int index2 = 0; index2 < YamlConfig._deprecatedconfigs.Length; ++index2)
      {
        if (stringList[index1].StartsWith(YamlConfig._deprecatedconfigs[index2] + ":") && (index1 == 0 || stringList[index1 - 1] != "#REMOVED FROM GAME - REDUNDANT"))
          stringList.Insert(index1, "#REMOVED FROM GAME - REDUNDANT");
      }
    }
    FileManager.WriteToFile((IEnumerable<string>) stringList, path, false);
  }

  private static void AddMissingPerms(string path, ref bool _afteradding)
  {
    string[] perms = YamlConfig.GetStringList("Permissions", path).ToArray();
    string[] names = Enum.GetNames(typeof (PlayerPermissions));
    if (perms.Length == names.Length)
      return;
    List<string> list = ((IEnumerable<string>) names).Select(permtype => new
    {
      permtype = permtype,
      inconfig = ((IEnumerable<string>) perms).Any<string>((Func<string, bool>) (perm => perm.StartsWith(permtype)))
    }).Where(t => !t.inconfig).Select(t => " - " + t.permtype + ": []").ToList<string>();
    List<string> stringList = FileManager.ReadAllLinesList(path);
    for (int index = 0; index < stringList.Count; ++index)
    {
      if (stringList[index] == "Permissions:")
        stringList.InsertRange(index + 1, (IEnumerable<string>) list);
    }
    FileManager.WriteToFile((IEnumerable<string>) stringList, path, false);
    _afteradding = true;
  }

  private static void AddMissingRoleVars(string path)
  {
    string time = TimeBehaviour.FormatTime("yyyy/MM/dd HH:mm:ss");
    string[] array = YamlConfig.GetStringList("Roles", path).ToArray();
    List<string> stringList = new List<string>();
    string config = FileManager.ReadAllText(path);
    foreach (string str in array)
    {
      string role = str;
      stringList.AddRange(((IEnumerable<string>) YamlConfig._rolevars).Where<string>((Func<string, bool>) (rolevar => !config.Contains(role + "_" + rolevar + ":"))).Select<string, string>((Func<string, string>) (rolevar => role + "_" + rolevar + ": default")));
    }
    if (stringList.Count <= 0)
      return;
    YamlConfig.Write((IEnumerable<string>) stringList, path, ref time);
  }

  private static void AddMissingTemplateKeys(
    string templatepath,
    string path,
    ref bool _afteradding)
  {
    string time = TimeBehaviour.FormatTime("yyyy/MM/dd HH:mm:ss");
    string str1 = FileManager.ReadAllText(path);
    string[] strArray = FileManager.ReadAllLines(templatepath);
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    List<string> stringList3 = new List<string>();
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!strArray[index].StartsWith("#") && !strArray[index].StartsWith(" -") && strArray[index].Contains(":") && (index + 1 < strArray.Length && strArray[index + 1].StartsWith(" -") || strArray[index].EndsWith("[]")))
        stringList1.Add(strArray[index]);
      else if (!strArray[index].StartsWith("#") && strArray[index].Contains(":") && !strArray[index].StartsWith(" -"))
        stringList2.Add(strArray[index].Substring(0, strArray[index].IndexOf(':') + 1));
    }
    foreach (string str2 in stringList2)
    {
      if (!str1.Contains(str2))
        stringList3.Add(str2 + " default");
    }
    YamlConfig.Write((IEnumerable<string>) stringList3, path, ref time);
    foreach (string str2 in stringList1)
    {
      if (!str1.Contains(str2))
      {
        bool flag = false;
        List<string> stringList4 = new List<string>()
        {
          "#LIST",
          str2
        };
        foreach (string str3 in strArray)
        {
          if (str3.StartsWith(str2) && str3.EndsWith("[]"))
          {
            stringList4.Clear();
            stringList4.AddRange((IEnumerable<string>) new string[2]
            {
              "#LIST - [] equals to empty",
              str3
            });
            break;
          }
          if (str3.StartsWith(str2))
            flag = true;
          else if (flag)
          {
            if (str3.StartsWith(" - "))
              stringList4.Add(str3);
            else if (!str3.StartsWith("#"))
              break;
          }
        }
        YamlConfig.Write((IEnumerable<string>) stringList4, path, ref time);
      }
    }
    _afteradding = true;
  }

  private static void Write(IEnumerable<string> text, string path, ref string time)
  {
    string[] array = text.ToArray<string>();
    if (array.Length == 0)
      return;
    YamlConfig.Write(string.Join("\r\n", array), path, ref time);
  }

  private static void Write(string text, string path, ref string time)
  {
    using (StreamWriter streamWriter = File.AppendText(path))
      streamWriter.Write("\r\n\r\n#ADDED BY CONFIG VALIDATOR - " + time + " Game version: " + CustomNetworkManager.CompatibleVersions[0] + "\r\n" + text);
  }

  private static void RemoveInvalid(string path)
  {
    string[] strArray = FileManager.ReadAllLines(path);
    bool flag = false;
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!strArray[index].StartsWith("#") && !strArray[index].StartsWith(" -") && (!strArray[index].Contains(":") && !string.IsNullOrEmpty(strArray[index].Replace(" ", ""))))
      {
        flag = true;
        strArray[index] = "#INVALID - " + strArray[index];
      }
    }
    if (!flag)
      return;
    FileManager.WriteToFile((IEnumerable<string>) strArray, path, false);
  }

  private void CommentInvalid(string key, string type)
  {
    if (this.IsVirtual)
      return;
    for (int index = 0; index < this._rawDataUnfiltered.Length; ++index)
    {
      if (this._rawDataUnfiltered[index].StartsWith(key + ": "))
        this._rawDataUnfiltered[index] = "#INVALID " + type + " - " + this._rawDataUnfiltered[index];
    }
    if (ServerStatic.DisableConfigValidation)
      return;
    FileManager.WriteToFile((IEnumerable<string>) this._rawDataUnfiltered, this.Path, false);
  }

  public bool Reload()
  {
    if (this.IsVirtual || string.IsNullOrEmpty(this.Path))
      return false;
    this.LoadConfigFile(this.Path);
    return true;
  }

  private string GetRawString(string key)
  {
    foreach (string str in this.RawData)
    {
      if (str.StartsWith(key + ": "))
        return str.Substring(key.Length + 2);
    }
    return "default";
  }

  public void SetString(string key, string value = null)
  {
    this.Reload();
    int num = 0;
    List<string> stringList = (List<string>) null;
    for (int index = 0; index < this._rawDataUnfiltered.Length; ++index)
    {
      if (this._rawDataUnfiltered[index].StartsWith(key + ": "))
      {
        if (value == null)
        {
          stringList = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
          stringList.RemoveAt(index);
          num = 2;
          break;
        }
        this._rawDataUnfiltered[index] = key + ": " + value;
        num = 1;
        break;
      }
    }
    if (this.IsVirtual)
      return;
    switch (num)
    {
      case 0:
        List<string> list = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
        list.Insert(list.Count, key + ": " + value);
        FileManager.WriteToFile((IEnumerable<string>) list, this.Path, false);
        break;
      case 1:
        FileManager.WriteToFile((IEnumerable<string>) this._rawDataUnfiltered, this.Path, false);
        break;
      case 2:
        if (stringList != null)
        {
          FileManager.WriteToFile((IEnumerable<string>) stringList, this.Path, false);
          break;
        }
        break;
    }
    this.Reload();
  }

  private static List<string> GetStringList(string key, string path)
  {
    bool flag = false;
    List<string> stringList = new List<string>();
    foreach (string readAllLine in FileManager.ReadAllLines(path))
    {
      if (!readAllLine.StartsWith(key) || !readAllLine.EndsWith("[]"))
      {
        if (readAllLine.StartsWith(key + ":"))
        {
          string data = readAllLine.Substring(key.Length + 1);
          if (data.Contains("[") && data.Contains("]"))
            return ((IEnumerable<string>) YamlConfig.ParseCommaSeparatedString(data)).ToList<string>();
          flag = true;
        }
        else if (flag)
        {
          if (readAllLine.StartsWith(" - "))
            stringList.Add(readAllLine.Substring(3));
          else if (!readAllLine.StartsWith("#"))
            break;
        }
      }
      else
        break;
    }
    return stringList;
  }

  public void SetStringListItem(string key, string value, string newValue)
  {
    this.Reload();
    bool flag = false;
    int num = 0;
    List<string> stringList = (List<string>) null;
    for (int index = 0; index < this._rawDataUnfiltered.Length; ++index)
    {
      string str = this._rawDataUnfiltered[index];
      if (str.StartsWith(key + ":"))
        flag = true;
      else if (flag)
      {
        if (value != null && str == " - " + value)
        {
          if (newValue == null)
          {
            stringList = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
            stringList.RemoveAt(index);
            num = 2;
            break;
          }
          this._rawDataUnfiltered[index] = " - " + newValue;
          num = 1;
          break;
        }
        if (!str.StartsWith(" - ") && !str.StartsWith("#"))
        {
          if (value != null)
          {
            stringList = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
            stringList.Insert(index, " - " + newValue);
            num = 2;
            break;
          }
          break;
        }
      }
    }
    if (this.IsVirtual)
      return;
    switch (num)
    {
      case 1:
        FileManager.WriteToFile((IEnumerable<string>) this._rawDataUnfiltered, this.Path, false);
        break;
      case 2:
        if (stringList != null)
        {
          FileManager.WriteToFile((IEnumerable<string>) stringList, this.Path, false);
          break;
        }
        break;
    }
    this.Reload();
  }

  public IEnumerable<string> StringListToText(string key, List<string> list)
  {
    yield return key + ":";
    foreach (string str in list)
      yield return " - " + str;
  }

  public Dictionary<string, string> GetStringDictionary(string key)
  {
    List<string> stringList = this.GetStringList(key);
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (string str in stringList)
    {
      int length = str.IndexOf(": ", StringComparison.Ordinal);
      string key1 = str.Substring(0, length);
      if (!dictionary.ContainsKey(key1))
        dictionary.Add(key1, str.Substring(length + 2));
      else
        ServerConsole.AddLog("Ignoring duplicated subkey " + key1 + " in dictionary " + key + " in the config file.");
    }
    return dictionary;
  }

  public void SetStringDictionaryItem(string key, string subkey, string value)
  {
    this.Reload();
    bool flag = false;
    int num = 0;
    List<string> stringList = (List<string>) null;
    for (int index = 0; index < this._rawDataUnfiltered.Length; ++index)
    {
      string str = this._rawDataUnfiltered[index];
      if (str.StartsWith(key + ":"))
        flag = true;
      else if (flag)
      {
        if (str.StartsWith(" - " + subkey + ": "))
        {
          if (value == null)
          {
            stringList = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
            stringList.RemoveAt(index);
            num = 2;
            break;
          }
          this._rawDataUnfiltered[index] = " - " + subkey + ": " + value;
          num = 1;
          break;
        }
        if (!str.StartsWith(" - ") && !str.StartsWith("#"))
        {
          if (value != null)
          {
            stringList = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
            stringList.Insert(index, " - " + subkey + ": " + value);
            num = 2;
            break;
          }
          break;
        }
      }
    }
    if (this.IsVirtual)
      return;
    switch (num)
    {
      case 0:
        List<string> list = ((IEnumerable<string>) this._rawDataUnfiltered).ToList<string>();
        list.Insert(list.Count, " - " + subkey + ": " + value);
        FileManager.WriteToFile((IEnumerable<string>) list, this.Path, false);
        break;
      case 1:
        FileManager.WriteToFile((IEnumerable<string>) this._rawDataUnfiltered, this.Path, false);
        break;
      case 2:
        if (stringList != null)
        {
          FileManager.WriteToFile((IEnumerable<string>) stringList, this.Path, false);
          break;
        }
        break;
    }
    this.Reload();
  }

  public static string[] ParseCommaSeparatedString(string data)
  {
    data = data.Trim();
    if (!data.StartsWith("[") || !data.EndsWith("]"))
      return (string[]) null;
    data = data.Substring(1, data.Length - 2).Replace(", ", ",");
    return data.Split(',');
  }

  public IEnumerable<string> GetKeys()
  {
    return ((IEnumerable<string>) this.RawData).Where<string>((Func<string, bool>) (line => line.Contains(":"))).Select<string, string>((Func<string, string>) (line => line.Split(':')[0]));
  }

  public bool IsList(string key)
  {
    bool flag = false;
    foreach (string str in this.RawData)
    {
      if (str.StartsWith(key + ":"))
        flag = true;
      else if (flag)
      {
        if (str.StartsWith(" - "))
          return true;
        if (!str.StartsWith("#"))
          break;
      }
    }
    return false;
  }

  public void Merge(ref YamlConfig toMerge)
  {
    string[] array = this.GetKeys().ToArray<string>();
    this.IsVirtual = true;
    foreach (string key in toMerge.GetKeys())
    {
      if (!array.Contains<string>(key))
      {
        if (toMerge.IsList(key))
        {
            foreach (string item in toMerge.StringListToText(key, toMerge.GetStringList(key)))
                RawData.Append(item);
        }
        else
          this.SetString(key, toMerge.GetRawString(key));
      }
    }
  }

  public bool GetBool(string key, bool def = false)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    bool result;
    if (bool.TryParse(lower, out result))
      return result;
    ServerConsole.AddLog(key + " has invalid value, " + lower + " is not a valid bool!");
    this.CommentInvalid(key, "BOOL");
    return def;
  }

  public byte GetByte(string key, byte def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    byte result;
    if (byte.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid byte!");
    this.CommentInvalid(key, "BYTE");
    return def;
  }

  public sbyte GetSByte(string key, sbyte def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    sbyte result;
    if (sbyte.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid signed byte!");
    this.CommentInvalid(key, "SBYTE");
    return def;
  }

  public char GetChar(string key, char def = ' ')
  {
    string rawString = this.GetRawString(key);
    if (rawString == "default")
      return def;
    char result;
    if (char.TryParse(rawString, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + rawString + " is not a valid char!");
    this.CommentInvalid(key, "CHAR");
    return def;
  }

  public Decimal GetDecimal(string key, Decimal def = 0M)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    Decimal result;
    if (Decimal.TryParse(lower.Replace(',', '.'), NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has invalid value, " + lower + " is not a valid decimal!");
    this.CommentInvalid(key, "DECIMAL");
    return def;
  }

  public double GetDouble(string key, double def = 0.0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    double result;
    if (double.TryParse(lower.Replace(',', '.'), NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has invalid value, " + lower + " is not a valid double!");
    this.CommentInvalid(key, "DOUBLE");
    return def;
  }

  public float GetFloat(string key, float def = 0.0f)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    float result;
    if (float.TryParse(lower.Replace(',', '.'), NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has invalid value, " + lower + " is not a valid float!");
    this.CommentInvalid(key, "FLOAT");
    return def;
  }

  public int GetInt(string key, int def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    int result;
    if (int.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid integer!");
    this.CommentInvalid(key, "INT");
    return def;
  }

  public uint GetUInt(string key, uint def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    uint result;
    if (uint.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid unsigned integer!");
    this.CommentInvalid(key, "UINT");
    return def;
  }

  public long GetLong(string key, long def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    long result;
    if (long.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid long!");
    this.CommentInvalid(key, "LONG");
    return def;
  }

  public ulong GetULong(string key, ulong def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    ulong result;
    if (ulong.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid unsigned long!");
    this.CommentInvalid(key, "ULONG");
    return def;
  }

  public short GetShort(string key, short def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    short result;
    if (short.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid short!");
    this.CommentInvalid(key, "SHORT");
    return def;
  }

  public ushort GetUShort(string key, ushort def = 0)
  {
    string lower = this.GetRawString(key).ToLower();
    if (lower == "default")
      return def;
    ushort result;
    if (ushort.TryParse(lower, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result))
      return result;
    ServerConsole.AddLog(key + " has an invalid value, " + lower + " is not a valid unsigned short!");
    this.CommentInvalid(key, "USHORT");
    return def;
  }

  public string GetString(string key, string def = "")
  {
    string rawString = this.GetRawString(key);
    return !(rawString == "default") ? rawString : def;
  }

  public List<bool> GetBoolList(string key)
  {
    return this.GetStringList(key).Select<string, bool>(new Func<string, bool>(bool.Parse)).ToList<bool>();
  }

  public List<byte> GetByteList(string key)
  {
    return this.GetStringList(key).Select<string, byte>(new Func<string, byte>(byte.Parse)).ToList<byte>();
  }

  public List<sbyte> GetSByteList(string key)
  {
    return this.GetStringList(key).Select<string, sbyte>(new Func<string, sbyte>(sbyte.Parse)).ToList<sbyte>();
  }

  public List<char> GetCharList(string key)
  {
    return this.GetStringList(key).Select<string, char>(new Func<string, char>(char.Parse)).ToList<char>();
  }

  public List<Decimal> GetDecimalList(string key)
  {
    return this.GetStringList(key).Select<string, Decimal>(new Func<string, Decimal>(Decimal.Parse)).ToList<Decimal>();
  }

  public List<double> GetDoubleList(string key)
  {
    return this.GetStringList(key).Select<string, double>(new Func<string, double>(double.Parse)).ToList<double>();
  }

  public List<float> GetFloatList(string key)
  {
    return this.GetStringList(key).Select<string, float>(new Func<string, float>(float.Parse)).ToList<float>();
  }

  public List<int> GetIntList(string key)
  {
    return this.GetStringList(key).Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>();
  }

  public List<uint> GetUIntList(string key)
  {
    return this.GetStringList(key).Select<string, uint>(new Func<string, uint>(uint.Parse)).ToList<uint>();
  }

  public List<long> GetLongList(string key)
  {
    return this.GetStringList(key).Select<string, long>(new Func<string, long>(long.Parse)).ToList<long>();
  }

  public List<ulong> GetULongList(string key)
  {
    return this.GetStringList(key).Select<string, ulong>(new Func<string, ulong>(ulong.Parse)).ToList<ulong>();
  }

  public List<short> GetShortList(string key)
  {
    return this.GetStringList(key).Select<string, short>(new Func<string, short>(short.Parse)).ToList<short>();
  }

  public List<ushort> GetUShortList(string key)
  {
    return this.GetStringList(key).Select<string, ushort>(new Func<string, ushort>(ushort.Parse)).ToList<ushort>();
  }

  public List<string> GetStringList(string key)
  {
    bool flag = false;
    List<string> stringList = new List<string>();
    foreach (string str in this.RawData)
    {
      if (!str.StartsWith(key) || !str.TrimEnd((char[]) Array.Empty<char>()).EndsWith("[]"))
      {
        if (str.StartsWith(key + ":"))
        {
          string data = str.Substring(key.Length + 1);
          if (data.Contains("[") && data.Contains("]"))
            return ((IEnumerable<string>) YamlConfig.ParseCommaSeparatedString(data)).ToList<string>();
          flag = true;
        }
        else if (flag)
        {
          if (str.StartsWith(" - "))
            stringList.Add(str.Substring(3).TrimEnd((char[]) Array.Empty<char>()));
          else if (!str.StartsWith("#"))
            break;
        }
      }
      else
        break;
    }
    return stringList;
  }
}
