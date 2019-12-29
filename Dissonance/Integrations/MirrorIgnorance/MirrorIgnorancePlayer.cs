// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.MirrorIgnorancePlayer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Dissonance.Integrations.MirrorIgnorance
{
  [RequireComponent(typeof (NetworkIdentity))]
  public class MirrorIgnorancePlayer : NetworkBehaviour, IDissonancePlayer
  {
    private static readonly Log Log = Logs.Create(LogCategory.Network, "Mirror Player Component");
    private DissonanceComms _comms;
    [SyncVar]
    private string _playerId;

    public bool IsTracking { get; private set; }

    public string PlayerId
    {
      get
      {
        return this._playerId;
      }
    }

    public Vector3 Position
    {
      get
      {
        return this.transform.position;
      }
    }

    public Quaternion Rotation
    {
      get
      {
        return this.transform.rotation;
      }
    }

    public NetworkPlayerType Type
    {
      get
      {
        if ((UnityEngine.Object) this._comms == (UnityEngine.Object) null || this._playerId == null)
          return NetworkPlayerType.Unknown;
        return !this._comms.LocalPlayerName.Equals(this._playerId) ? NetworkPlayerType.Remote : NetworkPlayerType.Local;
      }
    }

    public void OnDestroy()
    {
      if (!((UnityEngine.Object) this._comms != (UnityEngine.Object) null))
        return;
      this._comms.LocalPlayerNameChanged -= new Action<string>(this.SetPlayerName);
    }

    public override void OnStartClient()
    {
      base.OnStartClient();
      this._comms = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
      this.StartInit();
    }

    public void OnDisable()
    {
      if (!this.IsTracking)
        return;
      this.StopTracking();
    }

    public override void OnStartLocalPlayer()
    {
      base.OnStartLocalPlayer();
      DissonanceComms objectOfType = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
      if ((UnityEngine.Object) objectOfType == (UnityEngine.Object) null)
        throw MirrorIgnorancePlayer.Log.CreateUserErrorException("cannot find DissonanceComms component in scene", "not placing a DissonanceComms component on a game object in the scene", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-MirrorIgnorance/", "2D90A6C3-5F2B-4859-994C-EBBDDD4A10F4");
      if (objectOfType.LocalPlayerName != null)
        this.SetPlayerName(objectOfType.LocalPlayerName);
      objectOfType.LocalPlayerNameChanged += new Action<string>(this.SetPlayerName);
    }

    private void SetPlayerName(string playerName)
    {
      if (this.IsTracking)
        this.StopTracking();
      this.Network_playerId = playerName;
      this.StartTracking();
      if (!this.isLocalPlayer)
        return;
      this.CmdSetPlayerName(playerName);
    }

    public void StartInit()
    {
      if (string.IsNullOrEmpty(this.PlayerId))
        return;
      this.StartTracking();
    }

    [Command]
    private void CmdSetPlayerName(string playerName)
    {
      if (this.isServer)
      {
        this.CallCmdSetPlayerName(playerName);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(playerName);
        this.SendCommandInternal(typeof (MirrorIgnorancePlayer), nameof (CmdSetPlayerName), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    [ClientRpc]
    private void RpcSetPlayerName(string playerName)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(playerName);
      this.SendRPCInternal(typeof (MirrorIgnorancePlayer), nameof (RpcSetPlayerName), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    private void StartTracking()
    {
      if (this.IsTracking)
        throw MirrorIgnorancePlayer.Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "31971B1F-52FD-4FCF-89E9-67A17A917921");
      if (!((UnityEngine.Object) this._comms != (UnityEngine.Object) null))
        return;
      this._comms.TrackPlayerPosition((IDissonancePlayer) this);
      this.IsTracking = true;
    }

    private void StopTracking()
    {
      if (!this.IsTracking)
        throw MirrorIgnorancePlayer.Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "C7CF0174-0667-4F07-88E3-800ED652142D");
      if (!((UnityEngine.Object) this._comms != (UnityEngine.Object) null))
        return;
      this._comms.StopTracking((IDissonancePlayer) this);
      this.IsTracking = false;
    }

    static MirrorIgnorancePlayer()
    {
      NetworkBehaviour.RegisterCommandDelegate(typeof (MirrorIgnorancePlayer), "CmdSetPlayerName", new NetworkBehaviour.CmdDelegate(MirrorIgnorancePlayer.InvokeCmdCmdSetPlayerName));
      NetworkBehaviour.RegisterRpcDelegate(typeof (MirrorIgnorancePlayer), "RpcSetPlayerName", new NetworkBehaviour.CmdDelegate(MirrorIgnorancePlayer.InvokeRpcRpcSetPlayerName));
    }

    private void MirrorProcessed()
    {
    }

    public string Network_playerId
    {
      get
      {
        return this._playerId;
      }
      [param: In] set
      {
        this.SetSyncVar<string>(value, ref this._playerId, 1UL);
      }
    }

    protected static void InvokeCmdCmdSetPlayerName(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdSetPlayerName called on client.");
      else
        ((MirrorIgnorancePlayer) obj).CallCmdSetPlayerName(reader.ReadString());
    }

    public void CallCmdSetPlayerName(string playerName)
    {
      if (string.IsNullOrEmpty(playerName) || !string.IsNullOrEmpty(this._playerId))
        return;
      this.Network_playerId = playerName;
      this.RpcSetPlayerName(playerName);
    }

    protected static void InvokeRpcRpcSetPlayerName(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "RPC RpcSetPlayerName called on server.");
      else
        ((MirrorIgnorancePlayer) obj).CallRpcSetPlayerName(reader.ReadString());
    }

    public void CallRpcSetPlayerName(string playerName)
    {
      if (this.isLocalPlayer)
        return;
      this.SetPlayerName(playerName);
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WriteString(this._playerId);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WriteString(this._playerId);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        this.Network_playerId = reader.ReadString();
      }
      else
      {
        if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
          return;
        this.Network_playerId = reader.ReadString();
      }
    }
  }
}
