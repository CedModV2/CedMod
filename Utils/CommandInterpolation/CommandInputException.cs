// Decompiled with JetBrains decompiler
// Type: Utils.CommandInterpolation.CommandInputException
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utils.CommandInterpolation
{
  public class CommandInputException : Exception
  {
    public string ArgumentName { get; }

    public object ArgumentValue { get; }

    public CommandInputException(string argName, object argValue, string message)
      : base(message)
    {
      this.ArgumentName = argName;
      this.ArgumentValue = argValue;
    }
  }
}
