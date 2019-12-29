// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.ArrayPool`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Internal
{
  internal class ArrayPool<T>
  {
    private readonly int bufferLength;
    private readonly object gate;
    private int index;
    private T[][] buffers;

    public ArrayPool(int bufferLength)
    {
      this.bufferLength = bufferLength;
      this.buffers = new T[4][];
      this.gate = new object();
    }

    public T[] Rent()
    {
      lock (this.gate)
      {
        if (this.index >= this.buffers.Length)
          Array.Resize<T[]>(ref this.buffers, this.buffers.Length * 2);
        if (this.buffers[this.index] == null)
          this.buffers[this.index] = new T[this.bufferLength];
        T[] buffer = this.buffers[this.index];
        this.buffers[this.index] = (T[]) null;
        ++this.index;
        return buffer;
      }
    }

    public void Return(T[] array)
    {
      if (array.Length != this.bufferLength)
        throw new InvalidOperationException("return buffer is not from pool");
      lock (this.gate)
      {
        if (this.index == 0)
          return;
        this.buffers[--this.index] = array;
      }
    }
  }
}
