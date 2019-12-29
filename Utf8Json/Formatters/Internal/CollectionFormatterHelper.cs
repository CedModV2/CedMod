// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Internal.CollectionFormatterHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Internal;

namespace Utf8Json.Formatters.Internal
{
  internal static class CollectionFormatterHelper
  {
    internal static readonly byte[][] groupingName = new byte[2][]
    {
      JsonWriter.GetEncodedPropertyNameWithBeginObject("Key"),
      JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Elements")
    };
    internal static readonly AutomataDictionary groupingAutomata = new AutomataDictionary()
    {
      {
        JsonWriter.GetEncodedPropertyNameWithoutQuotation("Key"),
        0
      },
      {
        JsonWriter.GetEncodedPropertyNameWithoutQuotation("Elements"),
        1
      }
    };
  }
}
