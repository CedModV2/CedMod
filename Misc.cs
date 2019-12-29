// Decompiled with JetBrains decompiler
// Type: Misc
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Misc
{
  public static string LeadingZeroes(int integer, uint len, bool plusSign = false)
  {
    bool flag = integer < 0;
    if (flag)
      integer *= -1;
    string str = integer.ToString();
    while ((long) str.Length < (long) len)
      str = "0" + str;
    return (flag ? "-" : (plusSign ? "+" : "")) + str;
  }

  public static string RemoveSpecialCharacters(string str)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (char ch in str)
    {
      if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || (ch >= 'a' && ch <= 'z' || (ch == ' ' || ch == '-')) || (ch == '.' || ch == ',' || ch == '_'))
        stringBuilder.Append(ch);
    }
    return stringBuilder.ToString();
  }

  public static string StripTag(string input, string tag)
  {
    return Regex.Replace(input, "<.*?" + tag + ".*?>", string.Empty);
  }

  public static string StripTags(string input)
  {
    return Regex.Replace(input, "<.*?>", string.Empty);
  }

  public static int LevenshteinDistance(string s, string t)
  {
    int length1 = s.Length;
    int length2 = t.Length;
    int[,] numArray = new int[length1 + 1, length2 + 1];
    if (length1 == 0)
      return length2;
    if (length2 == 0)
      return length1;
    int index1 = 0;
    while (index1 <= length1)
      numArray[index1, 0] = index1++;
    int index2 = 0;
    while (index2 <= length2)
      numArray[0, index2] = index2++;
    for (int index3 = 1; index3 <= length1; ++index3)
    {
      for (int index4 = 1; index4 <= length2; ++index4)
      {
        int num = (int) t[index4 - 1] == (int) s[index3 - 1] ? 0 : 1;
        numArray[index3, index4] = Math.Min(Math.Min(numArray[index3 - 1, index4] + 1, numArray[index3, index4 - 1] + 1), numArray[index3 - 1, index4 - 1] + num);
      }
    }
    return numArray[length1, length2];
  }

  public static string LongestCommonSubstring(string a, string b)
  {
    int[,] numArray = new int[a.Length, b.Length];
    int length = 0;
    string str = "";
    for (int index1 = 0; index1 < a.Length; ++index1)
    {
      for (int index2 = 0; index2 < b.Length; ++index2)
      {
        if ((int) a[index1] == (int) b[index2])
        {
          numArray[index1, index2] = index1 == 0 || index2 == 0 ? 1 : numArray[index1 - 1, index2 - 1] + 1;
          if (numArray[index1, index2] > length)
          {
            length = numArray[index1, index2];
            str = a.Substring(index1 - length + 1, length);
          }
        }
        else
          numArray[index1, index2] = 0;
      }
    }
    return str;
  }

  private static string LongestCommonSubstringOfAInB(string a, string b)
  {
    if (b.Length < a.Length)
    {
      string str = a;
      a = b;
      b = str;
    }
    for (int length = a.Length; length > 0; --length)
    {
      for (int startIndex = a.Length - length; startIndex <= a.Length - length; ++startIndex)
      {
        string str = a.Substring(startIndex, length);
        if (b.Contains(str))
          return str;
      }
    }
    return "";
  }

  public static string Base64Encode(string plainText)
  {
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
  }

  public static string Base64Decode(string base64EncodedData)
  {
    return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
  }

  public static string ValidateIp(string text)
  {
    return new Regex("[^a-zA-Z0-9\\.\\:\\[\\]]").Replace(text, "");
  }

  public static bool ValidatePastebin(string text)
  {
    return new Regex("^[a-zA-Z0-9]{8}$").IsMatch(text);
  }

  public static string GetRuntimeVersion()
  {
    try
    {
      return RuntimeInformation.get_FrameworkDescription();
    }
    catch
    {
      return "Not supported!";
    }
  }

  public static AudioType GetAudioType(string path)
  {
    switch (Path.GetExtension(path))
    {
      case ".aac":
        return AudioType.ACC;
      case ".aiff":
        return AudioType.AIFF;
      case ".mod":
        return AudioType.MOD;
      case ".mp2":
      case ".mp3":
      case ".mpeg":
        return AudioType.MPEG;
      case ".ogg":
        return AudioType.OGGVORBIS;
      case ".wav":
        return AudioType.WAV;
      default:
        return AudioType.UNKNOWN;
    }
  }

  public static bool CultureInfoTryParse(string name, out CultureInfo info)
  {
    try
    {
      info = CultureInfo.GetCultureInfo(name);
      return true;
    }
    catch
    {
      info = (CultureInfo) null;
      return false;
    }
  }

  public static string ToHex(this Color color)
  {
    Color32 color32 = (Color32) color;
    return "#" + color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2");
  }

  public static string ToHex(this Color32 color)
  {
    return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
  }

  public static bool Contains(this string s, string value, StringComparison comparison)
  {
    return s.IndexOf(value, comparison) >= 0;
  }

  public static bool ParseVersion(out byte major, out byte minor)
  {
    try
    {
      string[] strArray = CustomNetworkManager.CompatibleVersions[0].Split('.');
      if (strArray.Length > 1 && byte.TryParse(strArray[0], out major) && byte.TryParse(strArray[1], out minor))
        return true;
      major = (byte) 0;
      minor = (byte) 0;
      return false;
    }
    catch
    {
      major = (byte) 0;
      minor = (byte) 0;
      return false;
    }
  }
}
