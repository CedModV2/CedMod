// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.QueryProcessor
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using Mirror;
using Org.BouncyCastle.Security;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

namespace RemoteAdmin
{
  public class QueryProcessor : NetworkBehaviour
  {
    public RemoteAdminCryptographicManager CryptoManager;
    public GameConsoleTransmission GCT;
    public static QueryProcessor Localplayer;
    public static CharacterClassManager LocalCCM;
    internal ServerRoles Roles;
    private PlayerCommandSender _sender;
    private RateLimit _commandRateLimit;
    private static SecureRandom _secureRandom;
    public static bool Lockdown;
    private int _toBanType;
    private static int _idIterator;
    private const int HashIterations = 250;
    internal int PasswordTries;
    internal int SignaturesCounter;
    private int _signaturesCounter;
    internal byte[] Key;
    internal byte[] Salt;
    internal byte[] ClientSalt;
    private byte[] _key;
    private byte[] _salt;
    private byte[] _clientSalt;
    private float _lastPlayerlistRequest;
    private string _toBan;
    private string _toBanNick;
    private string _toBanUserId;
    private string _toBanUserId2;
    private string _toBanAuth;
    private string _prevSalt;
    [SyncVar(hook = "SetServerRandom")]
    public string ServerRandom;
    private static string _serverStaticRandom;
    [SyncVar]
    [NonSerialized]
    public int PlayerId;
    [SyncVar]
    [NonSerialized]
    public bool OverridePasswordEnabled;
    internal bool PasswordSent;
    private bool _gameplayData;
    private bool _gdDirty;
    private string _ipAddress;
    private NetworkConnection _conns;

    public bool GameplayData
    {
      get
      {
        return this._gameplayData;
      }
      set
      {
        this._gameplayData = value;
        this._gdDirty = true;
      }
    }

    private void Start()
    {
      this._commandRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[2];
      this.Roles = this.GetComponent<ServerRoles>();
      this.CryptoManager = this.GetComponent<RemoteAdminCryptographicManager>();
      this.GCT = this.GetComponent<GameConsoleTransmission>();
      if (QueryProcessor._secureRandom == null)
        QueryProcessor._secureRandom = new SecureRandom();
      this.SignaturesCounter = 0;
      this._signaturesCounter = 0;
      if (NetworkServer.active)
      {
        this._conns = this.connectionToClient;
        this._ipAddress = this._conns.address;
        this.NetworkOverridePasswordEnabled = ServerStatic.PermissionsHandler.OverrideEnabled;
        if (string.IsNullOrEmpty(QueryProcessor._serverStaticRandom))
        {
          byte[] numArray;
          using (RandomNumberGenerator randomNumberGenerator = (RandomNumberGenerator) new RNGCryptoServiceProvider())
          {
            numArray = new byte[32];
            randomNumberGenerator.GetBytes(numArray);
          }
          QueryProcessor._serverStaticRandom = Convert.ToBase64String(numArray);
          ServerConsole.AddLog("Generated round random salt: " + QueryProcessor._serverStaticRandom);
        }
        if (string.IsNullOrEmpty(this.ServerRandom))
          this.NetworkServerRandom = QueryProcessor._serverStaticRandom;
        ++QueryProcessor._idIterator;
        this.NetworkPlayerId = QueryProcessor._idIterator;
      }
      this._sender = new PlayerCommandSender(this);
      if (!this.isLocalPlayer)
        return;
      QueryProcessor.Localplayer = this;
      QueryProcessor.LocalCCM = this.GetComponent<CharacterClassManager>();
    }

    public void SetServerRandom(string random)
    {
      this.NetworkServerRandom = random;
      if (this.isServer || this._prevSalt == random)
        return;
      this._prevSalt = random;
      GameCore.Console.AddDebugLog("SDAUTH", "Obtained server round random: " + random, MessageImportance.Normal, false);
    }

    private void Update()
    {
      if (this.isLocalPlayer && (double) this._lastPlayerlistRequest < 1.0)
        this._lastPlayerlistRequest += Time.deltaTime;
      if (!this._gdDirty)
        return;
      this._gdDirty = false;
      if (!NetworkServer.active)
        return;
      this.TargetSyncGameplayData(this.connectionToClient, this._gameplayData);
    }

