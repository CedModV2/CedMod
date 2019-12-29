// Decompiled with JetBrains decompiler
// Type: Utils.ConfigHandler.ConfigEntry
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utils.ConfigHandler
{
  public abstract class ConfigEntry
  {
    public string Key { get; }

    public abstract Type ValueType { get; }

    public abstract object ObjectValue { get; set; }

    public abstract object ObjectDefault { get; set; }

    public bool Inherit { get; }

    public string Name { get; }

    public string Description { get; }

    public ConfigEntry(string key, bool inherit = true, string name = null, string description = null)
    {
      this.Key = key;
      this.Inherit = inherit;
      this.Name = name;
      this.Description = description;
    }

    public ConfigEntry(string key, string name = null, string description = null)
      : this(key, true, name, description)
    {
    }
  }
}
