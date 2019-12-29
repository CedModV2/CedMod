// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.StringBuilder
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Internal.DoubleConversion
{
  internal struct StringBuilder
  {
    public byte[] buffer;
    public int offset;

    public StringBuilder(byte[] buffer, int position)
    {
      this.buffer = buffer;
      this.offset = position;
    }

    public void AddCharacter(byte str)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = str;
    }

    public void AddString(byte[] str)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, str.Length);
      for (int index = 0; index < str.Length; ++index)
        this.buffer[this.offset + index] = str[index];
      this.offset += str.Length;
    }

    public void AddSubstring(byte[] str, int length)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, length);
      for (int index = 0; index < length; ++index)
        this.buffer[this.offset + index] = str[index];
      this.offset += length;
    }

    public void AddSubstring(byte[] str, int start, int length)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, length);
      for (int index = 0; index < length; ++index)
        this.buffer[this.offset + index] = str[start + index];
      this.offset += length;
    }

    public void AddPadding(byte c, int count)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, count);
      for (int index = 0; index < count; ++index)
        this.buffer[this.offset + index] = c;
      this.offset += count;
    }

    public void AddStringSlow(string str)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, StringEncoding.UTF8.GetMaxByteCount(str.Length));
      this.offset += StringEncoding.UTF8.GetBytes(str, 0, str.Length, this.buffer, this.offset);
    }
  }
}
