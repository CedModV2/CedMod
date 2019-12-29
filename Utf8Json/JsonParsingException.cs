// Decompiled with JetBrains decompiler
// Type: Utf8Json.JsonParsingException
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json
{
  public class JsonParsingException : Exception
  {
    private WeakReference underyingBytes;
    private int limit;

    public int Offset { get; private set; }

    public string ActualChar { get; set; }

    public JsonParsingException(string message)
      : base(message)
    {
    }

    public JsonParsingException(
      string message,
      byte[] underlyingBytes,
      int offset,
      int limit,
      string actualChar)
      : base(message)
    {
      this.underyingBytes = new WeakReference((object) underlyingBytes);
      this.Offset = offset;
      this.ActualChar = actualChar;
      this.limit = limit;
    }

    public byte[] GetUnderlyingByteArrayUnsafe()
    {
      return this.underyingBytes.Target as byte[];
    }

    public string GetUnderlyingStringUnsafe()
    {
      return this.underyingBytes.Target is byte[] target ? StringEncoding.UTF8.GetString(target, 0, this.limit) + "..." : (string) null;
    }
  }
}
