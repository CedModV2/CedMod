// Decompiled with JetBrains decompiler
// Type: PlayerPositionManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionManager : NetworkBehaviour
{
  public static PlayerPositionManager singleton;
  private bool isReadyToWork;
  private bool _enableAntiPlayerWallhack;
  private PlayerPositionData[] transmitBuffer;
  private PlayerPositionData[] receivedData;
  private int usedData;
  private const byte PlayerPacketSize = 20;
  private CharacterClassManager myCCM;
  private bool invisTutorial;
  private bool[] tutorials;

  private void Start()
  {
    this._enableAntiPlayerWallhack = ConfigFile.ServerConfig.GetBool("anti_player_wallhack", false);
    this.invisTutorial = ConfigFile.ServerConfig.GetBool("neon_invistutorial", false);
    NetworkClient.RegisterHandler<PlayerPositionManager.PositionMessage>(new Action<NetworkConnection, PlayerPositionManager.PositionMessage>(this.TransmitPosition), true);
    Timing.RunCoroutine(this._Start(), Segment.Update);
  }

  private IEnumerator<float> _Start()
  {
    PlayerPositionManager playerPositionManager = this;
    PlayerPositionManager.singleton = playerPositionManager;
    if (!ServerStatic.IsDedicated)
    {
      while ((UnityEngine.Object) PlayerManager.localPlayer == (UnityEngine.Object) null)
        yield return float.NegativeInfinity;
      CharacterClassManager local_ccm = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
      while (local_ccm.CurClass < RoleType.Scp173)
        yield return float.NegativeInfinity;
      local_ccm = (CharacterClassManager) null;
    }
    playerPositionManager.isReadyToWork = true;
  }

  private void FixedUpdate()
  {
    this.ReceiveData();
    if (!NetworkServer.active)
      return;
    this.TransmitData();
  }

  [ServerCallback]
  private void TransmitData()
  {
    if (!NetworkServer.active)
      return;
    List<GameObject> players = PlayerManager.players;
    this.usedData = players.Count;
    if (this.receivedData == null || this.receivedData.Length < this.usedData)
    {
      this.receivedData = new PlayerPositionData[this.usedData * 2];
      this.tutorials = new bool[this.usedData * 2];
    }
    for (int index = 0; index < this.usedData; ++index)
    {
      this.receivedData[index] = new PlayerPositionData(players[index]);
      this.tutorials[index] = this.invisTutorial && players[index].GetComponent<CharacterClassManager>().CurClass == RoleType.Tutorial;
    }
    if (this.transmitBuffer == null || this.transmitBuffer.Length < this.usedData)
      this.transmitBuffer = new PlayerPositionData[this.usedData * 2];
    foreach (GameObject gameObject in players)
    {
      CharacterClassManager component1 = gameObject.GetComponent<CharacterClassManager>();
      Array.Copy((Array) this.receivedData, (Array) this.transmitBuffer, this.usedData);
      if (component1.CurClass.Is939())
      {
        for (int index = 0; index < this.usedData; ++index)
        {
          if ((double) this.transmitBuffer[index].position.y < 800.0)
          {
            CharacterClassManager component2 = players[index].GetComponent<CharacterClassManager>();
            if (component2.Classes.SafeGet(component2.CurClass).team != Team.SCP && component2.Classes.SafeGet(component2.CurClass).team != Team.RIP && !players[index].GetComponent<Scp939_VisionController>().CanSee(component1.GetComponent<Scp939PlayerScript>()))
              this.transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, this.transmitBuffer[index].playerID, false);
          }
        }
      }
      else if (component1.CurClass == RoleType.Scp096)
      {
        Scp096PlayerScript component2 = gameObject.GetComponent<Scp096PlayerScript>();
        for (int index = 0; index < this.usedData; ++index)
        {
          if (component2.Neon096Rework && component2.Networkenraged == Scp096PlayerScript.RageState.Enraged && !component2.visiblePlys.ContainsKey(this.transmitBuffer[index].playerID))
            this.transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, this.transmitBuffer[index].playerID, false);
          else if (this.transmitBuffer[index].uses268)
            this.transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, this.transmitBuffer[index].playerID, false);
        }
      }
      else if (component1.CurClass == RoleType.Spectator)
      {
        for (int index = 0; index < this.usedData; ++index)
        {
          if (this.tutorials[index])
            this.transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, this.transmitBuffer[index].playerID, false);
        }
      }
      else if (component1.CurClass != RoleType.Scp079)
      {
        for (int index = 0; index < this.usedData; ++index)
        {
          if (this.transmitBuffer[index].uses268)
            this.transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, this.transmitBuffer[index].playerID, false);
        }
      }
      NetworkConnection networkConnection = component1.netIdentity.isLocalPlayer ? NetworkServer.localConnection : component1.netIdentity.connectionToClient;
      if (this.usedData <= 20)
      {
        networkConnection.Send<PlayerPositionManager.PositionMessage>(new PlayerPositionManager.PositionMessage(this.transmitBuffer, (byte) this.usedData, (byte) 0), 1);
      }
      else
      {
        byte part;
        for (part = (byte) 0; (int) part < this.usedData / 20; ++part)
          networkConnection.Send<PlayerPositionManager.PositionMessage>(new PlayerPositionManager.PositionMessage(this.transmitBuffer, (byte) 20, part), 1);
        byte count = (byte) (this.usedData % ((int) part * 20));
        if (count > (byte) 0)
          networkConnection.Send<PlayerPositionManager.PositionMessage>(new PlayerPositionManager.PositionMessage(this.transmitBuffer, count, part), 1);
      }
    }
  }

  private void TransmitPosition(NetworkConnection conn, PlayerPositionManager.PositionMessage data)
  {
    this.receivedData = data.Positions;
    int num = (int) data.Part * 20 + (int) data.Count;
    if (this.usedData >= num)
      return;
    this.usedData = num;
  }

  private void ReceiveData()
  {
    if (!this.isReadyToWork || this.usedData == 0)
      return;
    if ((UnityEngine.Object) this.myCCM != (UnityEngine.Object) null)
    {
      foreach (GameObject player in PlayerManager.players)
      {
        QueryProcessor component1 = player.GetComponent<QueryProcessor>();
        for (int index = 0; index < this.usedData; ++index)
        {
          PlayerPositionData playerPositionData = this.receivedData[index];
          if (component1.PlayerId == playerPositionData.playerID)
          {
            if (!component1.isLocalPlayer)
            {
              CharacterClassManager component2 = player.GetComponent<CharacterClassManager>();
              if (this.myCCM.Classes.CheckBounds(this.myCCM.CurClass) && (component2.CurClass != RoleType.Scp173 || !this.myCCM.IsHuman()) && (double) Vector3.Distance(player.transform.position, playerPositionData.position) < 10.0)
              {
                player.transform.position = Vector3.Lerp(player.transform.position, playerPositionData.position, 0.2f);
                this.SetRotation(component2, Quaternion.Lerp(Quaternion.Euler(player.transform.rotation.eulerAngles), Quaternion.Euler(Vector3.up * playerPositionData.rotation), 0.3f));
              }
              else
              {
                player.transform.position = playerPositionData.position;
                this.SetRotation(component2, Quaternion.Euler(0.0f, playerPositionData.rotation, 0.0f));
              }
            }
            if (!NetworkServer.active)
            {
              PlyMovementSync component2 = player.GetComponent<PlyMovementSync>();
              component2.RealModelPosition = playerPositionData.position;
              component2.Rotations.y = playerPositionData.rotation;
              break;
            }
            break;
          }
        }
      }
    }
    else
      this.myCCM = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
  }

  private void SetRotation(CharacterClassManager target, Quaternion quat)
  {
    if (target.CurClass == RoleType.Scp173 && this.myCCM.IsHuman() && !Scp173PlayerScript.Blinking)
      return;
    target.transform.rotation = quat;
  }

  private void MirrorProcessed()
  {
  }

  private struct PositionMessage : IMessageBase, IEquatable<PlayerPositionManager.PositionMessage>
  {
    public PlayerPositionData[] Positions;
    public byte Count;
    public byte Part;

    public PositionMessage(PlayerPositionData[] positions, byte count, byte part)
    {
      this.Positions = positions;
      this.Count = count;
      this.Part = part;
    }

    public void Deserialize(NetworkReader reader)
    {
      this.Part = reader.ReadByte();
      this.Count = reader.ReadByte();
      int num = (int) this.Part * 20 + (int) this.Count;
      if (PlayerPositionManager.singleton.receivedData != null)
      {
        if (PlayerPositionManager.singleton.receivedData.Length >= num)
        {
          this.Positions = PlayerPositionManager.singleton.receivedData;
        }
        else
        {
          this.Positions = new PlayerPositionData[num * 2];
          Array.Copy((Array) PlayerPositionManager.singleton.receivedData, (Array) this.Positions, PlayerPositionManager.singleton.receivedData.Length);
        }
      }
      else
        this.Positions = new PlayerPositionData[num * 2];
      for (int index = (int) this.Part * 20; index < num; ++index)
        this.Positions[index] = new PlayerPositionData(reader.ReadVector3(), reader.ReadSingle(), (int) reader.ReadPackedUInt32(), false);
    }

    public void Serialize(NetworkWriter writer)
    {
      writer.WriteByte(this.Part);
      writer.WriteByte(this.Count);
      for (int index = (int) this.Part * 20; index < (int) this.Part * 20 + (int) this.Count; ++index)
      {
        PlayerPositionData position = this.Positions[index];
        writer.WriteVector3(position.position);
        writer.WriteSingle(position.rotation);
        writer.WritePackedUInt32((uint) position.playerID);
      }
    }

    public bool Equals(PlayerPositionManager.PositionMessage other)
    {
      return this.Positions == other.Positions && (int) this.Count == (int) other.Count && (int) this.Part == (int) other.Part;
    }

    public override bool Equals(object obj)
    {
      return obj is PlayerPositionManager.PositionMessage other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((this.Positions != null ? this.Positions.GetHashCode() : 0) * 397 ^ this.Count.GetHashCode()) * 397 ^ this.Part.GetHashCode();
    }

    public static bool operator ==(
      PlayerPositionManager.PositionMessage left,
      PlayerPositionManager.PositionMessage right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      PlayerPositionManager.PositionMessage left,
      PlayerPositionManager.PositionMessage right)
    {
      return !left.Equals(right);
    }
  }
}
