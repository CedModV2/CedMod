// Decompiled with JetBrains decompiler
// Type: GameConsoleTransmission
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using Mirror;
using Org.BouncyCastle.Security;
using RemoteAdmin;
using Security;
using System;
using UnityEngine;

public class GameConsoleTransmission : NetworkBehaviour
{
  public RemoteAdminCryptographicManager CryptoManager;
  public QueryProcessor Processor;
  public GameCore.Console Console;
  private static SecureRandom _secureRandom;
  private RateLimit _cmdRateLimit;

  private void Start()
  {
    this._cmdRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[2];
    this.CryptoManager = this.GetComponent<RemoteAdminCryptographicManager>();
    this.Processor = this.GetComponent<QueryProcessor>();
    if (GameConsoleTransmission._secureRandom == null)
      GameConsoleTransmission._secureRandom = new SecureRandom();
    if (!this.isLocalPlayer)
      return;
    this.Console = GameCore.Console.singleton;
  }

  [Server]
  public void SendToClient(NetworkConnection connection, string text, string color)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void GameConsoleTransmission::SendToClient(Mirror.NetworkConnection,System.String,System.String)' called on client");
    }
    else
    {
      byte[] bytes = Utf8.GetBytes(color + "#" + text);
      if (this.CryptoManager.EncryptionKey == null)
        this.TargetPrintOnConsole(connection, bytes, false);
      else
        this.TargetPrintOnConsole(connection, AES.AesGcmEncrypt(bytes, this.CryptoManager.EncryptionKey, GameConsoleTransmission._secureRandom), true);
    }
  }

  [TargetRpc]
  public void TargetPrintOnConsole(NetworkConnection connection, byte[] data, bool encrypted)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBytesAndSize(data);
    writer.WriteBoolean(encrypted);
    this.SendTargetRPCInternal(connection, typeof (GameConsoleTransmission), nameof (TargetPrintOnConsole), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Client]
  public void SendToServer(string command)
  {
    if (!NetworkClient.active)
    {
      Debug.LogWarning((object) "[Client] function 'System.Void GameConsoleTransmission::SendToServer(System.String)' called on server");
    }
    else
    {
      byte[] bytes = Utf8.GetBytes(command);
      if (this.CryptoManager.EncryptionKey == null)
        this.CmdCommandToServer(bytes, false);
      else
        this.CmdCommandToServer(AES.AesGcmEncrypt(bytes, this.CryptoManager.EncryptionKey, GameConsoleTransmission._secureRandom), true);
    }
  }

  [Command]
  public void CmdCommandToServer(byte[] data, bool encrypted)
  {
    if (this.isServer)
    {
      this.CallCmdCommandToServer(data, encrypted);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBytesAndSize(data);
      writer.WriteBoolean(encrypted);
      this.SendCommandInternal(typeof (GameConsoleTransmission), nameof (CmdCommandToServer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private Color ProcessColor(string name)
  {
    switch (name)
    {
      case "black":
        return Color.black;
      case "blue":
        return Color.blue;
      case "cyan":
        return Color.cyan;
      case "green":
        return Color.green;
      case "magenta":
        return Color.magenta;
      case "red":
        return Color.red;
      case "white":
        return Color.white;
      case "yellow":
        return Color.yellow;
      default:
        return Color.grey;
    }
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdCommandToServer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdCommandToServer called on client.");
    else
      ((GameConsoleTransmission) obj).CallCmdCommandToServer(reader.ReadBytesAndSize(), reader.ReadBoolean());
  }

  public void CallCmdCommandToServer(byte[] data, bool encrypted)
  {
    if (!this._cmdRateLimit.CanExecute(true) || data == null)
      return;
    string query;
    if (!encrypted)
    {
      if (this.CryptoManager.EncryptionKey != null || this.CryptoManager.ExchangeRequested)
      {
        this.SendToClient(this.connectionToClient, "Please use encrypted connection to send commands.", "magenta");
        return;
      }
      query = Utf8.GetString(data);
    }
    else
    {
      if (this.CryptoManager.EncryptionKey == null)
      {
        this.SendToClient(this.connectionToClient, "Can't process encrypted message from server before completing ECDHE exchange.", "magenta");
        return;
      }
      try
      {
        query = Utf8.GetString(AES.AesGcmDecrypt(data, this.CryptoManager.EncryptionKey));
      }
      catch
      {
        this.SendToClient(this.connectionToClient, "Decryption or verification of encrypted message failed.", "magenta");
        return;
      }
    }
    this.Processor.ProcessGameConsoleQuery(query, encrypted);
  }

  protected static void InvokeRpcTargetPrintOnConsole(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetPrintOnConsole called on server.");
    else
      ((GameConsoleTransmission) obj).CallTargetPrintOnConsole(ClientScene.readyConnection, reader.ReadBytesAndSize(), reader.ReadBoolean());
  }

  public void CallTargetPrintOnConsole(NetworkConnection connection, byte[] data, bool encrypted)
  {
    if (data == null)
      return;
    string str1;
    if (!encrypted)
    {
      str1 = Utf8.GetString(data);
    }
    else
    {
      if (this.CryptoManager.EncryptionKey == null)
      {
        GameCore.Console.AddLog("Can't process encrypted message from server before completing ECDHE exchange.", Color.magenta, false);
        return;
      }
      try
      {
        str1 = Utf8.GetString(AES.AesGcmDecrypt(data, this.CryptoManager.EncryptionKey));
      }
      catch
      {
        this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Decryption or verification of encrypted message failed.", "magenta");
        return;
      }
    }
    string name = str1.Remove(str1.IndexOf("#", StringComparison.Ordinal));
    string str2 = str1.Remove(0, str1.IndexOf("#", StringComparison.Ordinal) + 1);
    GameCore.Console.AddLog((encrypted ? "[FROM SERVER] " : "[UNENCRYPTED FROM SERVER] ") + str2, this.ProcessColor(name), false);
  }

  static GameConsoleTransmission()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (GameConsoleTransmission), "CmdCommandToServer", new NetworkBehaviour.CmdDelegate(GameConsoleTransmission.InvokeCmdCmdCommandToServer));
    NetworkBehaviour.RegisterRpcDelegate(typeof (GameConsoleTransmission), "TargetPrintOnConsole", new NetworkBehaviour.CmdDelegate(GameConsoleTransmission.InvokeRpcTargetPrintOnConsole));
  }
}
