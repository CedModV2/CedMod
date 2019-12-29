// Decompiled with JetBrains decompiler
// Type: IESLights.IESParseException
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.Serialization;

namespace IESLights
{
  [Serializable]
  public class IESParseException : Exception
  {
    public IESParseException()
    {
    }

    public IESParseException(string message)
      : base(message)
    {
    }

    public IESParseException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected IESParseException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
