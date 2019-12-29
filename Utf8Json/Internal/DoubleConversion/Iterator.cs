// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.Iterator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal.DoubleConversion
{
  internal struct Iterator
  {
    private byte[] buffer;
    private int offset;

    public Iterator(byte[] buffer, int offset)
    {
      this.buffer = buffer;
      this.offset = offset;
    }

    public byte Value
    {
      get
      {
        return this.buffer[this.offset];
      }
    }

    public static Iterator operator ++(Iterator self)
    {
      ++self.offset;
      return self;
    }

    public static Iterator operator +(Iterator self, int length)
    {
      return new Iterator()
      {
        buffer = self.buffer,
        offset = self.offset + length
      };
    }

    public static int operator -(Iterator lhs, Iterator rhs)
    {
      return lhs.offset - rhs.offset;
    }

    public static bool operator ==(Iterator lhs, Iterator rhs)
    {
      return lhs.offset == rhs.offset;
    }

    public static bool operator !=(Iterator lhs, Iterator rhs)
    {
      return lhs.offset != rhs.offset;
    }

    public static bool operator ==(Iterator lhs, char rhs)
    {
      return (int) lhs.buffer[lhs.offset] == (int) (byte) rhs;
    }

    public static bool operator !=(Iterator lhs, char rhs)
    {
      return (int) lhs.buffer[lhs.offset] != (int) (byte) rhs;
    }

    public static bool operator ==(Iterator lhs, byte rhs)
    {
      return (int) lhs.buffer[lhs.offset] == (int) rhs;
    }

    public static bool operator !=(Iterator lhs, byte rhs)
    {
      return (int) lhs.buffer[lhs.offset] != (int) rhs;
    }

    public static bool operator >=(Iterator lhs, char rhs)
    {
      return (int) lhs.buffer[lhs.offset] >= (int) (byte) rhs;
    }

    public static bool operator <=(Iterator lhs, char rhs)
    {
      return (int) lhs.buffer[lhs.offset] <= (int) (byte) rhs;
    }

    public static bool operator >(Iterator lhs, char rhs)
    {
      return (int) lhs.buffer[lhs.offset] > (int) (byte) rhs;
    }

    public static bool operator <(Iterator lhs, char rhs)
    {
      return (int) lhs.buffer[lhs.offset] < (int) (byte) rhs;
    }
  }
}
