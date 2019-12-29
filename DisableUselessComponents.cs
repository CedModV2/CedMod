// Decompiled with JetBrains decompiler
// Type: DisableUselessComponents
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod;
using GameCore;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DisableUselessComponents : NetworkBehaviour
{
  [SyncVar]
  private string label = "Player";
  [SyncVar]
  public bool isDedicated = true;
  private CharacterClassManager _ccm;
  private NicknameSync _ns;
  private bool _added;
  [SerializeField]
  private Behaviour[] uselessComponents;

  private void Start()
  {
    this._ns = this.GetComponent<NicknameSync>();
    if (NetworkServer.active)
      this.CmdSetName(this.isLocalPlayer ? "Host" : "Player", this.isLocalPlayer && ServerStatic.IsDedicated);
    this._ccm = this.GetComponent<CharacterClassManager>();
    if (!this.isLocalPlayer)
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.GetComponent<FirstPersonController>());
      foreach (Behaviour uselessComponent in this.uselessComponents)
        uselessComponent.enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.GetComponent<CharacterController>());
    }
    else
    {
      PlayerManager.localPlayer = this.gameObject;
      PlayerManager.LocalPlayerSet = true;
      PlayerManager.spect = this.GetComponent<SpectatorManager>();
      this.GetComponent<FirstPersonController>().enabled = false;
    }
  }

  private void FixedUpdate()
  {
    try
    {
      if (!this._added)
      {
        if (this._ccm.IsVerified)
        {
          if (!string.IsNullOrEmpty(this._ns.MyNick))
          {
            this._added = true;
            if (!this.isDedicated)
            {
              CharacterClassManager component = this.gameObject.GetComponent<CharacterClassManager>();
              Initializer.logger.Info("PlayerJoin", "Player joined: " + component.GetComponent<NicknameSync>().MyNick + " " + component.GetComponent<CharacterClassManager>().UserId + Environment.NewLine + "from IP: " + component.GetComponent<CharacterClassManager>().RequestIp);
              if (ConfigFile.ServerConfig.GetBool("cm_playerjoinbcenable", true))
              {
                foreach (string s in ConfigFile.ServerConfig.GetStringList("cm_playerjoinbc"))
                {
                  string[] strArray = s.Split(':');
                  string str = strArray[0];
                  uint uint16 = (uint) Convert.ToUInt16(strArray[1]);
                  Initializer.logger.Debug("PlayerJoin", "Raw Broadcast message: " + str);
                  foreach (string enclosedString in this.EnclosedStrings(s, "$S[", "]"))
                    str = str.Replace("$S[" + enclosedString + "]", ConfigFile.ServerConfig.GetString(enclosedString, "-"));
                  foreach (string enclosedString in this.EnclosedStrings(s, "$I[", "]"))
                    str = str.Replace("$I[" + enclosedString + "]", string.Concat((object) ConfigFile.ServerConfig.GetInt(enclosedString, 0)));
                  foreach (string enclosedString in this.EnclosedStrings(s, "$F[", "]"))
                    str = str.Replace("$F[" + enclosedString + "]", string.Concat((object) ConfigFile.ServerConfig.GetFloat(enclosedString, 0.0f)));
                  foreach (string enclosedString in this.EnclosedStrings(s, "$B[", "]"))
                    str = str.Replace("$B[" + enclosedString + "]", ConfigFile.ServerConfig.GetBool(enclosedString, false).ToString() ?? "");
                  int num = ServerConsole.PlayersAmount + 1;
                  string data = str.Replace("$curPlayerCount", num.ToString());
                  Initializer.logger.Debug("PlayerJoin", "Broadcasted message: " + data);
                  Initializer.logger.Debug("PlayerJoin", "With Duration: " + uint16.ToString());
                  Initializer.logger.Debug("PlayerJoin", "To player: " + component.GetComponent<NicknameSync>().MyNick);
                  QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(component.gameObject.GetComponent<NetworkIdentity>().connectionToClient, data, uint16, ConfigFile.ServerConfig.GetBool("cm_playerjoinbcmono", false));
                }
              }
              PlayerManager.AddPlayer(this.gameObject);
            }
          }
        }
      }
    }
    catch
    {
      GameCore.Console.AddDebugLog("MISCEL", string.Format("Error occured at FixedUpdate() in DisableUselessComponents.cs | DEBUG CODE: A{0}, C{1}, N{2}", (object) this._added, (object) ((UnityEngine.Object) this._ccm != (UnityEngine.Object) null), (object) ((UnityEngine.Object) this._ns != (UnityEngine.Object) null)), MessageImportance.LessImportant, false);
    }
    this.name = this.label;
  }

  private void OnDestroy()
  {
    if (this.isLocalPlayer)
      return;
    PlayerManager.RemovePlayer(this.gameObject);
  }

  [ServerCallback]
  private void CmdSetName(string n, bool b)
  {
    if (!NetworkServer.active)
      return;
    this.Networklabel = n;
    this.NetworkisDedicated = b;
  }

  private void MirrorProcessed()
  {
  }

  public string Networklabel
  {
    get
    {
      return this.label;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.label, 1UL);
    }
  }

  public bool NetworkisDedicated
  {
    get
    {
      return this.isDedicated;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isDedicated, 2UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this.label);
      writer.WriteBoolean(this.isDedicated);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this.label);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.isDedicated);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networklabel = reader.ReadString();
      this.NetworkisDedicated = reader.ReadBoolean();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.Networklabel = reader.ReadString();
      if ((num & 2L) == 0L)
        return;
      this.NetworkisDedicated = reader.ReadBoolean();
    }
  }

  private IEnumerable<string> EnclosedStrings(string s, string begin, string end)
  {
    int stop;
    for (int index = s.IndexOf(begin, 0); index >= 0; index = s.IndexOf(begin, stop + end.Length))
    {
      int startIndex = index + begin.Length;
      stop = s.IndexOf(end, startIndex);
      if (stop < 0)
        break;
      yield return s.Substring(startIndex, stop - startIndex);
    }
  }
}
