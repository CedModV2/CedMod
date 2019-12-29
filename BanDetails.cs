// Decompiled with JetBrains decompiler
// Type: BanDetails
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

public class BanDetails
{
  public string OriginalName;
  public string Id;
  public long Expires;
  public string Reason;
  public string Issuer;
  public long IssuanceTime;

  public override string ToString()
  {
    return this.OriginalName.Replace(";", ":") + ";" + this.Id.Replace(";", ":") + ";" + Convert.ToString(this.Expires) + ";" + this.Reason.Replace(";", ":") + ";" + this.Issuer.Replace(";", ":") + ";" + Convert.ToString(this.IssuanceTime);
  }
}
