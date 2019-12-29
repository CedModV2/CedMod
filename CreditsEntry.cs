// Decompiled with JetBrains decompiler
// Type: CreditsEntry
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;

public class CreditsEntry
{
  public string Title;
  public string Name;
  public bool Multi;

  public static CreditsEntry CreateSeparator()
  {
    return new CreditsEntry();
  }

  public static CreditsEntry CreateEntry(string title, string name)
  {
    return new CreditsEntry()
    {
      Multi = false,
      Title = title,
      Name = name
    };
  }

  public static CreditsEntry CreateEntry(string name)
  {
    return new CreditsEntry()
    {
      Multi = false,
      Title = "",
      Name = name
    };
  }

  public static CreditsEntry CreateEntry(string[] names)
  {
    string str = ((IEnumerable<string>) names).Aggregate<string, string>("", (Func<string, string, string>) ((current, n) => current + n + "\n"));
    return new CreditsEntry()
    {
      Multi = true,
      Title = "",
      Name = str
    };
  }
}
