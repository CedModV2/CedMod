// Decompiled with JetBrains decompiler
// Type: Cryptography.PBKDF2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Security.Cryptography;

namespace Cryptography
{
  public static class PBKDF2
  {
    public static string Pbkdf2HashString(
      string password,
      byte[] salt,
      int iterations,
      int outputBytes)
    {
      return Convert.ToBase64String(PBKDF2.Pbkdf2HashBytes(password, salt, iterations, outputBytes));
    }

    public static byte[] Pbkdf2HashBytes(
      string password,
      byte[] salt,
      int iterations,
      int outputBytes)
    {
      using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt)
      {
        IterationCount = iterations
      })
        return rfc2898DeriveBytes.GetBytes(outputBytes);
    }
  }
}
