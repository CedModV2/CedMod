// Decompiled with JetBrains decompiler
// Type: PlayerPrefsSl
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public static class PlayerPrefsSl
{
  private static Dictionary<string, string> _registry = new Dictionary<string, string>();
  private static readonly UTF8Encoding Encoding = new UTF8Encoding(false);
  private static string _path = FileManager.GetAppFolder(true, false, "") + "registry.txt";
  private const string ArraySeparator = ";;`'.+=;;";
  private const string KeySeparator = "::-%(|::";

  static PlayerPrefsSl()
  {
    PlayerPrefsSl.Refresh();
  }

  private static string Prefix(string key, PlayerPrefsSl.DataType type)
  {
    return ((byte) type).ToString("00") + key;
  }

  public static void Refresh()
  {
    PlayerPrefsSl._registry.Clear();
    if (!File.Exists(PlayerPrefsSl._path))
    {
      File.Create(PlayerPrefsSl._path).Close();
    }
    else
    {
      using (StreamReader streamReader = new StreamReader(PlayerPrefsSl._path))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
        {
          if (str.Contains("::-%(|::"))
          {
            int length = str.IndexOf("::-%(|::", StringComparison.Ordinal);
            PlayerPrefsSl._registry.Add(str.Substring(0, length), str.Substring(length + "::-%(|::".Length));
          }
        }
      }
    }
  }

  private static void Save()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (KeyValuePair<string, string> keyValuePair in PlayerPrefsSl._registry)
    {
      stringBuilder.Append(keyValuePair.Key);
      stringBuilder.Append("::-%(|::");
      stringBuilder.Append(keyValuePair.Value);
      stringBuilder.AppendLine();
    }
    File.WriteAllText(PlayerPrefsSl._path, stringBuilder.ToString(), (System.Text.Encoding) PlayerPrefsSl.Encoding);
  }

  public static bool HasKey(string key, PlayerPrefsSl.DataType type)
  {
    return PlayerPrefsSl._registry.ContainsKey(PlayerPrefsSl.Prefix(key, type));
  }

  public static void DeleteKey(string key, PlayerPrefsSl.DataType type)
  {
    PlayerPrefsSl._registry.Remove(PlayerPrefsSl.Prefix(key, type));
    PlayerPrefsSl.Save();
  }

  public static void DeleteAll()
  {
    File.WriteAllText(PlayerPrefsSl._path, "", (System.Text.Encoding) PlayerPrefsSl.Encoding);
  }

  private static void WriteString(string key, string value)
  {
    if (PlayerPrefsSl._registry.ContainsKey(key))
      PlayerPrefsSl._registry[key] = value;
    else
      PlayerPrefsSl._registry.Add(key, value);
    PlayerPrefsSl.Save();
  }

  public static void Set(string key, bool value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Bool), value ? "true" : "false");
  }

  public static void Set(string key, byte value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Byte), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, sbyte value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Sbyte), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, char value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Char), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, Decimal value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Decimal), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, double value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Double), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, float value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Float), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, int value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Int), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, uint value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Uint), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, long value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Long), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, ulong value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Ulong), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, short value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Short), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, ushort value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Ushort), value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
  }

  public static void Set(string key, string value)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.String), value);
  }

  public static void Set(string key, bool[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.BoolArray), string.Join<bool>(";;`'.+=;;", (IEnumerable<bool>) array));
  }

  public static void Set(string key, IEnumerable<bool> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.BoolArray), string.Join<bool>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, byte[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.ByteArray), string.Join<byte>(";;`'.+=;;", (IEnumerable<byte>) array));
  }

  public static void Set(string key, IEnumerable<byte> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.ByteArray), string.Join<byte>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, sbyte[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.SbyteArray), string.Join<sbyte>(";;`'.+=;;", (IEnumerable<sbyte>) array));
  }

  public static void Set(string key, IEnumerable<sbyte> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.SbyteArray), string.Join<sbyte>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, char[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.CharArray), string.Join<char>(";;`'.+=;;", (IEnumerable<char>) array));
  }

  public static void Set(string key, IEnumerable<char> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.CharArray), string.Join<char>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, Decimal[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.DecimalArray), string.Join<Decimal>(";;`'.+=;;", (IEnumerable<Decimal>) array));
  }

  public static void Set(string key, IEnumerable<Decimal> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.DecimalArray), string.Join<Decimal>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, double[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.DoubleArray), string.Join<double>(";;`'.+=;;", (IEnumerable<double>) array));
  }

  public static void Set(string key, IEnumerable<double> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.DoubleArray), string.Join<double>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, float[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.FloatArray), string.Join<float>(";;`'.+=;;", (IEnumerable<float>) array));
  }

  public static void Set(string key, IEnumerable<float> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.FloatArray), string.Join<float>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, int[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.IntArray), string.Join<int>(";;`'.+=;;", (IEnumerable<int>) array));
  }

  public static void Set(string key, IEnumerable<int> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.IntArray), string.Join<int>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, uint[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UintArray), string.Join<uint>(";;`'.+=;;", (IEnumerable<uint>) array));
  }

  public static void Set(string key, IEnumerable<uint> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UintArray), string.Join<uint>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, long[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.LongArray), string.Join<long>(";;`'.+=;;", (IEnumerable<long>) array));
  }

  public static void Set(string key, IEnumerable<long> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.LongArray), string.Join<long>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, ulong[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UlongArray), string.Join<ulong>(";;`'.+=;;", (IEnumerable<ulong>) array));
  }

  public static void Set(string key, IEnumerable<ulong> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UlongArray), string.Join<ulong>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, short[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.ShortArray), string.Join<short>(";;`'.+=;;", (IEnumerable<short>) array));
  }

  public static void Set(string key, IEnumerable<short> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.ShortArray), string.Join<short>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, ushort[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UshortArray), string.Join<ushort>(";;`'.+=;;", (IEnumerable<ushort>) array));
  }

  public static void Set(string key, IEnumerable<ushort> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UshortArray), string.Join<ushort>(";;`'.+=;;", ienumerable));
  }

  public static void Set(string key, string[] array)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.StringArray), string.Join(";;`'.+=;;", array));
  }

  public static void Set(string key, IEnumerable<string> ienumerable)
  {
    PlayerPrefsSl.WriteString(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.StringArray), string.Join(";;`'.+=;;", ienumerable));
  }

  public static bool Get(string key, bool defaultValue)
  {
    string str;
    bool result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Bool), out str) || !bool.TryParse(str, out result) ? defaultValue : result;
  }

  public static byte Get(string key, byte defaultValue)
  {
    string s;
    byte result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Byte), out s) || !byte.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static sbyte Get(string key, sbyte defaultValue)
  {
    string s;
    sbyte result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Sbyte), out s) || !sbyte.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static char Get(string key, char defaultValue)
  {
    string s;
    char result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Char), out s) || !char.TryParse(s, out result) ? defaultValue : result;
  }

  public static Decimal Get(string key, Decimal defaultValue)
  {
    string s;
    Decimal result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Decimal), out s) || !Decimal.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static double Get(string key, double defaultValue)
  {
    string s;
    double result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Double), out s) || !double.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static float Get(string key, float defaultValue)
  {
    string s;
    float result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Float), out s) || !float.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static int Get(string key, int defaultValue)
  {
    string s;
    int result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Int), out s) || !int.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static uint Get(string key, uint defaultValue)
  {
    string s;
    uint result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Uint), out s) || !uint.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static long Get(string key, long defaultValue)
  {
    string s;
    long result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Long), out s) || !long.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static ulong Get(string key, ulong defaultValue)
  {
    string s;
    ulong result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Ulong), out s) || !ulong.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static short Get(string key, short defaultValue)
  {
    string s;
    short result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Short), out s) || !short.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static ushort Get(string key, ushort defaultValue)
  {
    string s;
    ushort result;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.Int), out s) || !ushort.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result) ? defaultValue : result;
  }

  public static string Get(string key, string defaultValue)
  {
    string str;
    return !PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.String), out str) ? defaultValue : str;
  }

  public static bool[] Get(string key, bool[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.BoolArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    bool[] flagArray = new bool[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!bool.TryParse(strArray[index], out flagArray[index]))
        return defaultValue;
    }
    return flagArray;
  }

  public static byte[] Get(string key, byte[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.ByteArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    byte[] numArray = new byte[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!byte.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static sbyte[] Get(string key, sbyte[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.SbyteArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    sbyte[] numArray = new sbyte[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!sbyte.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static char[] Get(string key, char[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.CharArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    char[] chArray = new char[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!char.TryParse(strArray[index], out chArray[index]))
        return defaultValue;
    }
    return chArray;
  }

  public static Decimal[] Get(string key, Decimal[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.DecimalArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    Decimal[] numArray = new Decimal[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!Decimal.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static double[] Get(string key, double[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.DoubleArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    double[] numArray = new double[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!double.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static float[] Get(string key, float[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.FloatArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    float[] numArray = new float[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!float.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static int[] Get(string key, int[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.IntArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    int[] numArray = new int[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!int.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static uint[] Get(string key, uint[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UintArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    uint[] numArray = new uint[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!uint.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static long[] Get(string key, long[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.LongArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    long[] numArray = new long[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!long.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static ulong[] Get(string key, ulong[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UlongArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    ulong[] numArray = new ulong[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!ulong.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static short[] Get(string key, short[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.ShortArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    short[] numArray = new short[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!short.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static ushort[] Get(string key, ushort[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.UshortArray), out str))
      return defaultValue;
    string[] strArray = str.Split(new string[1]
    {
      ";;`'.+=;;"
    }, StringSplitOptions.None);
    ushort[] numArray = new ushort[strArray.Length];
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (!ushort.TryParse(strArray[index], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out numArray[index]))
        return defaultValue;
    }
    return numArray;
  }

  public static string[] Get(string key, string[] defaultValue)
  {
    string str;
    if (!PlayerPrefsSl._registry.TryGetValue(PlayerPrefsSl.Prefix(key, PlayerPrefsSl.DataType.StringArray), out str))
      return defaultValue;
    return str.Split(new string[1]{ ";;`'.+=;;" }, StringSplitOptions.None);
  }

  public enum DataType : byte
  {
    Bool,
    Byte,
    Sbyte,
    Char,
    Decimal,
    Double,
    Float,
    Int,
    Uint,
    Long,
    Ulong,
    Short,
    Ushort,
    String,
    BoolArray,
    ByteArray,
    SbyteArray,
    CharArray,
    DecimalArray,
    DoubleArray,
    FloatArray,
    IntArray,
    UintArray,
    LongArray,
    UlongArray,
    ShortArray,
    UshortArray,
    StringArray,
  }
}
