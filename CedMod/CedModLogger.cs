// Decompiled with JetBrains decompiler
// Type: CedMod.CedModLogger
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod.Logging;
using GameCore;

namespace CedMod
{
  public class CedModLogger : Logger
  {
    public override void Debug(string tag, string message)
    {
      if (!ConfigFile.ServerConfig.GetBool("cm_debug", false))
        return;
      this.Write("DEBUG", tag, message);
    }

    public override void Error(string tag, string message)
    {
      this.Write("ERROR", tag, message);
    }

    public override void Info(string tag, string message)
    {
      this.Write("INFO", tag, message);
    }

    public override void Warn(string tag, string message)
    {
      this.Write("WARN", tag, message);
    }

    private void Write(string level, string tag, string message)
    {
      ServerConsole.AddLog(string.Format("[{0}] [{1}] {2}", (object) level, (object) tag, (object) message));
    }
  }
}
