// Decompiled with JetBrains decompiler
// Type: UserPrint
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

internal class UserPrint : CommandSender
{
  private readonly QueryUser _qu;

  public override string SenderId
  {
    get
    {
      return "Query";
    }
  }

  public override string Nickname
  {
    get
    {
      return "Query";
    }
  }

  public override ulong Permissions
  {
    get
    {
      return this._qu.Permissions;
    }
  }

  public override byte KickPower
  {
    get
    {
      return this._qu.KickPower;
    }
  }

  public override bool FullPermissions
  {
    get
    {
      return false;
    }
  }

  public UserPrint(QueryUser usr)
  {
    this._qu = usr;
  }

  public override void Print(string text)
  {
    this._qu.Send(text);
  }

  public override void RaReply(
    string text,
    bool success,
    bool logToConsole,
    string overrideDisplay)
  {
    this._qu.Send(JsonSerialize.ToJson<QueryRaReply>(new QueryRaReply(text, success, logToConsole, overrideDisplay)));
  }
}
