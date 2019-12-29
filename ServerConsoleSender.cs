// Decompiled with JetBrains decompiler
// Type: ServerConsoleSender
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

internal class ServerConsoleSender : CommandSender
{
  public override string SenderId
  {
    get
    {
      return "SERVER CONSOLE";
    }
  }

  public override string Nickname
  {
    get
    {
      return "SERVER CONSOLE";
    }
  }

  public override ulong Permissions
  {
    get
    {
      return ServerStatic.PermissionsHandler.FullPerm;
    }
  }

  public override byte KickPower
  {
    get
    {
      return byte.MaxValue;
    }
  }

  public override bool FullPermissions
  {
    get
    {
      return true;
    }
  }

  public override void RaReply(
    string text,
    bool success,
    bool logToConsole,
    string overrideDisplay)
  {
    ServerConsole.AddLog(text);
  }

  public override void Print(string text)
  {
    ServerConsole.AddLog(text);
  }
}
