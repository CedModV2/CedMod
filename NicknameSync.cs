// Decompiled with JetBrains decompiler
// Type: NicknameSync
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class NicknameSync : NetworkBehaviour
{
  public LayerMask RaycastMask;
  private CharacterClassManager _ccm;
  private bool _nickSet;
  public float ViewRange;
  [SyncVar]
  private string _myNickSync;
  private string _firstNickname;
  private const ushort MaxNicknameLen = 48;

  public string MyNick
  {
    get
    {
      if (this._nickSet)
        return this._firstNickname;
      if (this._myNickSync == null)
        return "(null)";
      this._nickSet = true;
      this._firstNickname = this._myNickSync.Replace("<", "＜").Replace(">", "＞");
      if (this._firstNickname.Length > 48)
        this._firstNickname = this._firstNickname.Substring(0, 48);
      return this._firstNickname;
    }
    set
    {
      if (value == null)
        value = "(null)";
      this.Network_myNickSync = value.Length <= 48 ? value.Replace("<", "＜").Replace(">", "＞") : value.Replace("<", "＜").Replace(">", "＞").Substring(0, 48);
      if (!NetworkServer.active)
        return;
      this._nickSet = true;
      this._firstNickname = this._myNickSync;
    }
  }

  private void Start()
  {
    this._ccm = this.GetComponent<CharacterClassManager>();
    if (!this.isLocalPlayer)
      return;
    this.SetNick("Dedicated Server");
  }

  private void Update()
  {
  }

  [Command]
  private void CmdSetNick(string n)
  {
    if (this.isServer)
    {
      this.CallCmdSetNick(n);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(n);
      this.SendCommandInternal(typeof (NicknameSync), nameof (CmdSetNick), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ServerCallback]
  public void UpdateNickname(string n)
  {
    if (!NetworkServer.active)
      return;
    this._nickSet = true;
    if (n == null)
    {
      ServerConsole.AddLog("Banned " + this.connectionToClient.address + " for passing null name.");
      PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(this.gameObject, 26297460, "", "Server");
      this.SetNick("(null)");
    }
    else
    {
      StringBuilder stringBuilder = new StringBuilder();
      char highSurrogate = '0';
      bool flag = false;
      foreach (char ch in n)
      {
        if (char.IsLetterOrDigit(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
        {
          flag = true;
          stringBuilder.Append(ch);
        }
        else if (char.IsWhiteSpace(ch) && ch != '\n' && (ch != '\r' && ch != '\t'))
          stringBuilder.Append(ch);
        else if (char.IsHighSurrogate(ch))
          highSurrogate = ch;
        else if (char.IsLowSurrogate(ch) && char.IsSurrogatePair(highSurrogate, ch))
        {
          stringBuilder.Append(highSurrogate);
          stringBuilder.Append(ch);
          flag = true;
        }
      }
      string nick = stringBuilder.ToString();
      if (nick.Length > 32)
        nick = nick.Substring(0, 32);
      if (!flag)
      {
        ServerConsole.AddLog("Kicked " + this.connectionToClient.address + " for having an empty name.");
        ServerConsole.Disconnect(this.connectionToClient, "You may not have an empty name.");
        this.SetNick("Empty Name");
      }
      else
        this.SetNick(nick);
    }
  }

  private void SetNick(string nick)
  {
    this.MyNick = nick;
    ServerConsole.AddLog("Nickname of " + this._ccm.UserId + " is now " + nick + ".");
    ServerLogs.AddLog(ServerLogs.Modules.Networking, "Nickname of " + this._ccm.UserId + " is now " + nick + ".", ServerLogs.ServerLogType.ConnectionUpdate);
  }

  private void MirrorProcessed()
  {
  }

  public string Network_myNickSync
  {
    get
    {
      return this._myNickSync;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this._myNickSync, 1UL);
    }
  }

  protected static void InvokeCmdCmdSetNick(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetNick called on client.");
    else
      ((NicknameSync) obj).CallCmdSetNick(reader.ReadString());
  }

  public void CallCmdSetNick(string n)
  {
    if (this.isLocalPlayer)
    {
      this.MyNick = n;
    }
    else
    {
      if (this._nickSet || ConfigFile.ServerConfig.GetBool("online_mode", true))
        return;
      this._nickSet = true;
      if (n == null)
      {
        ServerConsole.AddLog("Banned " + this.connectionToClient.address + " for passing null name.");
        PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(this.gameObject, 26297460, "", "Server");
        this.SetNick("(null)");
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        char highSurrogate = '0';
        bool flag = false;
        foreach (char ch in n)
        {
          if (char.IsLetterOrDigit(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch))
          {
            flag = true;
            stringBuilder.Append(ch);
          }
          else if (char.IsWhiteSpace(ch) && ch != '\n' && (ch != '\r' && ch != '\t'))
            stringBuilder.Append(ch);
          else if (char.IsHighSurrogate(ch))
            highSurrogate = ch;
          else if (char.IsLowSurrogate(ch) && char.IsSurrogatePair(highSurrogate, ch))
          {
            stringBuilder.Append(highSurrogate);
            stringBuilder.Append(ch);
            flag = true;
          }
        }
        string str = stringBuilder.ToString();
        if (str.Length > 32)
          str = str.Substring(0, 32);
        if (!flag)
        {
          ServerConsole.AddLog("Kicked " + this.connectionToClient.address + " for having an empty name.");
          ServerConsole.Disconnect(this.connectionToClient, "You may not have an empty name.");
          this.SetNick("Empty Name");
        }
        else
        {
          this.SetNick(str.Replace("<", "＜").Replace(">", "＞").Replace("[", "(").Replace("]", ")"));
          this.GetComponent<CharacterClassManager>().SyncServerCmdBinding();
        }
      }
    }
  }

  static NicknameSync()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (NicknameSync), "CmdSetNick", new NetworkBehaviour.CmdDelegate(NicknameSync.InvokeCmdCmdSetNick));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this._myNickSync);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this._myNickSync);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Network_myNickSync = reader.ReadString();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.Network_myNickSync = reader.ReadString();
    }
  }
}
