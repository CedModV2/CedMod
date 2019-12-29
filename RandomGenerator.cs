// Decompiled with JetBrains decompiler
// Type: RandomGenerator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Security.Cryptography;

public static class RandomGenerator
{
  private static readonly Random Random = new Random();
  private static readonly byte[] OneByte = new byte[1];
  private static readonly byte[] TwoBytes = new byte[2];
  private static readonly byte[] FourBytes = new byte[4];
  private static readonly byte[] EightBytes = new byte[8];
  private static readonly byte[] SixteenBytes = new byte[16];
  private static readonly bool CryptoRng = ConfigFile.ServerConfig.GetBool("use_crypto_rng", false);

  public static bool GetBool(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetBoolUnsecure() : RandomGenerator.GetBoolSecure();
  }

  private static bool GetBoolSecure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      return RandomGenerator.OneByte[0] > (byte) 127;
    }
  }

  private static bool GetBoolUnsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    return RandomGenerator.OneByte[0] > (byte) 127;
  }

  public static byte[] GetBytes(int count, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetBytesUnsecure(count) : RandomGenerator.GetBytesSecure(count);
  }

  private static byte[] GetBytesSecure(int count)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      byte[] data = new byte[count];
      cryptoServiceProvider.GetBytes(data);
      return data;
    }
  }

  private static byte[] GetBytesUnsecure(int count)
  {
    byte[] buffer = new byte[count];
    RandomGenerator.Random.NextBytes(buffer);
    return buffer;
  }

  public static byte GetByte(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetByteUnsecure() : RandomGenerator.GetByteSecure();
  }

  private static byte GetByteSecure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      return RandomGenerator.OneByte[0];
    }
  }

  private static byte GetByteUnsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    return RandomGenerator.OneByte[0];
  }

  public static sbyte GetSByte(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetSByteUnsecure() : RandomGenerator.GetSByteSecure();
  }

  private static sbyte GetSByteSecure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      return (sbyte) RandomGenerator.OneByte[0];
    }
  }

  private static sbyte GetSByteUnsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    return (sbyte) RandomGenerator.OneByte[0];
  }

  public static short GetInt16(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetInt16Unsecure() : RandomGenerator.GetInt16Secure();
  }

  private static short GetInt16Secure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.TwoBytes);
      return BitConverter.ToInt16(RandomGenerator.TwoBytes, 0);
    }
  }

  private static short GetInt16Unsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.TwoBytes);
    return BitConverter.ToInt16(RandomGenerator.TwoBytes, 0);
  }

  public static ushort GetUInt16(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetUInt16Unsecure() : RandomGenerator.GetUInt16Secure();
  }

  private static ushort GetUInt16Secure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.TwoBytes);
      return BitConverter.ToUInt16(RandomGenerator.TwoBytes, 0);
    }
  }

  private static ushort GetUInt16Unsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.TwoBytes);
    return BitConverter.ToUInt16(RandomGenerator.TwoBytes, 0);
  }

  public static int GetInt32(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetInt32Unsecure() : RandomGenerator.GetInt32Secure();
  }

  private static int GetInt32Secure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      return BitConverter.ToInt32(RandomGenerator.FourBytes, 0);
    }
  }

  private static int GetInt32Unsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    return BitConverter.ToInt32(RandomGenerator.FourBytes, 0);
  }

  public static uint GetUInt32(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetUInt32Unsecure() : RandomGenerator.GetUInt32Secure();
  }

  private static uint GetUInt32Secure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      return BitConverter.ToUInt32(RandomGenerator.FourBytes, 0);
    }
  }

  private static uint GetUInt32Unsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    return BitConverter.ToUInt32(RandomGenerator.FourBytes, 0);
  }

  public static long GetInt64(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetInt64Unsecure() : RandomGenerator.GetInt64Secure();
  }

  private static long GetInt64Secure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      return BitConverter.ToInt64(RandomGenerator.EightBytes, 0);
    }
  }

  private static long GetInt64Unsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    return BitConverter.ToInt64(RandomGenerator.EightBytes, 0);
  }

  public static ulong GetUInt64(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetUInt64Unsecure() : RandomGenerator.GetUInt64Secure();
  }

  private static ulong GetUInt64Secure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      return BitConverter.ToUInt64(RandomGenerator.EightBytes, 0);
    }
  }

  private static ulong GetUInt64Unsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    return BitConverter.ToUInt64(RandomGenerator.EightBytes, 0);
  }

  public static float GetFloat(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetFloatUnsecure() : RandomGenerator.GetFloatSecure();
  }

  private static float GetFloatSecure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      return BitConverter.ToSingle(RandomGenerator.FourBytes, 0);
    }
  }

  private static float GetFloatUnsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    return BitConverter.ToSingle(RandomGenerator.FourBytes, 0);
  }

  public static double GetDouble(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetDoubleUnsecure() : RandomGenerator.GetDoubleSecure();
  }

  private static double GetDoubleSecure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      return BitConverter.ToDouble(RandomGenerator.EightBytes, 0);
    }
  }

  private static double GetDoubleUnsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    return BitConverter.ToDouble(RandomGenerator.EightBytes, 0);
  }

  public static Decimal GetDecimal(bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetDecimalUnsecure() : RandomGenerator.GetDecimalSecure();
  }

  private static Decimal GetDecimalSecure()
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.SixteenBytes);
      return new Decimal(new int[4]
      {
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 0),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 4),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 8),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 12)
      });
    }
  }

  private static Decimal GetDecimalUnsecure()
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.SixteenBytes);
    return new Decimal(new int[4]
    {
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 0),
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 4),
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 8),
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 12)
    });
  }

  public static byte GetByte(byte min, byte max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetByteUnsecure(min, max) : RandomGenerator.GetByteSecure(min, max);
  }

  private static byte GetByteSecure(byte min, byte max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      byte num;
      for (num = RandomGenerator.OneByte[0]; (int) num < (int) min || (int) num >= (int) max; num = RandomGenerator.OneByte[0])
        cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      return num;
    }
  }

  private static byte GetByteUnsecure(byte min, byte max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    byte num;
    for (num = RandomGenerator.OneByte[0]; (int) num < (int) min || (int) num >= (int) max; num = RandomGenerator.OneByte[0])
      RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    return num;
  }

  public static sbyte GetSByte(sbyte min, sbyte max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetSByteUnsecure(min, max) : RandomGenerator.GetSByteSecure(min, max);
  }

  private static sbyte GetSByteSecure(sbyte min, sbyte max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      sbyte num;
      for (num = (sbyte) RandomGenerator.OneByte[0]; (int) num < (int) min || (int) num >= (int) max; num = (sbyte) RandomGenerator.OneByte[0])
        cryptoServiceProvider.GetBytes(RandomGenerator.OneByte);
      return num;
    }
  }

  private static sbyte GetSByteUnsecure(sbyte min, sbyte max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    sbyte num;
    for (num = (sbyte) RandomGenerator.OneByte[0]; (int) num < (int) min || (int) num >= (int) max; num = (sbyte) RandomGenerator.OneByte[0])
      RandomGenerator.Random.NextBytes(RandomGenerator.OneByte);
    return num;
  }

  public static short GetInt16(short min, short max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetInt16Unsecure(min, max) : RandomGenerator.GetInt16Secure(min, max);
  }

  private static short GetInt16Secure(short min, short max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.TwoBytes);
      short int16;
      for (int16 = BitConverter.ToInt16(RandomGenerator.TwoBytes, 0); (int) int16 < (int) min || (int) int16 >= (int) max; int16 = BitConverter.ToInt16(RandomGenerator.TwoBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.TwoBytes);
      return int16;
    }
  }

  private static short GetInt16Unsecure(short min, short max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.TwoBytes);
    short int16;
    for (int16 = BitConverter.ToInt16(RandomGenerator.TwoBytes, 0); (int) int16 < (int) min || (int) int16 >= (int) max; int16 = BitConverter.ToInt16(RandomGenerator.TwoBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.TwoBytes);
    return int16;
  }

  public static ushort GetUInt16(ushort min, ushort max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetUInt16Unsecure(min, max) : RandomGenerator.GetUInt16Secure(min, max);
  }

  private static ushort GetUInt16Secure(ushort min, ushort max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.TwoBytes);
      ushort uint16;
      for (uint16 = BitConverter.ToUInt16(RandomGenerator.TwoBytes, 0); (int) uint16 < (int) min || (int) uint16 >= (int) max; uint16 = BitConverter.ToUInt16(RandomGenerator.TwoBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.TwoBytes);
      return uint16;
    }
  }

  private static ushort GetUInt16Unsecure(ushort min, ushort max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.TwoBytes);
    ushort uint16;
    for (uint16 = BitConverter.ToUInt16(RandomGenerator.TwoBytes, 0); (int) uint16 < (int) min || (int) uint16 >= (int) max; uint16 = BitConverter.ToUInt16(RandomGenerator.TwoBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.TwoBytes);
    return uint16;
  }

  public static int GetInt32(int min, int max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetInt32Unsecure(min, max) : RandomGenerator.GetInt32Secure(min, max);
  }

  private static int GetInt32Secure(int min, int max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      int int32;
      for (int32 = BitConverter.ToInt32(RandomGenerator.FourBytes, 0); int32 < min || int32 >= max; int32 = BitConverter.ToInt32(RandomGenerator.FourBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      return int32;
    }
  }

  private static int GetInt32Unsecure(int min, int max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    int int32;
    for (int32 = BitConverter.ToInt32(RandomGenerator.FourBytes, 0); int32 < min || int32 >= max; int32 = BitConverter.ToInt32(RandomGenerator.FourBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    return int32;
  }

  public static uint GetUInt32(uint min, uint max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetUInt32Unsecure(min, max) : RandomGenerator.GetUInt32Secure(min, max);
  }

  private static uint GetUInt32Secure(uint min, uint max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      uint uint32;
      for (uint32 = BitConverter.ToUInt32(RandomGenerator.FourBytes, 0); uint32 < min || uint32 >= max; uint32 = BitConverter.ToUInt32(RandomGenerator.FourBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      return uint32;
    }
  }

  private static uint GetUInt32Unsecure(uint min, uint max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    uint uint32;
    for (uint32 = BitConverter.ToUInt32(RandomGenerator.FourBytes, 0); uint32 < min || uint32 >= max; uint32 = BitConverter.ToUInt32(RandomGenerator.FourBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    return uint32;
  }

  public static long GetInt64(long min, long max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetInt64Unsecure(min, max) : RandomGenerator.GetInt64Secure(min, max);
  }

  private static long GetInt64Secure(long min, long max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      long int64;
      for (int64 = BitConverter.ToInt64(RandomGenerator.EightBytes, 0); int64 < min || int64 >= max; int64 = BitConverter.ToInt64(RandomGenerator.EightBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      return int64;
    }
  }

  private static long GetInt64Unsecure(long min, long max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    long int64;
    for (int64 = BitConverter.ToInt64(RandomGenerator.EightBytes, 0); int64 < min || int64 >= max; int64 = BitConverter.ToInt64(RandomGenerator.EightBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    return int64;
  }

  public static ulong GetUInt64(ulong min, ulong max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetUInt64Unsecure(min, max) : RandomGenerator.GetUInt64Secure(min, max);
  }

  private static ulong GetUInt64Secure(ulong min, ulong max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      ulong uint64;
      for (uint64 = BitConverter.ToUInt64(RandomGenerator.EightBytes, 0); uint64 < min || uint64 >= max; uint64 = BitConverter.ToUInt64(RandomGenerator.EightBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      return uint64;
    }
  }

  private static ulong GetUInt64Unsecure(ulong min, ulong max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    ulong uint64;
    for (uint64 = BitConverter.ToUInt64(RandomGenerator.EightBytes, 0); uint64 < min || uint64 >= max; uint64 = BitConverter.ToUInt64(RandomGenerator.EightBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    return uint64;
  }

  public static float GetFloat(float min, float max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetFloatUnsecure(min, max) : RandomGenerator.GetFloatSecure(min, max);
  }

  private static float GetFloatSecure(float min, float max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      float single;
      for (single = BitConverter.ToSingle(RandomGenerator.FourBytes, 0); (double) single < (double) min || (double) single > (double) max; single = BitConverter.ToSingle(RandomGenerator.FourBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.FourBytes);
      return single;
    }
  }

  private static float GetFloatUnsecure(float min, float max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    float single;
    for (single = BitConverter.ToSingle(RandomGenerator.FourBytes, 0); (double) single < (double) min || (double) single > (double) max; single = BitConverter.ToSingle(RandomGenerator.FourBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.FourBytes);
    return single;
  }

  public static double GetDouble(double min, double max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetDoubleUnsecure(min, max) : RandomGenerator.GetDoubleSecure(min, max);
  }

  private static double GetDoubleSecure(double min, double max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      double num;
      for (num = BitConverter.ToDouble(RandomGenerator.EightBytes, 0); num < min || num > max; num = BitConverter.ToDouble(RandomGenerator.EightBytes, 0))
        cryptoServiceProvider.GetBytes(RandomGenerator.EightBytes);
      return num;
    }
  }

  private static double GetDoubleUnsecure(double min, double max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    double num;
    for (num = BitConverter.ToDouble(RandomGenerator.EightBytes, 0); num < min || num > max; num = BitConverter.ToDouble(RandomGenerator.EightBytes, 0))
      RandomGenerator.Random.NextBytes(RandomGenerator.EightBytes);
    return num;
  }

  public static Decimal GetDecimal(Decimal min, Decimal max, bool secure = false)
  {
    return !secure && !RandomGenerator.CryptoRng ? RandomGenerator.GetDecimalUnsecure(min, max) : RandomGenerator.GetDecimalSecure(min, max);
  }

  private static Decimal GetDecimalSecure(Decimal min, Decimal max)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      cryptoServiceProvider.GetBytes(RandomGenerator.SixteenBytes);
      Decimal num = new Decimal(new int[4]
      {
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 0),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 4),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 8),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 12)
      });
      while (num < min || num > max)
      {
        cryptoServiceProvider.GetBytes(RandomGenerator.SixteenBytes);
        num = new Decimal(new int[4]
        {
          BitConverter.ToInt32(RandomGenerator.SixteenBytes, 0),
          BitConverter.ToInt32(RandomGenerator.SixteenBytes, 4),
          BitConverter.ToInt32(RandomGenerator.SixteenBytes, 8),
          BitConverter.ToInt32(RandomGenerator.SixteenBytes, 12)
        });
      }
      return num;
    }
  }

  private static Decimal GetDecimalUnsecure(Decimal min, Decimal max)
  {
    RandomGenerator.Random.NextBytes(RandomGenerator.SixteenBytes);
    Decimal num = new Decimal(new int[4]
    {
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 0),
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 4),
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 8),
      BitConverter.ToInt32(RandomGenerator.SixteenBytes, 12)
    });
    while (num < min || num > max)
    {
      RandomGenerator.Random.NextBytes(RandomGenerator.SixteenBytes);
      num = new Decimal(new int[4]
      {
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 0),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 4),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 8),
        BitConverter.ToInt32(RandomGenerator.SixteenBytes, 12)
      });
    }
    return num;
  }
}
