// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.Vector
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal.DoubleConversion
{
  internal struct Vector
  {
    public readonly byte[] bytes;
    public readonly int start;
    public readonly int _length;

    public Vector(byte[] bytes, int start, int length)
    {
      this.bytes = bytes;
      this.start = start;
      this._length = length;
    }

    public byte this[int i]
    {
      get
      {
        return this.bytes[this.start + i];
      }
      set
      {
        this.bytes[this.start + i] = value;
      }
    }

    public int length()
    {
      return this._length;
    }

    public byte first()
    {
      return this.bytes[this.start];
    }

    public byte last()
    {
      return this.bytes[this._length - 1];
    }

    public bool is_empty()
    {
      return this._length == 0;
    }

    public Vector SubVector(int from, int to)
    {
      return new Vector(this.bytes, this.start + from, to - from);
    }
  }
}
