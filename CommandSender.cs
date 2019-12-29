// Decompiled with JetBrains decompiler
// Type: CommandSender
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public abstract class CommandSender : IOutput
{
  public abstract string SenderId { get; }

  public abstract string Nickname { get; }

  public abstract ulong Permissions { get; }

  public abstract byte KickPower { get; }

  public abstract bool FullPermissions { get; }

  public abstract void RaReply(
    string text,
    bool success,
    bool logToConsole,
    string overrideDisplay);

  public abstract void Print(string text);
}
