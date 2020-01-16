// Decompiled with JetBrains decompiler
// Type: Utils.ConfigHandler.ConfigEntry`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utils.ConfigHandler
{
  public class ConfigEntry<T> : ConfigEntry
  {
    public override Type ValueType
    {
      get
      {
        return typeof (T);
      }
    }

    public T Value { get; set; }

    public T Default { get; set; }

    public override object ObjectValue
    {
      get
      {
        return (object) this.Value;
      }
      set
      {
        this.Value = (T) value;
      }
    }

    public override object ObjectDefault
    {
      get
      {
        return (object) this.Default;
      }
      set
      {
        this.Default = (T) value;
      }
    }

    public ConfigEntry(string key, T defaultValue = default(T), bool inherit = true, string name = null, string description = null)
      : base(key, inherit, name, description)
    {
      this.Default = defaultValue;
    }

    public ConfigEntry(string key, T defaultValue = default(T), string name = null, string description = null)
      : this(key, defaultValue, true, name, description)
    {
        this.Default = defaultValue;
    }
  }
}
