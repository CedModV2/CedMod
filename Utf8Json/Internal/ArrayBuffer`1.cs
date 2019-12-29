// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.ArrayBuffer`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Internal
{
  public struct ArrayBuffer<T>
  {
    public T[] Buffer;
    public int Size;

    public ArrayBuffer(int initialSize)
    {
      this.Buffer = new T[initialSize];
      this.Size = 0;
    }

    public void Add(T value)
    {
      if (this.Size >= this.Buffer.Length)
        Array.Resize<T>(ref this.Buffer, this.Size * 2);
      this.Buffer[this.Size++] = value;
    }

    public T[] ToArray()
    {
      if (this.Buffer.Length == this.Size)
        return this.Buffer;
      T[] objArray = new T[this.Size];
      Array.Copy((Array) this.Buffer, (Array) objArray, this.Size);
      return objArray;
    }
  }
}
