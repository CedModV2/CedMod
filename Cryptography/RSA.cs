// Decompiled with JetBrains decompiler
// Type: Cryptography.RSA
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;

namespace Cryptography
{
  public static class RSA
  {
    public static bool Verify(string data, string signature, string key)
    {
      using (TextReader reader = (TextReader) new StringReader(key))
      {
        AsymmetricKeyParameter asymmetricKeyParameter = (AsymmetricKeyParameter) new PemReader(reader).ReadObject();
        ISigner signer = SignerUtilities.GetSigner("SHA256withRSA");
        signer.Init(false, (ICipherParameters) asymmetricKeyParameter);
        byte[] signature1 = Convert.FromBase64String(signature);
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        signer.BlockUpdate(bytes, 0, bytes.Length);
        return signer.VerifySignature(signature1);
      }
    }
  }
}
