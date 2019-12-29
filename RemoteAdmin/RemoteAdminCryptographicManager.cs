// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.RemoteAdminCryptographicManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using GameCore;
using Mirror;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using UnityEngine;

namespace RemoteAdmin
{
  public class RemoteAdminCryptographicManager : NetworkBehaviour
  {
    internal AsymmetricCipherKeyPair EcdhKeys;
    internal ECDHBasicAgreement Exchange;
    internal byte[] EcdhPublicKeySignature;
    internal bool ExchangeRequested;
    internal byte[] EncryptionKey;

    public void Init()
    {
      this.EcdhKeys = ECDH.GenerateKeys(384);
      this.Exchange = ECDH.Init(this.EcdhKeys);
      this.ExchangeRequested = true;
    }

    [Server]
    public void StartExchange()
    {
      if (!NetworkServer.active)
      {
        Debug.LogWarning((object) "[Server] function 'System.Void RemoteAdmin.RemoteAdminCryptographicManager::StartExchange()' called on client");
      }
      else
      {
        if (this.Exchange == null || this.EcdhKeys == null)
          this.Init();
        this.TargetDiffieHellmanExchange(this.connectionToClient, ECDSA.KeyToString(this.EcdhKeys.Public));
      }
    }

    [TargetRpc]
    public void TargetDiffieHellmanExchange(NetworkConnection conn, string publicKey)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(publicKey);
      this.SendTargetRPCInternal(conn, typeof (RemoteAdminCryptographicManager), nameof (TargetDiffieHellmanExchange), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    [Command]
    public void CmdDiffieHellmanExchange(string publicKey, byte[] signature)
    {
      if (this.isServer)
      {
        this.CallCmdDiffieHellmanExchange(publicKey, signature);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(publicKey);
        writer.WriteBytesAndSize(signature);
        this.SendCommandInternal(typeof (RemoteAdminCryptographicManager), nameof (CmdDiffieHellmanExchange), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    private void MirrorProcessed()
    {
    }

    protected static void InvokeCmdCmdDiffieHellmanExchange(
      NetworkBehaviour obj,
      NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdDiffieHellmanExchange called on client.");
      else
        ((RemoteAdminCryptographicManager) obj).CallCmdDiffieHellmanExchange(reader.ReadString(), reader.ReadBytesAndSize());
    }

    public void CallCmdDiffieHellmanExchange(string publicKey, byte[] signature)
    {
      if (this.EncryptionKey != null || this.Exchange == null || this.EcdhKeys == null)
        return;
      AsymmetricKeyParameter publicKey1 = this.GetComponent<ServerRoles>().PublicKey;
      string authToken = this.GetComponent<CharacterClassManager>().AuthToken;
      if (CharacterClassManager.OnlineMode && (publicKey == null || authToken == null))
        this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Please complete authentication before requesting ECDHE exchange.", "magenta");
      else if (CharacterClassManager.OnlineMode && publicKey1 != null && !ECDSA.VerifyBytes(publicKey, signature, publicKey1))
        this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Exchange parameters signature is invalid!", "magenta");
      else
        this.EncryptionKey = ECDH.DeriveKey(this.Exchange, ECDSA.PublicKeyFromString(publicKey));
    }

    protected static void InvokeRpcTargetDiffieHellmanExchange(
      NetworkBehaviour obj,
      NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetDiffieHellmanExchange called on server.");
      else
        ((RemoteAdminCryptographicManager) obj).CallTargetDiffieHellmanExchange(ClientScene.readyConnection, reader.ReadString());
    }

    public void CallTargetDiffieHellmanExchange(NetworkConnection conn, string publicKey)
    {
      if (this.EncryptionKey != null)
      {
        Console.AddLog("Rejected duplicated Elliptic-curve Diffie–Hellman (ECDH) parameters from server.", Color.magenta, false);
      }
      else
      {
        if (publicKey == null)
          return;
        if (this.Exchange == null || this.EcdhKeys == null)
          this.Init();
        if (this.EcdhPublicKeySignature == null)
          this.EcdhPublicKeySignature = ECDSA.SignBytes(ECDSA.KeyToString(this.EcdhKeys.Public), Console.SessionKeys.Private);
        this.EncryptionKey = ECDH.DeriveKey(this.Exchange, ECDSA.PublicKeyFromString(publicKey));
        this.CmdDiffieHellmanExchange(ECDSA.KeyToString(this.EcdhKeys.Public), this.EcdhPublicKeySignature);
        Console.AddLog("Completed ECDHE exchange with server.", Color.grey, false);
      }
    }

    static RemoteAdminCryptographicManager()
    {
      NetworkBehaviour.RegisterCommandDelegate(typeof (RemoteAdminCryptographicManager), "CmdDiffieHellmanExchange", new NetworkBehaviour.CmdDelegate(RemoteAdminCryptographicManager.InvokeCmdCmdDiffieHellmanExchange));
      NetworkBehaviour.RegisterRpcDelegate(typeof (RemoteAdminCryptographicManager), "TargetDiffieHellmanExchange", new NetworkBehaviour.CmdDelegate(RemoteAdminCryptographicManager.InvokeRpcTargetDiffieHellmanExchange));
    }
  }
}
