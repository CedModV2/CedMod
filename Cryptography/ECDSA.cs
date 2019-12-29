// Decompiled with JetBrains decompiler
// Type: Cryptography.ECDSA
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Cryptography
{
  public static class ECDSA
  {
    public static AsymmetricCipherKeyPair GenerateKeys(int size = 384)
    {
      ECKeyPairGenerator keyPairGenerator = new ECKeyPairGenerator(nameof (ECDSA));
      keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), size));
      return keyPairGenerator.GenerateKeyPair();
    }

    public static string Sign(string data, AsymmetricKeyParameter privKey)
    {
      return Convert.ToBase64String(ECDSA.SignBytes(data, privKey));
    }

    public static byte[] SignBytes(string data, AsymmetricKeyParameter privKey)
    {
      try
      {
        return ECDSA.SignBytes(Encoding.UTF8.GetBytes(data), privKey);
      }
      catch
      {
        return (byte[]) null;
      }
    }

    public static byte[] SignBytes(byte[] data, AsymmetricKeyParameter privKey)
    {
      try
      {
        ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
        signer.Init(true, (ICipherParameters) privKey);
        signer.BlockUpdate(data, 0, data.Length);
        return signer.GenerateSignature();
      }
      catch
      {
        return (byte[]) null;
      }
    }

    public static bool Verify(string data, string signature, AsymmetricKeyParameter pubKey)
    {
      return ECDSA.VerifyBytes(data, Convert.FromBase64String(signature), pubKey);
    }

    public static bool VerifyBytes(string data, byte[] signature, AsymmetricKeyParameter pubKey)
    {
      try
      {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
        signer.Init(false, (ICipherParameters) pubKey);
        signer.BlockUpdate(bytes, 0, data.Length);
        return signer.VerifySignature(signature);
      }
      catch (Exception ex)
      {
        GameCore.Console.AddLog("ECDSA Verification Error (BouncyCastle): " + ex.Message + ", " + ex.StackTrace, Color.red, false);
        return false;
      }
    }

    public static AsymmetricKeyParameter PublicKeyFromString(string key)
    {
      using (TextReader reader = (TextReader) new StringReader(key))
        return (AsymmetricKeyParameter) new PemReader(reader).ReadObject();
    }

    public static string KeyToString(AsymmetricKeyParameter key)
    {
      using (TextWriter writer = (TextWriter) new StringWriter())
      {
        PemWriter pemWriter = new PemWriter(writer);
        pemWriter.WriteObject((object) key);
        pemWriter.Writer.Flush();
        return writer.ToString();
      }
    }
  }
}
