// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.StackFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class StackFormatter<T> : CollectionFormatterBase<T, ArrayBuffer<T>, Stack<T>.Enumerator, Stack<T>>
  {
    protected override void Add(ref ArrayBuffer<T> collection, int index, T value)
    {
      collection.Add(value);
    }

    protected override ArrayBuffer<T> Create()
    {
      return new ArrayBuffer<T>(4);
    }

    protected override Stack<T>.Enumerator GetSourceEnumerator(Stack<T> source)
    {
      return source.GetEnumerator();
    }

    protected override Stack<T> Complete(ref ArrayBuffer<T> intermediateCollection)
    {
      T[] buffer = intermediateCollection.Buffer;
      Stack<T> objStack = new Stack<T>(intermediateCollection.Size);
      for (int index = intermediateCollection.Size - 1; index >= 0; --index)
        objStack.Push(buffer[index]);
      return objStack;
    }
  }
}
