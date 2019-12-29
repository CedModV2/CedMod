// Decompiled with JetBrains decompiler
// Type: Cryptography.ECDH
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace Cryptography
{
  public static class ECDH
  {
    public static AsymmetricCipherKeyPair GenerateKeys(int size = 384)
    {
      ECKeyPairGenerator keyPairGenerator = new ECKeyPairGenerator(nameof (ECDH));
      keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), size));
      return keyPairGenerator.GenerateKeyPair();
    }

    public static ECDHBasicAgreement Init(AsymmetricCipherKeyPair localKey)
    {
      ECDHBasicAgreement ecdhBasicAgreement = new ECDHBasicAgreement();
      ecdhBasicAgreement.Init((ICipherParameters) localKey.Private);
      return ecdhBasicAgreement;
    }

    public static byte[] DeriveKey(ECDHBasicAgreement exchange, AsymmetricKeyParameter remoteKey)
    {
      using (SHA256 shA256 = SHA256.Create())
        return shA256.ComputeHash(exchange.CalculateAgreement((ICipherParameters) remoteKey).ToByteArray());
    }
  }
}
