// Decompiled with JetBrains decompiler
// Type: Cryptography.AES
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.IO;

namespace Cryptography
{
  public static class AES
  {
    public const int NonceSizeBytes = 32;
    public const int MacSizeBits = 128;

    public static byte[] AesGcmEncrypt(byte[] data, byte[] secret, SecureRandom secureRandom)
    {
      byte[] numArray1 = new byte[32];
      secureRandom.NextBytes(numArray1, 0, numArray1.Length);
      GcmBlockCipher gcmBlockCipher = new GcmBlockCipher((IBlockCipher) new AesEngine());
      gcmBlockCipher.Init(true, (ICipherParameters) new AeadParameters(new KeyParameter(secret), 128, numArray1));
      byte[] numArray2 = new byte[gcmBlockCipher.GetOutputSize(data.Length)];
      int outOff = gcmBlockCipher.ProcessBytes(data, 0, data.Length, numArray2, 0);
      gcmBlockCipher.DoFinal(numArray2, outOff);
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream))
        {
          binaryWriter.Write(numArray1);
          binaryWriter.Write(numArray2);
        }
        return memoryStream.ToArray();
      }
    }

    public static byte[] AesGcmDecrypt(byte[] data, byte[] secret)
    {
      using (MemoryStream memoryStream = new MemoryStream(data))
      {
        using (BinaryReader binaryReader = new BinaryReader((Stream) memoryStream))
        {
          byte[] nonce = binaryReader.ReadBytes(32);
          GcmBlockCipher gcmBlockCipher = new GcmBlockCipher((IBlockCipher) new AesEngine());
          gcmBlockCipher.Init(false, (ICipherParameters) new AeadParameters(new KeyParameter(secret), 128, nonce));
          byte[] input = binaryReader.ReadBytes(data.Length - nonce.Length);
          byte[] output = new byte[gcmBlockCipher.GetOutputSize(input.Length)];
          int outOff = gcmBlockCipher.ProcessBytes(input, 0, input.Length, output, 0);
          gcmBlockCipher.DoFinal(output, outOff);
          return output;
        }
      }
    }
  }
}
