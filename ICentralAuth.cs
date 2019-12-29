// Decompiled with JetBrains decompiler
// Type: ICentralAuth
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public interface ICentralAuth
{
  void TokenGenerated(string token);

  void RequestBadge(string token);

  void RequestPublicPart(string token);

  void Fail();

  CharacterClassManager GetCcm();

  void Ok(
    string userId,
    string userId2,
    string ban,
    string server,
    bool bypass,
    bool bypassWl,
    bool DNT,
    string serial,
    string vacSession,
    string rqIp,
    string Asn,
    bool BypassIpCheck);

  void FailToken(string reason);
}