    [Command]
    public void CmdRequestSalt(byte[] clSalt)
    {
      if (this.isServer)
      {
        this.CallCmdRequestSalt(clSalt);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBytesAndSize(clSalt);
        this.SendCommandInternal(typeof (QueryProcessor), nameof (CmdRequestSalt), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    [TargetRpc]
    public void TargetSaltGenerated(NetworkConnection conn, byte[] salt)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBytesAndSize(salt);
      this.SendTargetRPCInternal(conn, typeof (QueryProcessor), nameof (TargetSaltGenerated), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    [Command]
    public void CmdSendPassword(byte[] authSignature)
    {
      if (this.isServer)
      {
        this.CallCmdSendPassword(authSignature);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBytesAndSize(authSignature);
        this.SendCommandInternal(typeof (QueryProcessor), nameof (CmdSendPassword), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    [TargetRpc]
    private void TargetReplyPassword(NetworkConnection conn, bool b)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendTargetRPCInternal(conn, typeof (QueryProcessor), nameof (TargetReplyPassword), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    [Server]
    internal void TargetReply(
      NetworkConnection conn,
      string content,
      bool isSuccess,
      bool logInConsole,
      string overrideDisplay)
    {
      if (!NetworkServer.active)
        Debug.LogWarning((object) "[Server] function 'System.Void RemoteAdmin.QueryProcessor::TargetReply(Mirror.NetworkConnection,System.String,System.Boolean,System.Boolean,System.String)' called on client");
      else if (this.CryptoManager.EncryptionKey == null)
      {
        if (ServerStatic.IsDedicated && this.isLocalPlayer)
          ServerConsole.AddLog("[RA output] " + content);
        else if (!this.CryptoManager.ExchangeRequested)
          this.TargetReplyPlain(conn, content, isSuccess, logInConsole, overrideDisplay);
        else
          this.TargetReplyPlain(conn, "ERROR#ECDHE exchange was requested, please complete it on client side.", false, true, "");
      }
      else
        this.TargetReplyEncrypted(conn, AES.AesGcmEncrypt(Utf8.GetBytes(content), this.CryptoManager.EncryptionKey, QueryProcessor._secureRandom), isSuccess, logInConsole, overrideDisplay);
    }

    [TargetRpc]
    public void TargetReplyPlain(
      NetworkConnection conn,
      string content,
      bool isSuccess,
      bool logInConsole,
      string overrideDisplay)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(content);
      writer.WriteBoolean(isSuccess);
      writer.WriteBoolean(logInConsole);
      writer.WriteString(overrideDisplay);
      this.SendTargetRPCInternal(conn, typeof (QueryProcessor), nameof (TargetReplyPlain), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    [TargetRpc]
    public void TargetReplyEncrypted(
      NetworkConnection conn,
      byte[] content,
      bool isSuccess,
      bool logInConsole,
      string overrideDisplay)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBytesAndSize(content);
      writer.WriteBoolean(isSuccess);
      writer.WriteBoolean(logInConsole);
      writer.WriteString(overrideDisplay);
      this.SendTargetRPCInternal(conn, typeof (QueryProcessor), nameof (TargetReplyEncrypted), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    [Command]
    public void CmdSendEncryptedQuery(byte[] query)
    {
      if (this.isServer)
      {
        this.CallCmdSendEncryptedQuery(query);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBytesAndSize(query);
        this.SendCommandInternal(typeof (QueryProcessor), nameof (CmdSendEncryptedQuery), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    [Command]
    public void CmdSendQuery(string query, int counter, byte[] signature)
    {
      if (this.isServer)
      {
        this.CallCmdSendQuery(query, counter, signature);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(query);
        writer.WritePackedInt32(counter);
        writer.WriteBytesAndSize(signature);
        this.SendCommandInternal(typeof (QueryProcessor), nameof (CmdSendQuery), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    internal void ProcessGameConsoleQuery(string query, bool encrypted)
    {
      this.GCT.SendToClient(this.connectionToClient, "Command not found.", "red");
    }

    private bool VerifyRequestSignature(
      string message,
      int counter,
      byte[] signature,
      bool validateCounter = true)
    {
      return !this.Roles.PublicKeyAccepted && this.Roles.RemoteAdminMode == ServerRoles.AccessMode.PasswordOverride ? this.VerifyHmacSignature(message, counter, signature, validateCounter) : this.VerifyEcdsaSignature(message, counter, signature, validateCounter);
    }

    private bool VerifyHmacSignature(
      string message,
      int counter,
      byte[] signature,
      bool validateCounter = true)
    {
      if (counter <= this._signaturesCounter)
      {
        if (validateCounter)
          return false;
      }
      else
        this._signaturesCounter = counter;
      if (!this.OverridePasswordEnabled)
        return false;
      return ((IEnumerable<byte>) Sha.Sha512Hmac(Utf8.GetBytes(message + ":[:COUNTER:]:" + (object) counter + ":[:SALT:]:" + this.ServerRandom), this._key)).SequenceEqual<byte>((IEnumerable<byte>) signature);
    }

    private bool VerifyEcdsaSignature(
      string message,
      int counter,
      byte[] signature,
      bool validateCounter = true)
    {
      if (!this.Roles.PublicKeyAccepted || this.Roles.PublicKey == null)
      {
        GameCore.Console.AddLog("VerifyEcdsaSignature called with empty Public Key", Color.red, false);
        Debug.LogError((object) "VerifyEcdsaSignature called with empty Public Key");
        return false;
      }
      if (counter <= this._signaturesCounter)
      {
        if (validateCounter)
          return false;
      }
      else
        this._signaturesCounter = counter;
      return ECDSA.VerifyBytes(message + ":[:COUNTER:]:" + (object) counter + ":[:SALT:]:" + this.ServerRandom, signature, this.Roles.PublicKey);
    }

    public static byte[] DerivePassword(string password, byte[] serversalt, byte[] clientsalt)
    {
      byte[] salt = Sha.Sha512(Convert.ToBase64String(serversalt) + Convert.ToBase64String(clientsalt));
      return PBKDF2.Pbkdf2HashBytes(password, salt, 250, 512);
    }

    [TargetRpc]
    public void TargetSyncGameplayData(NetworkConnection conn, bool gd)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(gd);
      this.SendTargetRPCInternal(conn, typeof (QueryProcessor), nameof (TargetSyncGameplayData), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    private void OnDestroy()
    {
      if (!NetworkServer.active)
        return;
      for (int index = 0; index < Scp096PlayerScript.VisiblePlyLists.Count; ++index)
      {
        Dictionary<int, ReferenceHub> visiblePlyList = Scp096PlayerScript.VisiblePlyLists[index];
        if (visiblePlyList.ContainsKey(this.PlayerId))
          visiblePlyList.Remove(this.PlayerId);
      }
      CustomNetworkManager.PlayerDisconnect(this._conns);
      if ((UnityEngine.Object) ServerLogs.singleton == (UnityEngine.Object) null)
        return;
      CharacterClassManager component = this.GetComponent<CharacterClassManager>();
      ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("{0} {1} disconnected from IP address {2}. Last class: {3} ({4})", (object) this.GetComponent<NicknameSync>().MyNick, string.IsNullOrEmpty(component.UserId) ? (object) "no ID" : (object) component.UserId, (object) this._ipAddress, (object) component.CurClass, component.CurClass < RoleType.Scp173 ? (object) "NOT SPAWNED" : (object) component.Classes.SafeGet(component.CurClass).fullName), ServerLogs.ServerLogType.ConnectionUpdate);
    }

    private void MirrorProcessed()
    {
    }

    public string NetworkServerRandom
    {
      get
      {
        return this.ServerRandom;
      }
      [param: In] set
      {
        if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
        {
          this.setSyncVarHookGuard(1UL, true);
          this.SetServerRandom(value);
          this.setSyncVarHookGuard(1UL, false);
        }
        this.SetSyncVar<string>(value, ref this.ServerRandom, 1UL);
      }
    }

    public int NetworkPlayerId
    {
      get
      {
        return this.PlayerId;
      }
      [param: In] set
      {
        this.SetSyncVar<int>(value, ref this.PlayerId, 2UL);
      }
    }

    public bool NetworkOverridePasswordEnabled
    {
      get
      {
        return this.OverridePasswordEnabled;
      }
      [param: In] set
      {
        this.SetSyncVar<bool>(value, ref this.OverridePasswordEnabled, 4UL);
      }
    }

    protected static void InvokeCmdCmdRequestSalt(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdRequestSalt called on client.");
      else
        ((QueryProcessor) obj).CallCmdRequestSalt(reader.ReadBytesAndSize());
    }

    protected static void InvokeCmdCmdSendPassword(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdSendPassword called on client.");
      else
        ((QueryProcessor) obj).CallCmdSendPassword(reader.ReadBytesAndSize());
    }

    protected static void InvokeCmdCmdSendEncryptedQuery(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdSendEncryptedQuery called on client.");
      else
        ((QueryProcessor) obj).CallCmdSendEncryptedQuery(reader.ReadBytesAndSize());
    }

    protected static void InvokeCmdCmdSendQuery(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdSendQuery called on client.");
      else
        ((QueryProcessor) obj).CallCmdSendQuery(reader.ReadString(), reader.ReadPackedInt32(), reader.ReadBytesAndSize());
    }

    public void CallCmdRequestSalt(byte[] clSalt)
    {
      if (!this._commandRateLimit.CanExecute(true))
        return;
      if (!ServerStatic.PermissionsHandler.OverrideEnabled)
      {
        this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Password authentication is disabled on this server!", "magenta");
      }
      else
      {
        if (this._clientSalt == null)
        {
          if (clSalt == null)
          {
            this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Please generate and send your salt!", "red");
            return;
          }
          if (clSalt.Length < 32)
          {
            this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Generated salt is too short. Please generate longer salt and try again!", "red");
            return;
          }
          this._clientSalt = clSalt;
          if (this._key == null && this._salt != null)
            this._key = ServerStatic.PermissionsHandler.DerivePassword(this._salt, this._clientSalt);
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Your salt " + Convert.ToBase64String(clSalt) + " has been accepted by the server.", "cyan");
        }
        if (this._salt != null)
        {
          this.TargetSaltGenerated(this.connectionToClient, this._salt);
        }
        else
        {
          byte[] data;
          using (RandomNumberGenerator randomNumberGenerator = (RandomNumberGenerator) new RNGCryptoServiceProvider())
          {
            data = new byte[32];
            randomNumberGenerator.GetBytes(data);
          }
          this._salt = data;
          this._key = ServerStatic.PermissionsHandler.DerivePassword(this._salt, this._clientSalt);
          this.TargetSaltGenerated(this.connectionToClient, this._salt);
        }
      }
    }

    public void CallCmdSendPassword(byte[] authSignature)
    {
      if (!this._commandRateLimit.CanExecute(true))
        return;
      bool b = false;
      if (this.Roles.RemoteAdmin)
      {
        b = true;
        this.PasswordTries = 0;
      }
      else
      {
        if (this._salt == null || this._clientSalt == null)
        {
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Can't verify your remote admin password - please generate salt first!", "red");
          return;
        }
        if (this._clientSalt.Length < 16)
        {
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Generated salt is too short. Please rejoin the server and try again!", "red");
          return;
        }
        if (this.VerifyHmacSignature("Login", -1, authSignature, false))
        {
          this.PasswordTries = 0;
          UserGroup overrideGroup = ServerStatic.PermissionsHandler.OverrideGroup;
          if (overrideGroup != null)
          {
            ServerConsole.AddLog("Assigned group " + overrideGroup.BadgeText + " to " + this.GetComponent<NicknameSync>().MyNick + " - override password.");
            this.Roles.SetGroup(overrideGroup, true, false, false);
            b = true;
          }
          else
            this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Non-existing group is assigned for override password!", "red");
        }
        else
        {
          ++this.PasswordTries;
          ServerConsole.AddLog("Rejected override password sent by " + this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ").");
          ServerLogs.AddLog(ServerLogs.Modules.Permissions, "Rejected override password sent by " + this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ").", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
        }
      }
      if (this.PasswordTries >= 3)
      {
        ServerLogs.AddLog(ServerLogs.Modules.Permissions, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ") has been kicked from the server for sending too many invalid override passwords.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
        ServerConsole.Disconnect(this.connectionToClient, "You have been kicked for too many Remote Admin login attempts.");
      }
      else
        this.TargetReplyPassword(this.connectionToClient, b);
    }

    public void CallCmdSendEncryptedQuery(byte[] query)
    {
      if (!this._commandRateLimit.CanExecute(true) || query == null)
        return;
      if (!this.Roles.RemoteAdmin)
        this.GCT.SendToClient(this.connectionToClient, "You are not logged in to remote admin panel!", "red");
      else if (this.CryptoManager.EncryptionKey == null)
      {
        this.GCT.SendToClient(this.connectionToClient, "Please complete ECDHE exchange before sending encrypted remote admin requests.", "magenta");
      }
      else
      {
        string str;
        try
        {
          str = Utf8.GetString(AES.AesGcmDecrypt(query, this.CryptoManager.EncryptionKey));
        }
        catch
        {
          this.GCT.SendToClient(this.connectionToClient, "Decryption or verification of remote admin request failed.", "magenta");
          return;
        }
        if (!str.Contains(":[:COUNTER:]:"))
        {
          this.GCT.SendToClient(this.connectionToClient, "Remote admin request doesn't contain a signatures counter.", "magenta");
        }
        else
        {
          int length = str.LastIndexOf(":[:COUNTER:]:", StringComparison.Ordinal);
          int result;
          if (!int.TryParse(str.Substring(length + 13), out result))
            this.GCT.SendToClient(this.connectionToClient, "Remote admin request contains non-integer signatures counter.", "magenta");
          else if (result <= this._signaturesCounter)
          {
            this.GCT.SendToClient(this.connectionToClient, "Remote admin request contains smaller signatures counter than previous request.", "magenta");
          }
          else
          {
            this._signaturesCounter = result;
            CommandProcessor.ProcessQuery(str.Substring(0, length), (CommandSender) this._sender);
          }
        }
      }
    }

    public void CallCmdSendQuery(string query, int counter, byte[] signature)
    {
      if (!this._commandRateLimit.CanExecute(true) || query == null || signature == null)
        return;
      if (string.IsNullOrEmpty(this.ServerRandom))
        this.GCT.SendToClient(this.connectionToClient, "Remote Admin error - ServerRandom is empty or null.", "magenta");
      else if (this.Roles.RemoteAdmin)
      {
        if (this.CryptoManager.ExchangeRequested)
          this.GCT.SendToClient(this.connectionToClient, "ECDHE exchange was requested, please use encrypted channel for remote admin commands.", "magenta");
        else if (this.VerifyRequestSignature(query, counter, signature, true))
          CommandProcessor.ProcessQuery(query, (CommandSender) this._sender);
        else
          this.GCT.SendToClient(this.connectionToClient, "Signature verification of request \"" + query + "\" failed!", "magenta");
      }
      else
        this.GCT.SendToClient(this.connectionToClient, "You are not logged in to remote admin panel!", "red");
    }

    protected static void InvokeRpcTargetSaltGenerated(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetSaltGenerated called on server.");
      else
        ((QueryProcessor) obj).CallTargetSaltGenerated(ClientScene.readyConnection, reader.ReadBytesAndSize());
    }

    protected static void InvokeRpcTargetReplyPassword(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetReplyPassword called on server.");
      else
        ((QueryProcessor) obj).CallTargetReplyPassword(ClientScene.readyConnection, reader.ReadBoolean());
    }

    protected static void InvokeRpcTargetReplyPlain(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetReplyPlain called on server.");
      else
        ((QueryProcessor) obj).CallTargetReplyPlain(ClientScene.readyConnection, reader.ReadString(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadString());
    }

    protected static void InvokeRpcTargetReplyEncrypted(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetReplyEncrypted called on server.");
      else
        ((QueryProcessor) obj).CallTargetReplyEncrypted(ClientScene.readyConnection, reader.ReadBytesAndSize(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadString());
    }

    protected static void InvokeRpcTargetSyncGameplayData(
      NetworkBehaviour obj,
      NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetSyncGameplayData called on server.");
      else
        ((QueryProcessor) obj).CallTargetSyncGameplayData(ClientScene.readyConnection, reader.ReadBoolean());
    }

    public void CallTargetSaltGenerated(NetworkConnection conn, byte[] salt)
    {
    }

    public void CallTargetReplyPassword(NetworkConnection conn, bool b)
    {
    }

    public void CallTargetReplyPlain(
      NetworkConnection conn,
      string content,
      bool isSuccess,
      bool logInConsole,
      string overrideDisplay)
    {
    }

    public void CallTargetReplyEncrypted(
      NetworkConnection conn,
      byte[] content,
      bool isSuccess,
      bool logInConsole,
      string overrideDisplay)
    {
    }

    public void CallTargetSyncGameplayData(NetworkConnection conn, bool gd)
    {
      this._gameplayData = gd;
    }

    static QueryProcessor()
    {
      NetworkBehaviour.RegisterCommandDelegate(typeof (QueryProcessor), "CmdRequestSalt", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeCmdCmdRequestSalt));
      NetworkBehaviour.RegisterCommandDelegate(typeof (QueryProcessor), "CmdSendPassword", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeCmdCmdSendPassword));
      NetworkBehaviour.RegisterCommandDelegate(typeof (QueryProcessor), "CmdSendEncryptedQuery", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeCmdCmdSendEncryptedQuery));
      NetworkBehaviour.RegisterCommandDelegate(typeof (QueryProcessor), "CmdSendQuery", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeCmdCmdSendQuery));
      NetworkBehaviour.RegisterRpcDelegate(typeof (QueryProcessor), "TargetSaltGenerated", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeRpcTargetSaltGenerated));
      NetworkBehaviour.RegisterRpcDelegate(typeof (QueryProcessor), "TargetReplyPassword", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeRpcTargetReplyPassword));
      NetworkBehaviour.RegisterRpcDelegate(typeof (QueryProcessor), "TargetReplyPlain", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeRpcTargetReplyPlain));
      NetworkBehaviour.RegisterRpcDelegate(typeof (QueryProcessor), "TargetReplyEncrypted", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeRpcTargetReplyEncrypted));
      NetworkBehaviour.RegisterRpcDelegate(typeof (QueryProcessor), "TargetSyncGameplayData", new NetworkBehaviour.CmdDelegate(QueryProcessor.InvokeRpcTargetSyncGameplayData));
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WriteString(this.ServerRandom);
        writer.WritePackedInt32(this.PlayerId);
        writer.WriteBoolean(this.OverridePasswordEnabled);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WriteString(this.ServerRandom);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 2L) != 0L)
      {
        writer.WritePackedInt32(this.PlayerId);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 4L) != 0L)
      {
        writer.WriteBoolean(this.OverridePasswordEnabled);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        string random = reader.ReadString();
        this.SetServerRandom(random);
        this.NetworkServerRandom = random;
        this.NetworkPlayerId = reader.ReadPackedInt32();
        this.NetworkOverridePasswordEnabled = reader.ReadBoolean();
      }
      else
      {
        long num = (long) reader.ReadPackedUInt64();
        if ((num & 1L) != 0L)
        {
          string random = reader.ReadString();
          this.SetServerRandom(random);
          this.NetworkServerRandom = random;
        }
        if ((num & 2L) != 0L)
          this.NetworkPlayerId = reader.ReadPackedInt32();
        if ((num & 4L) == 0L)
          return;
        this.NetworkOverridePasswordEnabled = reader.ReadBoolean();
      }
    }
  }
}
