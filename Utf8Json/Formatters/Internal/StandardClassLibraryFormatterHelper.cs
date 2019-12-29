// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Internal.StandardClassLibraryFormatterHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Internal;

namespace Utf8Json.Formatters.Internal
{
  internal static class StandardClassLibraryFormatterHelper
  {
    internal static readonly byte[][] keyValuePairName = new byte[2][]
    {
      JsonWriter.GetEncodedPropertyNameWithBeginObject("Key"),
      JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Value")
    };
    internal static readonly AutomataDictionary keyValuePairAutomata = new AutomataDictionary()
    {
      {
        JsonWriter.GetEncodedPropertyNameWithoutQuotation("Key"),
        0
      },
      {
        JsonWriter.GetEncodedPropertyNameWithoutQuotation("Value"),
        1
      }
    };
  }
}
