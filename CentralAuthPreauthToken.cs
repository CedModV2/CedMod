// Decompiled with JetBrains decompiler
// Type: CentralAuthPreauthToken
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public readonly struct CentralAuthPreauthToken
{
  public readonly string UserId;
  public readonly byte Flags;
  public readonly string Country;
  public readonly ulong Expiration;
  public readonly string Signature;

  public CentralAuthPreauthToken(
    string userId,
    byte flags,
    string country,
    ulong expiration,
    string signature)
  {
    this.UserId = userId;
    this.Flags = flags;
    this.Country = country;
    this.Expiration = expiration;
    this.Signature = signature;
  }
}
