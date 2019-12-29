// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.ILStreamReader
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Utf8Json.Internal
{
  internal class ILStreamReader : BinaryReader
  {
    private static readonly OpCode[] oneByteOpCodes = new OpCode[256];
    private static readonly OpCode[] twoByteOpCodes = new OpCode[256];
    private int endPosition;

    public int CurrentPosition
    {
      get
      {
        return (int) this.BaseStream.Position;
      }
    }

    public bool EndOfStream
    {
      get
      {
        return (int) this.BaseStream.Position >= this.endPosition;
      }
    }

    static ILStreamReader()
    {
      foreach (FieldInfo field in typeof (OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public))
      {
        OpCode opCode = (OpCode) field.GetValue((object) null);
        ushort num = (ushort) opCode.Value;
        if (num < (ushort) 256)
          ILStreamReader.oneByteOpCodes[(int) num] = opCode;
        else if (((int) num & 65280) == 65024)
          ILStreamReader.twoByteOpCodes[(int) num & (int) byte.MaxValue] = opCode;
      }
    }

    public ILStreamReader(byte[] ilByteArray)
      : base((Stream) new MemoryStream(ilByteArray))
    {
      this.endPosition = ilByteArray.Length;
    }

    public OpCode ReadOpCode()
    {
      byte num1 = this.ReadByte();
      if (num1 != (byte) 254)
        return ILStreamReader.oneByteOpCodes[(int) num1];
      byte num2 = this.ReadByte();
      return ILStreamReader.twoByteOpCodes[(int) num2];
    }

    public int ReadMetadataToken()
    {
      return this.ReadInt32();
    }
  }
}
