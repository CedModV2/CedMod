// Decompiled with JetBrains decompiler
// Type: Utils.ConfigHandler.InheritableConfigRegister
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utils.ConfigHandler
{
  public abstract class InheritableConfigRegister : ConfigRegister
  {
    protected InheritableConfigRegister(ConfigRegister parentConfigRegister = null)
    {
      this.ParentConfigRegister = parentConfigRegister;
    }

    public ConfigRegister ParentConfigRegister { get; protected set; }

    public abstract bool ShouldInheritConfigEntry(ConfigEntry configEntry);

    public abstract void UpdateConfigValueInheritable(ConfigEntry configEntry);

    public override void UpdateConfigValue(ConfigEntry configEntry)
    {
      if (configEntry != null && configEntry.Inherit && (this.ParentConfigRegister != null && this.ShouldInheritConfigEntry(configEntry)))
        this.ParentConfigRegister.UpdateConfigValue(configEntry);
      else
        this.UpdateConfigValueInheritable(configEntry);
    }

    public ConfigRegister[] GetConfigRegisterHierarchy(bool highestToLowest = true)
    {
      List<ConfigRegister> configRegisterList = new List<ConfigRegister>();
      for (ConfigRegister configRegister = (ConfigRegister) this; configRegister != null && !configRegisterList.Contains(configRegister); configRegister = inheritableConfigRegister.ParentConfigRegister)
      {
        configRegisterList.Add(configRegister);
        if (!(configRegister is InheritableConfigRegister inheritableConfigRegister))
          break;
      }
      if (highestToLowest)
        configRegisterList.Reverse();
      return configRegisterList.ToArray();
    }
  }
}
