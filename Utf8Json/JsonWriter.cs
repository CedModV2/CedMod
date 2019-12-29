// Decompiled with JetBrains decompiler
// Type: Utf8Json.JsonWriter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Text;
using Utf8Json.Internal;
using Utf8Json.Internal.DoubleConversion;

namespace Utf8Json
{
  public struct JsonWriter
  {
    private static readonly byte[] emptyBytes = new byte[0];
    private byte[] buffer;
    private int offset;

    public int CurrentOffset
    {
      get
      {
        return this.offset;
      }
    }

    public void AdvanceOffset(int offset)
    {
      this.offset += offset;
    }

    public static byte[] GetEncodedPropertyName(string propertyName)
    {
      JsonWriter jsonWriter = new JsonWriter();
      jsonWriter.WritePropertyName(propertyName);
      return jsonWriter.ToUtf8ByteArray();
    }

    public static byte[] GetEncodedPropertyNameWithPrefixValueSeparator(string propertyName)
    {
      JsonWriter jsonWriter = new JsonWriter();
      jsonWriter.WriteValueSeparator();
      jsonWriter.WritePropertyName(propertyName);
      return jsonWriter.ToUtf8ByteArray();
    }

    public static byte[] GetEncodedPropertyNameWithBeginObject(string propertyName)
    {
      JsonWriter jsonWriter = new JsonWriter();
      jsonWriter.WriteBeginObject();
      jsonWriter.WritePropertyName(propertyName);
      return jsonWriter.ToUtf8ByteArray();
    }

    public static byte[] GetEncodedPropertyNameWithoutQuotation(string propertyName)
    {
      JsonWriter jsonWriter = new JsonWriter();
      jsonWriter.WriteString(propertyName);
      ArraySegment<byte> buffer = jsonWriter.GetBuffer();
      byte[] numArray = new byte[buffer.Count - 2];
      Buffer.BlockCopy((Array) buffer.Array, buffer.Offset + 1, (Array) numArray, 0, numArray.Length);
      return numArray;
    }

    public JsonWriter(byte[] initialBuffer)
    {
      this.buffer = initialBuffer;
      this.offset = 0;
    }

    public ArraySegment<byte> GetBuffer()
    {
      return this.buffer == null ? new ArraySegment<byte>(JsonWriter.emptyBytes, 0, 0) : new ArraySegment<byte>(this.buffer, 0, this.offset);
    }

    public byte[] ToUtf8ByteArray()
    {
      return this.buffer == null ? JsonWriter.emptyBytes : BinaryUtil.FastCloneWithResize(this.buffer, this.offset);
    }

    public override string ToString()
    {
      return this.buffer == null ? (string) null : Encoding.UTF8.GetString(this.buffer, 0, this.offset);
    }

