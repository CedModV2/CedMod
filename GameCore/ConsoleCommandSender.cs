// Decompiled with JetBrains decompiler
// Type: GameCore.ConsoleCommandSender
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace GameCore
{
  internal class ConsoleCommandSender : CommandSender
  {
    public override string SenderId
    {
      get
      {
        return "GAME CONSOLE";
      }
    }

    public override string Nickname
    {
      get
      {
        return "GAME CONSOLE";
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
      Console.AddLog("[RA Reply] " + text, success ? Color.green : Color.red, false);
    }

    public override void Print(string text)
    {
      Console.AddLog(text, Color.green, false);
    }
  }
}
