// Decompiled with JetBrains decompiler
// Type: CedMod.Logging.Logger
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace CedMod.Logging
{
  public abstract class Logger
  {
    public abstract void Debug(string tag, string message);

    public abstract void Info(string tag, string message);

    public abstract void Warn(string tag, string message);

    public abstract void Error(string tag, string message);
  }
}
