// Decompiled with JetBrains decompiler
// Type: Utf8Json.JsonReader
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Text;
using Utf8Json.Internal;
using Utf8Json.Internal.DoubleConversion;

namespace Utf8Json
{
  public struct JsonReader
  {
    private static readonly ArraySegment<byte> nullTokenSegment = new ArraySegment<byte>(new byte[4]
    {
      (byte) 110,
      (byte) 117,
      (byte) 108,
      (byte) 108
    }, 0, 4);
    private static readonly byte[] bom = Encoding.UTF8.GetPreamble();
    private readonly byte[] bytes;
    private int offset;

    public JsonReader(byte[] bytes)
      : this(bytes, 0)
    {
    }

    public JsonReader(byte[] bytes, int offset)
    {
      this.bytes = bytes;
      this.offset = offset;
      if (bytes.Length < 3 || (int) bytes[offset] != (int) JsonReader.bom[0] || ((int) bytes[offset + 1] != (int) JsonReader.bom[1] || (int) bytes[offset + 2] != (int) JsonReader.bom[2]))
        return;
      this.offset = (offset += 3);
    }

    private JsonParsingException CreateParsingException(string expected)
    {
      string actualChar = ((char) this.bytes[this.offset]).ToString();
      int offset = this.offset;
      try
      {
        switch (this.GetCurrentJsonToken())
        {
          case JsonToken.Number:
            ArraySegment<byte> arraySegment = this.ReadNumberSegment();
            actualChar = StringEncoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
            break;
          case JsonToken.String:
            actualChar = "\"" + this.ReadString() + "\"";
            break;
          case JsonToken.True:
            actualChar = "true";
            break;
          case JsonToken.False:
            actualChar = "false";
            break;
          case JsonToken.Null:
            actualChar = "null";
            break;
        }
      }
      catch
      {
      }
      return new JsonParsingException("expected:'" + expected + "', actual:'" + actualChar + "', at offset:" + (object) offset, this.bytes, offset, this.offset, actualChar);
    }

    private JsonParsingException CreateParsingExceptionMessage(string message)
    {
      string actualChar = ((char) this.bytes[this.offset]).ToString();
      int offset = this.offset;
      return new JsonParsingException(message, this.bytes, offset, offset, actualChar);
    }

    private bool IsInRange
    {
      get
      {
        return this.offset < this.bytes.Length;
      }
    }

    public void AdvanceOffset(int offset)
    {
      this.offset += offset;
    }

    public byte[] GetBufferUnsafe()
    {
      return this.bytes;
    }

    public int GetCurrentOffsetUnsafe()
    {
      return this.offset;
    }

    public JsonToken GetCurrentJsonToken()
    {
      this.SkipWhiteSpace();
      if (this.offset >= this.bytes.Length)
        return JsonToken.None;
      switch (this.bytes[this.offset])
      {
        case 34:
          return JsonToken.String;
        case 44:
          return JsonToken.ValueSeparator;
        case 45:
          return JsonToken.Number;
        case 48:
          return JsonToken.Number;
        case 49:
          return JsonToken.Number;
        case 50:
          return JsonToken.Number;
        case 51:
          return JsonToken.Number;
        case 52:
          return JsonToken.Number;
        case 53:
          return JsonToken.Number;
        case 54:
          return JsonToken.Number;
        case 55:
          return JsonToken.Number;
        case 56:
          return JsonToken.Number;
        case 57:
          return JsonToken.Number;
        case 58:
          return JsonToken.NameSeparator;
        case 91:
          return JsonToken.BeginArray;
        case 93:
          return JsonToken.EndArray;
        case 102:
          return JsonToken.False;
        case 110:
          return JsonToken.Null;
        case 116:
          return JsonToken.True;
        case 123:
          return JsonToken.BeginObject;
        case 125:
          return JsonToken.EndObject;
        default:
          return JsonToken.None;
      }
    }

    public void SkipWhiteSpace()
    {
      for (int offset = this.offset; offset < this.bytes.Length; ++offset)
      {
        switch (this.bytes[offset])
        {
          case 9:
          case 10:
          case 13:
          case 32:
            continue;
          case 47:
            offset = JsonReader.ReadComment(this.bytes, offset);
            continue;
          default:
            this.offset = offset;
            return;
        }
      }
      this.offset = this.bytes.Length;
    }