    public void EnsureCapacity(int appendLength)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, appendLength);
    }

    public void WriteRaw(byte rawValue)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = rawValue;
    }

    public void WriteRaw(byte[] rawValue)
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, rawValue.Length);
      Buffer.BlockCopy((Array) rawValue, 0, (Array) this.buffer, this.offset, rawValue.Length);
      this.offset += rawValue.Length;
    }

    public void WriteRawUnsafe(byte rawValue)
    {
      this.buffer[this.offset++] = rawValue;
    }

    public void WriteBeginArray()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 91;
    }

    public void WriteEndArray()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 93;
    }

    public void WriteBeginObject()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 123;
    }

    public void WriteEndObject()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 125;
    }

    public void WriteValueSeparator()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 44;
    }

    public void WriteNameSeparator()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 58;
    }

    public void WritePropertyName(string propertyName)
    {
      this.WriteString(propertyName);
      this.WriteNameSeparator();
    }

    public void WriteQuotation()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 1);
      this.buffer[this.offset++] = (byte) 34;
    }

    public void WriteNull()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 4);
      this.buffer[this.offset] = (byte) 110;
      this.buffer[this.offset + 1] = (byte) 117;
      this.buffer[this.offset + 2] = (byte) 108;
      this.buffer[this.offset + 3] = (byte) 108;
      this.offset += 4;
    }

    public void WriteBoolean(bool value)
    {
      if (value)
      {
        BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 4);
        this.buffer[this.offset] = (byte) 116;
        this.buffer[this.offset + 1] = (byte) 114;
        this.buffer[this.offset + 2] = (byte) 117;
        this.buffer[this.offset + 3] = (byte) 101;
        this.offset += 4;
      }
      else
      {
        BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 5);
        this.buffer[this.offset] = (byte) 102;
        this.buffer[this.offset + 1] = (byte) 97;
        this.buffer[this.offset + 2] = (byte) 108;
        this.buffer[this.offset + 3] = (byte) 115;
        this.buffer[this.offset + 4] = (byte) 101;
        this.offset += 5;
      }
    }

    public void WriteTrue()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 4);
      this.buffer[this.offset] = (byte) 116;
      this.buffer[this.offset + 1] = (byte) 114;
      this.buffer[this.offset + 2] = (byte) 117;
      this.buffer[this.offset + 3] = (byte) 101;
      this.offset += 4;
    }

    public void WriteFalse()
    {
      BinaryUtil.EnsureCapacity(ref this.buffer, this.offset, 5);
      this.buffer[this.offset] = (byte) 102;
      this.buffer[this.offset + 1] = (byte) 97;
      this.buffer[this.offset + 2] = (byte) 108;
      this.buffer[this.offset + 3] = (byte) 115;
      this.buffer[this.offset + 4] = (byte) 101;
      this.offset += 5;
    }

    public void WriteSingle(float value)
    {
      this.offset += DoubleToStringConverter.GetBytes(ref this.buffer, this.offset, value);
    }

    public void WriteDouble(double value)
    {
      this.offset += DoubleToStringConverter.GetBytes(ref this.buffer, this.offset, value);
    }

    public void WriteByte(byte value)
    {
      this.WriteUInt64((ulong) value);
    }

    public void WriteUInt16(ushort value)
    {
      this.WriteUInt64((ulong) value);
    }

    public void WriteUInt32(uint value)
    {
      this.WriteUInt64((ulong) value);
    }

    public void WriteUInt64(ulong value)
    {
      this.offset += NumberConverter.WriteUInt64(ref this.buffer, this.offset, value);
    }

    public void WriteSByte(sbyte value)
    {
      this.WriteInt64((long) value);
    }

    public void WriteInt16(short value)
    {
      this.WriteInt64((long) value);
    }

    public void WriteInt32(int value)
    {
      this.WriteInt64((long) value);
    }

    public void WriteInt64(long value)
    {
      this.offset += NumberConverter.WriteInt64(ref this.buffer, this.offset, value);
    }

    public void WriteString(string value)
    {
      if (value == null)
      {
        this.WriteNull();
      }
      else
      {
        int offset = this.offset;
        int appendLength = StringEncoding.UTF8.GetMaxByteCount(value.Length) + 2;
        BinaryUtil.EnsureCapacity(ref this.buffer, offset, appendLength);
        int charIndex = 0;
        int length = value.Length;
        this.buffer[this.offset++] = (byte) 34;
        for (int index = 0; index < value.Length; ++index)
        {
          byte num;
          switch (value[index])
          {
            case '\b':
              num = (byte) 98;
              break;
            case '\t':
              num = (byte) 116;
              break;
            case '\n':
              num = (byte) 110;
              break;
            case '\f':
              num = (byte) 102;
              break;
            case '\r':
              num = (byte) 114;
              break;
            case '"':
              num = (byte) 34;
              break;
            case '\\':
              num = (byte) 92;
              break;
            default:
              continue;
          }
          appendLength += 2;
          BinaryUtil.EnsureCapacity(ref this.buffer, offset, appendLength);
          this.offset += StringEncoding.UTF8.GetBytes(value, charIndex, index - charIndex, this.buffer, this.offset);
          charIndex = index + 1;
          this.buffer[this.offset++] = (byte) 92;
          this.buffer[this.offset++] = num;
        }
        if (charIndex != value.Length)
          this.offset += StringEncoding.UTF8.GetBytes(value, charIndex, value.Length - charIndex, this.buffer, this.offset);
        this.buffer[this.offset++] = (byte) 34;
      }
    }
  }
}
