// Decompiled with JetBrains decompiler
// Type: StartupArguments
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;

public static class StartupArguments
{
  public static bool IsSetShort(string param)
  {
    return ((IEnumerable<string>) Environment.GetCommandLineArgs()).Any<string>((Func<string, bool>) (x => x.StartsWith("-") && !x.StartsWith("--") && x.Contains(param)));
  }

  public static bool IsSetBool(string param, string alias = "")
  {
    if (Environment.GetCommandLineArgs().Contains<string>("--" + param))
      return true;
    return !string.IsNullOrEmpty(alias) && StartupArguments.IsSetShort(alias);
  }

  public static string GetArgument(string param, string alias = "", string def = "")
  {
    string[] commandLineArgs = Environment.GetCommandLineArgs();
    bool flag = false;
    foreach (string str in commandLineArgs)
    {
      if (flag && !str.StartsWith("-"))
        return str;
      flag = str == "--" + param || !string.IsNullOrEmpty(alias) && str.StartsWith("-") && !str.StartsWith("--") && str.EndsWith(alias);
    }
    return def;
  }
}
