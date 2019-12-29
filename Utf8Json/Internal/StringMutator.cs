// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.StringMutator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Text;

namespace Utf8Json.Internal
{
  internal static class StringMutator
  {
    public static string Original(string s)
    {
      return s;
    }

    public static string ToCamelCase(string s)
    {
      if (string.IsNullOrEmpty(s) || char.IsLower(s, 0))
        return s;
      char[] charArray = s.ToCharArray();
      charArray[0] = char.ToLowerInvariant(charArray[0]);
      return new string(charArray);
    }

    public static string ToSnakeCase(string s)
    {
      if (string.IsNullOrEmpty(s))
        return s;
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < s.Length; ++index)
      {
        char c = s[index];
        if (char.IsUpper(c))
        {
          if (index == 0)
            stringBuilder.Append(char.ToLowerInvariant(c));
          else if (char.IsUpper(s[index - 1]))
          {
            stringBuilder.Append(char.ToLowerInvariant(c));
          }
          else
          {
            stringBuilder.Append("_");
            stringBuilder.Append(char.ToLowerInvariant(c));
          }
        }
        else
          stringBuilder.Append(c);
      }
      return stringBuilder.ToString();
    }
  }
}
