// Decompiled with JetBrains decompiler
// Type: Utils.ConfigHandler.ConfigRegister
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

namespace Utils.ConfigHandler
{
  public abstract class ConfigRegister
  {
    protected readonly List<ConfigEntry> registeredConfigs = new List<ConfigEntry>();

    public ConfigEntry[] GetRegisteredConfigs()
    {
      return this.registeredConfigs.ToArray();
    }

    public ConfigEntry GetRegisteredConfig(string key)
    {
      if (string.IsNullOrEmpty(key))
        return (ConfigEntry) null;
      foreach (ConfigEntry registeredConfig in this.registeredConfigs)
      {
        if (string.Equals(key, registeredConfig.Key, StringComparison.OrdinalIgnoreCase))
          return registeredConfig;
      }
      return (ConfigEntry) null;
    }

    public void RegisterConfig(ConfigEntry configEntry, bool updateValue = true)
    {
      if (configEntry == null || string.IsNullOrEmpty(configEntry.Key))
        return;
      this.registeredConfigs.Add(configEntry);
      if (!updateValue)
        return;
      this.UpdateConfigValue(configEntry);
    }

    public void RegisterConfigs(ConfigEntry[] configEntries, bool updateValue = true)
    {
      if (configEntries == null)
        return;
      foreach (ConfigEntry configEntry in configEntries)
        this.RegisterConfig(configEntry, updateValue);
    }

    public void UnRegisterConfig(ConfigEntry configEntry)
    {
      if (configEntry == null || string.IsNullOrEmpty(configEntry.Key))
        return;
      this.registeredConfigs.Remove(configEntry);
    }

    public void UnRegisterConfig(string key)
    {
      this.UnRegisterConfig(this.GetRegisteredConfig(key));
    }

    public void UnRegisterConfigs(params ConfigEntry[] configEntries)
    {
      if (configEntries == null)
        return;
      foreach (ConfigEntry configEntry in configEntries)
        this.UnRegisterConfig(configEntry);
    }

    public void UnRegisterConfigs(params string[] keys)
    {
      if (keys == null)
        return;
      foreach (string key in keys)
        this.UnRegisterConfig(key);
    }

    public void UnRegisterConfigs()
    {
      foreach (ConfigEntry registeredConfig in this.registeredConfigs)
        this.UnRegisterConfig(registeredConfig);
    }

    public abstract void UpdateConfigValue(ConfigEntry configEntry);

    public void UpdateConfigValues(params ConfigEntry[] configEntries)
    {
      if (configEntries == null)
        return;
      foreach (ConfigEntry configEntry in configEntries)
        this.UpdateConfigValue(configEntry);
    }

    public void UpdateRegisteredConfigValues()
    {
      foreach (ConfigEntry registeredConfig in this.registeredConfigs)
        this.UpdateConfigValue(registeredConfig);
    }
  }
}
