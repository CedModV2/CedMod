// Decompiled with JetBrains decompiler
// Type: Cryptography.Sha
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Security.Cryptography;
using System.Text;

namespace Cryptography
{
  public static class Sha
  {
    public static byte[] Sha256(byte[] message)
    {
      using (SHA256 shA256 = SHA256.Create())
        return shA256.ComputeHash(message);
    }

    public static byte[] Sha256(string message)
    {
      return Sha.Sha256(Utf8.GetBytes(message));
    }

    public static byte[] Sha256Hmac(byte[] key, byte[] message)
    {
      using (HMACSHA256 hmacshA256 = new HMACSHA256(key))
        return hmacshA256.ComputeHash(message);
    }

    public static byte[] Sha512(string message)
    {
      return Sha.Sha512(Utf8.GetBytes(message));
    }

    public static byte[] Sha512(byte[] message)
    {
      using (SHA512 shA512 = SHA512.Create())
        return shA512.ComputeHash(message);
    }

    public static byte[] Sha512Hmac(byte[] key, byte[] message)
    {
      using (HMACSHA512 hmacshA512 = new HMACSHA512(key))
        return hmacshA512.ComputeHash(message);
    }

    public static string HashToString(byte[] hash)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (byte num in hash)
        stringBuilder.Append(num.ToString("X2"));
      return stringBuilder.ToString();
    }
  }
}