    public bool ReadIsNull()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 110)
        return false;
      if (this.bytes[this.offset + 1] != (byte) 117 || this.bytes[this.offset + 2] != (byte) 108 || this.bytes[this.offset + 3] != (byte) 108)
        throw this.CreateParsingException("null");
      this.offset += 4;
      return true;
    }

    public bool ReadIsBeginArray()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 91)
        return false;
      ++this.offset;
      return true;
    }

    public void ReadIsBeginArrayWithVerify()
    {
      if (!this.ReadIsBeginArray())
        throw this.CreateParsingException("[");
    }

    public bool ReadIsEndArray()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 93)
        return false;
      ++this.offset;
      return true;
    }

    public void ReadIsEndArrayWithVerify()
    {
      if (!this.ReadIsEndArray())
        throw this.CreateParsingException("]");
    }

    public bool ReadIsEndArrayWithSkipValueSeparator(ref int count)
    {
      this.SkipWhiteSpace();
      if (this.IsInRange && this.bytes[this.offset] == (byte) 93)
      {
        ++this.offset;
        return true;
      }
      if (count++ != 0)
        this.ReadIsValueSeparatorWithVerify();
      return false;
    }

    public bool ReadIsInArray(ref int count)
    {
      if (count == 0)
      {
        this.ReadIsBeginArrayWithVerify();
        if (this.ReadIsEndArray())
          return false;
      }
      else
      {
        if (this.ReadIsEndArray())
          return false;
        this.ReadIsValueSeparatorWithVerify();
      }
      ++count;
      return true;
    }

    public bool ReadIsBeginObject()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 123)
        return false;
      ++this.offset;
      return true;
    }

    public void ReadIsBeginObjectWithVerify()
    {
      if (!this.ReadIsBeginObject())
        throw this.CreateParsingException("{");
    }

    public bool ReadIsEndObject()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 125)
        return false;
      ++this.offset;
      return true;
    }

    public void ReadIsEndObjectWithVerify()
    {
      if (!this.ReadIsEndObject())
        throw this.CreateParsingException("}");
    }

    public bool ReadIsEndObjectWithSkipValueSeparator(ref int count)
    {
      this.SkipWhiteSpace();
      if (this.IsInRange && this.bytes[this.offset] == (byte) 125)
      {
        ++this.offset;
        return true;
      }
      if (count++ != 0)
        this.ReadIsValueSeparatorWithVerify();
      return false;
    }

    public bool ReadIsInObject(ref int count)
    {
      if (count == 0)
      {
        this.ReadIsBeginObjectWithVerify();
        if (this.ReadIsEndObject())
          return false;
      }
      else
      {
        if (this.ReadIsEndObject())
          return false;
        this.ReadIsValueSeparatorWithVerify();
      }
      ++count;
      return true;
    }

    public bool ReadIsValueSeparator()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 44)
        return false;
      ++this.offset;
      return true;
    }

    public void ReadIsValueSeparatorWithVerify()
    {
      if (!this.ReadIsValueSeparator())
        throw this.CreateParsingException(",");
    }

    public bool ReadIsNameSeparator()
    {
      this.SkipWhiteSpace();
      if (!this.IsInRange || this.bytes[this.offset] != (byte) 58)
        return false;
      ++this.offset;
      return true;
    }

    public void ReadIsNameSeparatorWithVerify()
    {
      if (!this.ReadIsNameSeparator())
        throw this.CreateParsingException(":");
    }

    private void ReadStringSegmentCore(
      out byte[] resultBytes,
      out int resultOffset,
      out int resultLength)
    {
      byte[] bytes = (byte[]) null;
      int num1 = 0;
      char[] array = (char[]) null;
      int charCount = 0;
      if (this.bytes[this.offset] != (byte) 34)
        throw this.CreateParsingException("String Begin Token");
      ++this.offset;
      int offset1 = this.offset;
      for (int offset2 = this.offset; offset2 < this.bytes.Length; ++offset2)
      {
        switch (this.bytes[offset2])
        {
          case 34:
            ++this.offset;
            if (num1 == 0 && charCount == 0)
            {
              resultBytes = this.bytes;
              resultOffset = offset1;
              resultLength = this.offset - 1 - offset1;
              return;
            }
            if (bytes == null)
              bytes = JsonReader.StringBuilderCache.GetBuffer();
            if (charCount != 0)
            {
              BinaryUtil.EnsureCapacity(ref bytes, num1, StringEncoding.UTF8.GetMaxByteCount(charCount));
              num1 += StringEncoding.UTF8.GetBytes(array, 0, charCount, bytes, num1);
            }
            int num2 = this.offset - offset1 - 1;
            BinaryUtil.EnsureCapacity(ref bytes, num1, num2);
            Buffer.BlockCopy((Array) this.bytes, offset1, (Array) bytes, num1, num2);
            int num3 = num1 + num2;
            resultBytes = bytes;
            resultOffset = 0;
            resultLength = num3;
            return;
          case 92:
            byte num4;
            switch ((char) this.bytes[offset2 + 1])
            {
              case '"':
              case '/':
              case '\\':
                num4 = this.bytes[offset2 + 1];
                break;
              case 'b':
                num4 = (byte) 8;
                break;
              case 'f':
                num4 = (byte) 12;
                break;
              case 'n':
                num4 = (byte) 10;
                break;
              case 'r':
                num4 = (byte) 13;
                break;
              case 't':
                num4 = (byte) 9;
                break;
              case 'u':
                if (array == null)
                  array = JsonReader.StringBuilderCache.GetCodePointStringBuffer();
                if (charCount == 0)
                {
                  if (bytes == null)
                    bytes = JsonReader.StringBuilderCache.GetBuffer();
                  int count = offset2 - offset1;
                  BinaryUtil.EnsureCapacity(ref bytes, num1, count + 1);
                  Buffer.BlockCopy((Array) this.bytes, offset1, (Array) bytes, num1, count);
                  num1 += count;
                }
                if (array.Length == charCount)
                  Array.Resize<char>(ref array, array.Length * 2);
                int num5 = (int) this.bytes[offset2 + 2];
                char ch1 = (char) this.bytes[offset2 + 3];
                char ch2 = (char) this.bytes[offset2 + 4];
                char ch3 = (char) this.bytes[offset2 + 5];
                int num6 = (int) ch1;
                int num7 = (int) ch2;
                int num8 = (int) ch3;
                int codePoint = JsonReader.GetCodePoint((char) num5, (char) num6, (char) num7, (char) num8);
                array[charCount++] = (char) codePoint;
                offset2 += 5;
                this.offset += 6;
                offset1 = this.offset;
                continue;
              default:
                throw this.CreateParsingExceptionMessage("Bad JSON escape.");
            }
            if (bytes == null)
              bytes = JsonReader.StringBuilderCache.GetBuffer();
            if (charCount != 0)
            {
              BinaryUtil.EnsureCapacity(ref bytes, num1, StringEncoding.UTF8.GetMaxByteCount(charCount));
              num1 += StringEncoding.UTF8.GetBytes(array, 0, charCount, bytes, num1);
              charCount = 0;
            }
            int count1 = offset2 - offset1;
            BinaryUtil.EnsureCapacity(ref bytes, num1, count1 + 1);
            Buffer.BlockCopy((Array) this.bytes, offset1, (Array) bytes, num1, count1);
            int num9 = num1 + count1;
            byte[] numArray = bytes;
            int index = num9;
            num1 = index + 1;
            int num10 = (int) num4;
            numArray[index] = (byte) num10;
            ++offset2;
            this.offset += 2;
            offset1 = this.offset;
            break;
          default:
            if (charCount != 0)
            {
              if (bytes == null)
                bytes = JsonReader.StringBuilderCache.GetBuffer();
              BinaryUtil.EnsureCapacity(ref bytes, num1, StringEncoding.UTF8.GetMaxByteCount(charCount));
              num1 += StringEncoding.UTF8.GetBytes(array, 0, charCount, bytes, num1);
              charCount = 0;
            }
            ++this.offset;
            break;
        }
      }
      resultLength = 0;
      resultBytes = (byte[]) null;
      resultOffset = 0;
      throw this.CreateParsingException("String End Token");
    }

    private static int GetCodePoint(char a, char b, char c, char d)
    {
      return ((JsonReader.ToNumber(a) * 16 + JsonReader.ToNumber(b)) * 16 + JsonReader.ToNumber(c)) * 16 + JsonReader.ToNumber(d);
    }

    private static int ToNumber(char x)
    {
      if ('0' <= x && x <= '9')
        return (int) x - 48;
      if ('a' <= x && x <= 'f')
        return (int) x - 97 + 10;
      if ('A' <= x && x <= 'F')
        return (int) x - 65 + 10;
      throw new JsonParsingException("Invalid Character" + x.ToString());
    }

    public ArraySegment<byte> ReadStringSegmentUnsafe()
    {
      if (this.ReadIsNull())
        return JsonReader.nullTokenSegment;
      byte[] resultBytes;
      int resultOffset;
      int resultLength;
      this.ReadStringSegmentCore(out resultBytes, out resultOffset, out resultLength);
      return new ArraySegment<byte>(resultBytes, resultOffset, resultLength);
    }

    public string ReadString()
    {
      if (this.ReadIsNull())
        return (string) null;
      byte[] resultBytes;
      int resultOffset;
      int resultLength;
      this.ReadStringSegmentCore(out resultBytes, out resultOffset, out resultLength);
      return Encoding.UTF8.GetString(resultBytes, resultOffset, resultLength);
    }

    public string ReadPropertyName()
    {
      string str = this.ReadString();
      this.ReadIsNameSeparatorWithVerify();
      return str;
    }

    public ArraySegment<byte> ReadStringSegmentRaw()
    {
      ArraySegment<byte> arraySegment = new ArraySegment<byte>();
      if (this.ReadIsNull())
      {
        arraySegment = JsonReader.nullTokenSegment;
      }
      else
      {
        if (this.bytes[this.offset++] != (byte) 34)
          throw this.CreateParsingException("\"");
        int offset1 = this.offset;
        for (int offset2 = this.offset; offset2 < this.bytes.Length; ++offset2)
        {
          if (this.bytes[offset2] == (byte) 34 && this.bytes[offset2 - 1] != (byte) 92)
          {
            this.offset = offset2 + 1;
            arraySegment = new ArraySegment<byte>(this.bytes, offset1, this.offset - offset1 - 1);
            goto label_10;
          }
        }
        throw this.CreateParsingExceptionMessage("not found end string.");
      }
label_10:
      return arraySegment;
    }

    public ArraySegment<byte> ReadPropertyNameSegmentRaw()
    {
      ArraySegment<byte> arraySegment = this.ReadStringSegmentRaw();
      this.ReadIsNameSeparatorWithVerify();
      return arraySegment;
    }

    public bool ReadBoolean()
    {
      this.SkipWhiteSpace();
      if (this.bytes[this.offset] == (byte) 116)
      {
        if (this.bytes[this.offset + 1] != (byte) 114 || this.bytes[this.offset + 2] != (byte) 117 || this.bytes[this.offset + 3] != (byte) 101)
          throw this.CreateParsingException("true");
        this.offset += 4;
        return true;
      }
      if (this.bytes[this.offset] != (byte) 102)
        throw this.CreateParsingException("true | false");
      if (this.bytes[this.offset + 1] != (byte) 97 || this.bytes[this.offset + 2] != (byte) 108 || (this.bytes[this.offset + 3] != (byte) 115 || this.bytes[this.offset + 4] != (byte) 101))
        throw this.CreateParsingException("false");
      this.offset += 5;
      return false;
    }

    private static bool IsWordBreak(byte c)
    {
      switch (c)
      {
        case 32:
        case 34:
        case 44:
        case 58:
        case 91:
        case 93:
        case 123:
        case 125:
          return true;
        default:
          return false;
      }
    }

    public void ReadNext()
    {
      this.ReadNextCore(this.GetCurrentJsonToken());
    }

    private void ReadNextCore(JsonToken token)
    {
      switch (token)
      {
        case JsonToken.BeginObject:
        case JsonToken.EndObject:
        case JsonToken.BeginArray:
        case JsonToken.EndArray:
        case JsonToken.ValueSeparator:
        case JsonToken.NameSeparator:
          ++this.offset;
          break;
        case JsonToken.Number:
          for (int offset = this.offset; offset < this.bytes.Length; ++offset)
          {
            if (JsonReader.IsWordBreak(this.bytes[offset]))
            {
              this.offset = offset;
              return;
            }
          }
          this.offset = this.bytes.Length;
          break;
        case JsonToken.String:
          ++this.offset;
          for (int offset = this.offset; offset < this.bytes.Length; ++offset)
          {
            if (this.bytes[offset] == (byte) 34 && this.bytes[offset - 1] != (byte) 92)
            {
              this.offset = offset + 1;
              return;
            }
          }
          throw this.CreateParsingExceptionMessage("not found end string.");
        case JsonToken.True:
        case JsonToken.Null:
          this.offset += 4;
          break;
        case JsonToken.False:
          this.offset += 5;
          break;
      }
    }

    public void ReadNextBlock()
    {
      this.ReadNextBlockCore(0);
    }

    private void ReadNextBlockCore(int stack)
    {
      JsonToken currentJsonToken = this.GetCurrentJsonToken();
      switch (currentJsonToken)
      {
        case JsonToken.BeginObject:
        case JsonToken.BeginArray:
          ++this.offset;
          this.ReadNextBlockCore(stack + 1);
          break;
        case JsonToken.EndObject:
        case JsonToken.EndArray:
          ++this.offset;
          if (stack - 1 == 0)
            break;
          this.ReadNextBlockCore(stack - 1);
          break;
        case JsonToken.Number:
        case JsonToken.String:
        case JsonToken.True:
        case JsonToken.False:
        case JsonToken.Null:
        case JsonToken.ValueSeparator:
        case JsonToken.NameSeparator:
          do
          {
            this.ReadNextCore(currentJsonToken);
            currentJsonToken = this.GetCurrentJsonToken();
          }
          while (stack != 0 && currentJsonToken >= JsonToken.Number);
          if (stack == 0)
            break;
          this.ReadNextBlockCore(stack);
          break;
      }
    }

    public ArraySegment<byte> ReadNextBlockSegment()
    {
      int offset = this.offset;
      this.ReadNextBlock();
      return new ArraySegment<byte>(this.bytes, offset, this.offset - offset);
    }

    public sbyte ReadSByte()
    {
      return checked ((sbyte) this.ReadInt64());
    }

    public short ReadInt16()
    {
      return checked ((short) this.ReadInt64());
    }

    public int ReadInt32()
    {
      return checked ((int) this.ReadInt64());
    }

    public long ReadInt64()
    {
      this.SkipWhiteSpace();
      int readCount;
      long num = NumberConverter.ReadInt64(this.bytes, this.offset, out readCount);
      if (readCount == 0)
        throw this.CreateParsingException("Number Token");
      this.offset += readCount;
      return num;
    }

    public byte ReadByte()
    {
      return checked ((byte) this.ReadUInt64());
    }

    public ushort ReadUInt16()
    {
      return checked ((ushort) this.ReadUInt64());
    }

    public uint ReadUInt32()
    {
      return checked ((uint) this.ReadUInt64());
    }

    public ulong ReadUInt64()
    {
      this.SkipWhiteSpace();
      int readCount;
      long num = (long) NumberConverter.ReadUInt64(this.bytes, this.offset, out readCount);
      if (readCount == 0)
        throw this.CreateParsingException("Number Token");
      this.offset += readCount;
      return (ulong) num;
    }

    public float ReadSingle()
    {
      this.SkipWhiteSpace();
      int readCount;
      double single = (double) StringToDoubleConverter.ToSingle(this.bytes, this.offset, out readCount);
      if (readCount == 0)
        throw this.CreateParsingException("Number Token");
      this.offset += readCount;
      return (float) single;
    }

    public double ReadDouble()
    {
      this.SkipWhiteSpace();
      int readCount;
      double num = StringToDoubleConverter.ToDouble(this.bytes, this.offset, out readCount);
      if (readCount == 0)
        throw this.CreateParsingException("Number Token");
      this.offset += readCount;
      return num;
    }

    public ArraySegment<byte> ReadNumberSegment()
    {
      this.SkipWhiteSpace();
      int offset1 = this.offset;
      for (int offset2 = this.offset; offset2 < this.bytes.Length; ++offset2)
      {
        if (!NumberConverter.IsNumberRepresentation(this.bytes[offset2]))
        {
          this.offset = offset2;
          goto label_6;
        }
      }
      this.offset = this.bytes.Length;
label_6:
      return new ArraySegment<byte>(this.bytes, offset1, this.offset - offset1);
    }

    private static int ReadComment(byte[] bytes, int offset)
    {
      if (bytes[offset + 1] == (byte) 47)
      {
        offset += 2;
        for (int index = offset; index < bytes.Length; ++index)
        {
          if (bytes[index] == (byte) 13 || bytes[index] == (byte) 10)
            return index;
        }
        throw new JsonParsingException("Can not find end token of single line comment(\r or \n).");
      }
      if (bytes[offset + 1] == (byte) 42)
      {
        offset += 2;
        for (int index = offset; index < bytes.Length; ++index)
        {
          if (bytes[index] == (byte) 42 && bytes[index + 1] == (byte) 47)
            return index + 1;
        }
        throw new JsonParsingException("Can not find end token of multi line comment(*/).");
      }
      return offset;
    }

    internal static class StringBuilderCache
    {
      [ThreadStatic]
      private static byte[] buffer;
      [ThreadStatic]
      private static char[] codePointStringBuffer;

      public static byte[] GetBuffer()
      {
        if (JsonReader.StringBuilderCache.buffer == null)
          JsonReader.StringBuilderCache.buffer = new byte[(int) ushort.MaxValue];
        return JsonReader.StringBuilderCache.buffer;
      }

      public static char[] GetCodePointStringBuffer()
      {
        if (JsonReader.StringBuilderCache.codePointStringBuffer == null)
          JsonReader.StringBuilderCache.codePointStringBuffer = new char[(int) ushort.MaxValue];
        return JsonReader.StringBuilderCache.codePointStringBuffer;
      }
    }
  }
}
