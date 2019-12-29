// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.PlayerCommandSender
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace RemoteAdmin
{
  internal class PlayerCommandSender : CommandSender
  {
    internal readonly QueryProcessor Processor;
    internal readonly CharacterClassManager CCM;
    internal readonly NicknameSync NS;
    internal readonly ServerRoles SR;

    public PlayerCommandSender(QueryProcessor qp)
    {
      this.Processor = qp;
      this.SR = qp.GetComponent<ServerRoles>();
      this.CCM = qp.GetComponent<CharacterClassManager>();
      this.NS = qp.GetComponent<NicknameSync>();
    }

    public override string SenderId
    {
      get
      {
        return this.CCM.UserId;
      }
    }

    public int PlayerId
    {
      get
      {
        return this.Processor.PlayerId;
      }
    }

    public override string Nickname
    {
      get
      {
        return this.NS.MyNick;
      }
    }

    public override ulong Permissions
    {
      get
      {
        return this.SR.Permissions;
      }
    }

    public override byte KickPower
    {
      get
      {
        return !this.SR.RaEverywhere ? this.SR.KickPower : byte.MaxValue;
      }
    }

    public override bool FullPermissions
    {
      get
      {
        return false;
      }
    }

    public override void RaReply(
      string text,
      bool success,
      bool logToConsole,
      string overrideDisplay)
    {
      this.Processor.TargetReply(this.Processor.connectionToClient, text, success, logToConsole, overrideDisplay);
    }

    public override void Print(string text)
    {
      this.Processor.TargetReply(this.Processor.connectionToClient, text, true, true, "");
    }
  }
}
