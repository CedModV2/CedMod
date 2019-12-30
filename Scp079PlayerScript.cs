// Decompiled with JetBrains decompiler
// Type: Scp079PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using GameCore;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

public class Scp079PlayerScript : NetworkBehaviour
{
  public static List<Scp079PlayerScript> instances = new List<Scp079PlayerScript>();
  private List<Scp079Interactable> interactables = new List<Scp079Interactable>();
  public SyncListString lockedDoors = new SyncListString();
  [SyncVar]
  private float curMana = 100f;
  [SyncVar]
  public float maxMana = 100f;
  public List<Scp079Interactable> nearbyInteractables = new List<Scp079Interactable>();
  private List<Scp079Interaction> interactions = new List<Scp079Interaction>();
  public Scp079PlayerScript.Level079[] levels;
  public Scp079PlayerScript.Ability079[] abilities;
  public float[] generatorAuxRegenerationFactor;
  public static Camera079[] allCameras;
  private ServerRoles roles;
  public GameObject plyCamera;
  public Scp079PlayerScript.Ability079[] expEarnWays;
  [SyncVar]
  private string curSpeaker;
  private DissonanceUserSetup _dissonance;
  [SyncVar]
  private int curLvl;
  [SyncVar]
  private float curExp;
  public Camera079 currentCamera;
  public bool sameClass;
  public bool iAm079;
  public List<Scp079Interaction> interactionHistory;
  private RateLimit _interactRateLimit;
  private RateLimit _cameraSyncRateLimit;
  public string currentZone;
  public string currentRoom;
  public Camera079[] nearestCameras;
  private float ucpTimer;

  public int Lvl
  {
    get
    {
      return this.curLvl;
    }
    private set
    {
      this.NetworkcurLvl = Mathf.Clamp(value, 0, this.levels.Length - 1);
    }
  }

  public string Speaker
  {
    get
    {
      return this.curSpeaker;
    }
    private set
    {
      this.NetworkcurSpeaker = value;
      this._dissonance.SpeakerAs079 = !string.IsNullOrEmpty(value);
    }
  }

  public float Exp
  {
    get
    {
      return this.curExp;
    }
    private set
    {
      this.NetworkcurExp = value;
    }
  }

  public float Mana
  {
    get
    {
      return this.curMana;
    }
    private set
    {
      this.NetworkcurMana = value;
    }
  }

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._cameraSyncRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[7];
    this._dissonance = this.GetComponentInChildren<DissonanceUserSetup>();
    this.roles = this.GetComponent<ServerRoles>();
    if (this.isLocalPlayer || NetworkServer.active)
      Scp079PlayerScript.allCameras = UnityEngine.Object.FindObjectsOfType<Camera079>();
    if (this.isLocalPlayer)
      Interface079.lply = this;
    if (!NetworkServer.active)
      return;
    foreach (Scp079PlayerScript.Ability079 ability in this.abilities)
      ConfigFile.ServerConfig.GetInt("scp079_ability_" + ability.label.Replace(" ", "_").ToLower(), -1);
    this.OnExpChange();
  }

  private void Update()
  {
    this.RefreshInstances();
    this.UpdateCameraPosition();
    this.ServerUpdateMana();
  }

  [Server]
  public void AddExperience(float amount)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp079PlayerScript::AddExperience(System.Single)' called on client");
    }
    else
    {
      this.Exp += amount;
      this.OnExpChange();
    }
  }

  [Server]
  public void ForceLevel(int levelToForce, bool notifyUser)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp079PlayerScript::ForceLevel(System.Int32,System.Boolean)' called on client");
    }
    else
    {
      this.Lvl = levelToForce;
      if (!notifyUser)
        return;
      this.TargetLevelChanged(this.connectionToClient, levelToForce);
    }
  }

  [Server]
  public void ResetAll()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp079PlayerScript::ResetAll()' called on client");
    }
    else
    {
      this.interactionHistory.Clear();
      this.NetworkcurLvl = 0;
      this.NetworkcurExp = 0.0f;
      this.NetworkcurMana = 0.0f;
      this.RpcSwitchCamera(Interface079.singleton.defaultCamera.cameraId, false);
    }
  }

  [Server]
  private void OnExpChange()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp079PlayerScript::OnExpChange()' called on client");
    }
    else
    {
      while (this.curLvl < this.levels.Length - 1 && (double) this.curExp >= (double) this.levels[this.curLvl + 1].unlockExp)
      {
        int newLvl = this.curLvl + 1;
        this.Lvl = newLvl;
        this.Exp -= (float) this.levels[newLvl].unlockExp;
        this.NetworkmaxMana = this.levels[newLvl].maxMana;
        this.TargetLevelChanged(this.connectionToClient, newLvl);
      }
    }
  }

  [TargetRpc]
  private void TargetLevelChanged(NetworkConnection conn, int newLvl)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(newLvl);
    this.SendTargetRPCInternal(conn, typeof (Scp079PlayerScript), nameof (TargetLevelChanged), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Server]
  public void AddInteractionToHistory(GameObject go, string cmd, bool addMana)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp079PlayerScript::AddInteractionToHistory(UnityEngine.GameObject,System.String,System.Boolean)' called on client");
    }
    else
    {
      if ((UnityEngine.Object) go == (UnityEngine.Object) null)
        return;
      Scp079Interactable component = go.GetComponent<Scp079Interactable>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      if (addMana)
        this.RpcGainExp(ExpGainType.GeneralInteractions, (RoleType) component.type);
      this.interactionHistory.Add(new Scp079Interaction()
      {
        activationTime = Time.realtimeSinceStartup,
        interactable = component,
        command = cmd
      });
    }
  }

  [Command]
  public void CmdInteract(string command, GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdInteract(command, target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(command);
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Scp079PlayerScript), nameof (CmdInteract), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdResetDoors()
  {
    if (this.isServer)
    {
      this.CallCmdResetDoors();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Scp079PlayerScript), nameof (CmdResetDoors), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Server]
  public List<Scp079Interaction> ReturnRecentHistory(
    float timeLimit,
    Scp079Interactable.InteractableType[] filter = null)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Collections.Generic.List`1<Scp079Interaction> Scp079PlayerScript::ReturnRecentHistory(System.Single,Scp079Interactable/InteractableType[])' called on client");
      return (List<Scp079Interaction>) null;
    }
    this.interactions.Clear();
    foreach (Scp079Interaction scp079Interaction in this.interactionHistory)
    {
      if ((double) Time.realtimeSinceStartup - (double) scp079Interaction.activationTime <= (double) timeLimit)
      {
        bool flag = filter == null;
        if (filter != null)
        {
          foreach (Scp079Interactable.InteractableType interactableType in filter)
          {
            if (scp079Interaction.interactable.type == interactableType)
              flag = true;
          }
        }
        if (flag)
          this.interactions.Add(scp079Interaction);
      }
    }
    return this.interactions;
  }

  public Scp079Interactable.ZoneAndRoom GetOtherRoom()
  {
    Scp079Interactable.ZoneAndRoom zoneAndRoom = new Scp079Interactable.ZoneAndRoom()
    {
      currentRoom = "::NONE::",
      currentZone = "::NONE::"
    };
    RaycastHit hitInfo;
    if (!Physics.Raycast(new Ray(this.transform.position, Vector3.up), out hitInfo, 100f, (int) Interface079.singleton.roomDetectionMask))
      return zoneAndRoom;
    Transform transform = hitInfo.transform;
    while ((UnityEngine.Object) transform != (UnityEngine.Object) null && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !(transform.gameObject.tag == "Room"))
      transform = transform.transform.parent;
    if ((UnityEngine.Object) transform == (UnityEngine.Object) null)
      return zoneAndRoom;
    zoneAndRoom.currentRoom = transform.transform.name;
    zoneAndRoom.currentZone = transform.transform.parent.name;
    return zoneAndRoom;
  }

  public void RefreshCurrentRoom()
  {
    int num = 0;
    try
    {
      this.currentRoom = "::NONE::";
      this.currentZone = "::NONE::";
      num = 5;
      RaycastHit hitInfo;
      if (Physics.Raycast(new Ray(this.currentCamera.transform.position, Vector3.up), out hitInfo, 100f, (int) Interface079.singleton.roomDetectionMask) || Physics.Raycast(new Ray(this.currentCamera.transform.position, Vector3.down), out hitInfo, 100f, (int) Interface079.singleton.roomDetectionMask))
      {
        GameCore.Console.AddDebugLog("SCP079", "Hit:" + hitInfo.transform.name, MessageImportance.LessImportant, false);
        num = 1;
        Transform transform = hitInfo.transform;
        while ((UnityEngine.Object) transform != (UnityEngine.Object) null && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !(transform.gameObject.tag == "Room"))
          transform = transform.transform.parent;
        num = 2;
        if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
        {
          this.currentRoom = transform.transform.name;
          this.currentZone = transform.transform.parent.name;
        }
      }
      GameCore.Console.AddDebugLog("SCP079", "CurrentZone:" + this.currentZone, MessageImportance.LessImportant, false);
      num = 3;
      this.nearbyInteractables.Clear();
      if (Interface079.singleton.allInteractables == null)
        return;
      foreach (Scp079Interactable allInteractable in Interface079.singleton.allInteractables)
      {
        if ((UnityEngine.Object) allInteractable != (UnityEngine.Object) null && ((double) Vector3.Distance(allInteractable.transform.position, this.currentCamera.transform.position) < (this.currentZone == "Outside" ? 250.0 : 40.0) && allInteractable.IsVisible(this.currentZone, this.currentRoom)))
          this.nearbyInteractables.Add(allInteractable);
      }
    }
    catch
    {
      GameCore.Console.AddDebugLog("SCP079", "<color=red>ERROR: An unexpected error has occurred. Error code: " + (object) num + ".</color>", MessageImportance.MostImportant, false);
    }
  }

  private bool CheckInteractableLegitness(string cr, string cz, GameObject tr, bool allowNull)
  {
    if ((UnityEngine.Object) tr == (UnityEngine.Object) null)
      return allowNull;
    Scp079Interactable component = tr.GetComponent<Scp079Interactable>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
    {
      GameCore.Console.AddDebugLog("SCP079", "This gameobject (" + tr.name + ") is not a intractable element.", MessageImportance.LessImportant, false);
      return false;
    }
    foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in component.currentZonesAndRooms)
    {
      if (currentZonesAndRoom.currentRoom == cr && currentZonesAndRoom.currentZone == cz)
        return true;
    }
    GameCore.Console.AddDebugLog("SCP079", "No match. The received object is not a part of the current room.", MessageImportance.LessImportant, false);
    foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in component.currentZonesAndRooms)
      GameCore.Console.AddDebugLog("SCP079", currentZonesAndRoom.currentRoom + "!=" + cr + ":" + currentZonesAndRoom.currentZone + "!=" + cz, MessageImportance.LeastImportant, true);
    return false;
  }

  public float GetManaFromLabel(string label, Scp079PlayerScript.Ability079[] array)
  {
    if (this.roles.BypassMode && array == this.abilities)
      return 0.0f;
    foreach (Scp079PlayerScript.Ability079 ability079 in array)
    {
      if (ability079.label == label)
        return ability079.requiredAccessTier > this.curLvl + 1 ? float.PositiveInfinity : ability079.mana;
    }
    GameCore.Console.AddDebugLog("SCP079", "The requested ability (" + label + ") doesn't exist.", MessageImportance.Normal, false);
    return float.PositiveInfinity;
  }

  public void Init(RoleType classID, Role c)
  {
    if (!this.iAm079 && this.isLocalPlayer && classID == RoleType.Scp079)
      Interface079.singleton.RefreshInteractables();
    this.sameClass = c.team == Team.SCP;
    this.iAm079 = classID == RoleType.Scp079;
    if (!this.iAm079)
      return;
    if (!Scp079PlayerScript.instances.Contains(this))
      Scp079PlayerScript.instances.Add(this);
    if (!NetworkServer.active || !((UnityEngine.Object) this.currentCamera == (UnityEngine.Object) null) && !(this.currentCamera.transform.position == Vector3.zero))
      return;
    this.RpcSwitchCamera(Interface079.singleton.defaultCamera.cameraId, false);
  }

  [ServerCallback]
  private void ServerUpdateMana()
  {
    if (!NetworkServer.active || !NetworkServer.active || !this.iAm079)
      return;
    if (!string.IsNullOrEmpty(this.Speaker))
    {
      this.Mana -= this.GetManaFromLabel("Speaker Update", this.abilities) * Time.deltaTime;
      if ((double) this.Mana > 0.0)
        return;
      this.Speaker = string.Empty;
    }
    else if (this.lockedDoors.Count > 0)
    {
      this.Mana -= this.GetManaFromLabel("Door Lock", this.abilities) * Time.deltaTime * (float) this.lockedDoors.Count;
      if ((double) this.Mana > 0.0)
        return;
      this.lockedDoors.Clear();
    }
    else
      this.Mana = Mathf.Clamp(this.curMana + this.levels[this.curLvl].manaPerSecond * Time.deltaTime * this.generatorAuxRegenerationFactor[Generator079.mainGenerator.totalVoltage], 0.0f, this.levels[this.curLvl].maxMana);
  }

  private void RefreshInstances()
  {
    bool flag;
    do
    {
      flag = true;
      foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
      {
        if ((UnityEngine.Object) instance == (UnityEngine.Object) null || !instance.iAm079)
        {
          Scp079PlayerScript.instances.Remove(instance);
          flag = false;
          break;
        }
      }
    }
    while (!flag);
  }

  [Command]
  public void CmdSwitchCamera(ushort cameraId, bool lookatRotation)
  {
    if (this.isServer)
    {
      this.CallCmdSwitchCamera(cameraId, lookatRotation);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteUInt16(cameraId);
      writer.WriteBoolean(lookatRotation);
      this.SendCommandInternal(typeof (Scp079PlayerScript), nameof (CmdSwitchCamera), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public float CalculateCameraSwitchCost(Vector3 cameraPosition)
  {
    return (UnityEngine.Object) this.currentCamera == (UnityEngine.Object) null || this.roles.BypassMode ? 0.0f : (float) ((double) Vector3.Distance(cameraPosition, this.currentCamera.transform.position) * (double) this.GetManaFromLabel("Camera Switch", this.abilities) / 10.0);
  }

  [ClientRpc]
  private void RpcFlickerLights(string cr, string cz, float duration)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(cr);
    writer.WriteString(cz);
    writer.WriteSingle(duration);
    this.SendRPCInternal(typeof (Scp079PlayerScript), nameof (RpcFlickerLights), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcSwitchCamera(ushort camId, bool lookatRotation)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteUInt16(camId);
    writer.WriteBoolean(lookatRotation);
    this.SendRPCInternal(typeof (Scp079PlayerScript), nameof (RpcSwitchCamera), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void UpdateCameraPosition()
  {
    if (!this.isLocalPlayer || !this.iAm079 || (UnityEngine.Object) this.currentCamera == (UnityEngine.Object) null)
      return;
    this.ucpTimer += Time.deltaTime;
    if ((double) this.ucpTimer <= 0.150000005960464)
      return;
    this.CmdUpdateCameraPosition(this.currentCamera.cameraId, (short) this.currentCamera.curRot, (sbyte) this.currentCamera.curPitch);
    this.ucpTimer = 0.0f;
  }

  [Command]
  private void CmdUpdateCameraPosition(ushort cameraId, short rotation, sbyte pitch)
  {
    if (this.isServer)
    {
      this.CallCmdUpdateCameraPosition(cameraId, rotation, pitch);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteUInt16(cameraId);
      writer.WriteInt16(rotation);
      writer.WriteSByte(pitch);
      this.SendCommandInternal(typeof (Scp079PlayerScript), nameof (CmdUpdateCameraPosition), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcUpdateCameraPostion(ushort cameraId, short rotation, sbyte pitch)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteUInt16(cameraId);
    writer.WriteInt16(rotation);
    writer.WriteSByte(pitch);
    this.SendRPCInternal(typeof (Scp079PlayerScript), nameof (RpcUpdateCameraPostion), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcNotEnoughMana(float required, float cur)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(required);
    writer.WriteSingle(cur);
    this.SendRPCInternal(typeof (Scp079PlayerScript), nameof (RpcNotEnoughMana), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcGainExp(ExpGainType type, RoleType details)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32((int) type);
    writer.WritePackedInt32((int) details);
    this.SendRPCInternal(typeof (Scp079PlayerScript), nameof (RpcGainExp), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public Scp079PlayerScript()
  {
    this.InitSyncObject((SyncObject) this.lockedDoors);
  }

  static Scp079PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp079PlayerScript), "CmdInteract", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdInteract));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp079PlayerScript), "CmdResetDoors", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdResetDoors));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp079PlayerScript), "CmdSwitchCamera", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdSwitchCamera));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp079PlayerScript), "CmdUpdateCameraPosition", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdUpdateCameraPosition));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp079PlayerScript), "RpcFlickerLights", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcFlickerLights));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp079PlayerScript), "RpcSwitchCamera", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcSwitchCamera));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp079PlayerScript), "RpcUpdateCameraPostion", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcUpdateCameraPostion));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp079PlayerScript), "RpcNotEnoughMana", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcNotEnoughMana));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp079PlayerScript), "RpcGainExp", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcGainExp));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp079PlayerScript), "TargetLevelChanged", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcTargetLevelChanged));
  }

  private void MirrorProcessed()
  {
  }

  public string NetworkcurSpeaker
  {
    get
    {
      return this.curSpeaker;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.curSpeaker, 1UL);
    }
  }

  public int NetworkcurLvl
  {
    get
    {
      return this.curLvl;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.curLvl, 2UL);
    }
  }

  public float NetworkcurExp
  {
    get
    {
      return this.curExp;
    }
    [param: In] set
    {
      this.SetSyncVar<float>(value, ref this.curExp, 4UL);
    }
  }

  public float NetworkcurMana
  {
    get
    {
      return this.curMana;
    }
    [param: In] set
    {
      this.SetSyncVar<float>(value, ref this.curMana, 8UL);
    }
  }

  public float NetworkmaxMana
  {
    get
    {
      return this.maxMana;
    }
    [param: In] set
    {
      this.SetSyncVar<float>(value, ref this.maxMana, 16UL);
    }
  }

  protected static void InvokeCmdCmdInteract(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdInteract called on client.");
    else
      ((Scp079PlayerScript) obj).CallCmdInteract(reader.ReadString(), reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdResetDoors(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdResetDoors called on client.");
    else
      ((Scp079PlayerScript) obj).CallCmdResetDoors();
  }

  protected static void InvokeCmdCmdSwitchCamera(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSwitchCamera called on client.");
    else
      ((Scp079PlayerScript) obj).CallCmdSwitchCamera(reader.ReadUInt16(), reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdUpdateCameraPosition(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUpdateCameraPosition called on client.");
    else
      ((Scp079PlayerScript) obj).CallCmdUpdateCameraPosition(reader.ReadUInt16(), reader.ReadInt16(), reader.ReadSByte());
  }

  public void CallCmdInteract(string command, GameObject target)
  {
    if (!this._interactRateLimit.CanExecute(true) || !this.iAm079)
      return;
    GameCore.Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant, false);
    if (!command.Contains(":"))
      return;
    string[] strArray = command.Split(':');
    this.RefreshCurrentRoom();
    if (!this.CheckInteractableLegitness(this.currentRoom, this.currentZone, target, true))
      return;
    List<string> stringList = ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();
    string s = strArray[0];
    // ISSUE: reference to a compiler-generated method
    switch (<PrivateImplementationDetails>.ComputeStringHash(s))
    {
      case 762402896:
        if (!(s == "ELEVATORTELEPORT"))
          break;
        float manaFromLabel1 = this.GetManaFromLabel("Elevator Teleport", this.abilities);
        if ((double) manaFromLabel1 > (double) this.curMana)
        {
          this.RpcNotEnoughMana(manaFromLabel1, this.curMana);
          break;
        }
        Camera079 camera079 = (Camera079) null;
        foreach (Scp079Interactable nearbyInteractable in this.nearbyInteractables)
        {
          if (nearbyInteractable.type == Scp079Interactable.InteractableType.ElevatorTeleport)
            camera079 = nearbyInteractable.optionalObject.GetComponent<Camera079>();
        }
        if (!((UnityEngine.Object) camera079 != (UnityEngine.Object) null))
          break;
        this.RpcSwitchCamera(camera079.cameraId, false);
        this.Mana -= manaFromLabel1;
        this.AddInteractionToHistory(target, strArray[0], true);
        break;
      case 1024196740:
        if (!(s == "TESLA"))
          break;
        float manaFromLabel2 = this.GetManaFromLabel("Tesla Gate Burst", this.abilities);
        if ((double) manaFromLabel2 > (double) this.curMana)
        {
          this.RpcNotEnoughMana(manaFromLabel2, this.curMana);
          break;
        }
        GameObject go1 = GameObject.Find(this.currentZone + "/" + this.currentRoom + "/Gate");
        if (!((UnityEngine.Object) go1 != (UnityEngine.Object) null))
          break;
        go1.GetComponent<TeslaGate>().RpcInstantBurst();
        this.AddInteractionToHistory(go1, strArray[0], true);
        this.Mana -= manaFromLabel2;
        break;
      case 1881799010:
        if (!(s == "SPEAKER"))
          break;
        string name = this.currentZone + "/" + this.currentRoom + "/Scp079Speaker";
        GameObject go2 = GameObject.Find(name);
        float manaFromLabel3 = this.GetManaFromLabel("Speaker Start", this.abilities);
        if ((double) manaFromLabel3 * 1.5 > (double) this.curMana)
        {
          this.RpcNotEnoughMana(manaFromLabel3, this.curMana);
          break;
        }
        if (!((UnityEngine.Object) go2 != (UnityEngine.Object) null))
          break;
        this.Mana -= manaFromLabel3;
        this.Speaker = name;
        this.AddInteractionToHistory(go2, strArray[0], true);
        break;
      case 2214547930:
        if (!(s == "LOCKDOWN") || AlphaWarheadController.Host.inProgress)
          break;
        float manaFromLabel4 = this.GetManaFromLabel("Room Lockdown", this.abilities);
        if ((double) manaFromLabel4 > (double) this.curMana)
        {
          this.RpcNotEnoughMana(manaFromLabel4, this.curMana);
          break;
        }
        if ((UnityEngine.Object) GameObject.Find(this.currentZone + "/" + this.currentRoom) != (UnityEngine.Object) null)
        {
          List<Scp079Interactable> scp079InteractableList = new List<Scp079Interactable>();
          foreach (Scp079Interactable allInteractable in Interface079.singleton.allInteractables)
          {
            foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in allInteractable.currentZonesAndRooms)
            {
              if (currentZonesAndRoom.currentRoom == this.currentRoom && currentZonesAndRoom.currentZone == this.currentZone && ((double) allInteractable.transform.position.y - 100.0 < (double) this.currentCamera.transform.position.y && !scp079InteractableList.Contains(allInteractable)))
                scp079InteractableList.Add(allInteractable);
            }
          }
          GameObject go3 = (GameObject) null;
          foreach (Scp079Interactable scp079Interactable in scp079InteractableList)
          {
            switch (scp079Interactable.type)
            {
              case Scp079Interactable.InteractableType.Door:
                if (scp079Interactable.GetComponent<Door>().destroyed)
                {
                  GameCore.Console.AddDebugLog("SCP079", "Lockdown can't initiate, one of the doors were destroyed.", MessageImportance.LessImportant, false);
                  return;
                }
                continue;
              case Scp079Interactable.InteractableType.Lockdown:
                go3 = scp079Interactable.gameObject;
                continue;
              default:
                continue;
            }
          }
          if (scp079InteractableList.Count == 0 || (UnityEngine.Object) go3 == (UnityEngine.Object) null)
          {
            GameCore.Console.AddDebugLog("SCP079", "This room can't be locked down.", MessageImportance.LessImportant, false);
            break;
          }
          foreach (Scp079Interactable scp079Interactable in scp079InteractableList)
          {
            if (scp079Interactable.type == Scp079Interactable.InteractableType.Door)
            {
              Door component = scp079Interactable.GetComponent<Door>();
              if (component.locked || (double) component.scp079Lockdown > -2.5)
                return;
              if (component.isOpen)
                component.ChangeState(false);
              component.scp079Lockdown = 10f;
            }
          }
          this.RpcFlickerLights(this.currentRoom, this.currentZone, 8f);
          this.AddInteractionToHistory(go3, strArray[0], true);
          this.Mana -= this.GetManaFromLabel("Room Lockdown", this.abilities);
          break;
        }
        GameCore.Console.AddDebugLog("SCP079", "Room couldn't be specified.", MessageImportance.Normal, false);
        break;
      case 3114290502:
        if (!(s == "DOORLOCK") || AlphaWarheadController.Host.inProgress)
          break;
        if ((UnityEngine.Object) target == (UnityEngine.Object) null)
        {
          GameCore.Console.AddDebugLog("SCP079", "The door lock command requires a target.", MessageImportance.LessImportant, false);
          break;
        }
        Door component1 = target.GetComponent<Door>();
        if ((UnityEngine.Object) component1 == (UnityEngine.Object) null)
          break;
        // ISSUE: explicit non-virtual call
        // ISSUE: explicit non-virtual call
        if (stringList != null && __nonvirtual (stringList.Count) > 0 && (stringList != null && __nonvirtual (stringList.Contains(component1.DoorName))))
          GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
        if ((UnityEngine.Object) component1.sound_checkpointWarning != (UnityEngine.Object) null)
          break;
        float manaFromLabel5 = this.GetManaFromLabel("Door Lock Minimum", this.abilities);
        if ((double) manaFromLabel5 > (double) this.curMana)
        {
          this.RpcNotEnoughMana(manaFromLabel5, this.curMana);
          break;
        }
        if (component1.locked)
          break;
        string str = component1.transform.parent.name + "/" + component1.transform.name;
        if (!this.lockedDoors.Contains(str))
          this.lockedDoors.Add(str);
        component1.LockBy079();
        this.AddInteractionToHistory(component1.gameObject, strArray[0], true);
        this.Mana -= this.GetManaFromLabel("Door Lock Start", this.abilities);
        break;
      case 3149585726:
        if (!(s == "ELEVATORUSE"))
          break;
        float manaFromLabel6 = this.GetManaFromLabel("Elevator Use", this.abilities);
        if ((double) manaFromLabel6 > (double) this.curMana)
        {
          this.RpcNotEnoughMana(manaFromLabel6, this.curMana);
          break;
        }
        string empty = string.Empty;
        if (strArray.Length > 1)
          empty = strArray[1];
        foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
        {
          if (lift.elevatorName == empty && lift.UseLift())
          {
            this.Mana -= manaFromLabel6;
            bool flag = false;
            foreach (Lift.Elevator elevator in lift.elevators)
            {
              this.AddInteractionToHistory(elevator.door.GetComponentInParent<Scp079Interactable>().gameObject, strArray[0], !flag);
              flag = true;
            }
          }
        }
        break;
      case 3406927017:
        if (!(s == "DOOR") || AlphaWarheadController.Host.inProgress)
          break;
        if ((UnityEngine.Object) target == (UnityEngine.Object) null)
        {
          GameCore.Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant, false);
          break;
        }
        Door component2 = target.GetComponent<Door>();
        if ((UnityEngine.Object) component2 == (UnityEngine.Object) null)
          break;
        // ISSUE: explicit non-virtual call
        // ISSUE: explicit non-virtual call
        if (stringList != null && __nonvirtual (stringList.Count) > 0 && (stringList != null && __nonvirtual (stringList.Contains(component2.DoorName))))
        {
          GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
          break;
        }
        float manaFromLabel7 = this.GetManaFromLabel("Door Interaction " + (string.IsNullOrEmpty(component2.permissionLevel) ? "DEFAULT" : component2.permissionLevel), this.abilities);
        if ((double) manaFromLabel7 > (double) this.curMana)
        {
          GameCore.Console.AddDebugLog("SCP079", "Not enough mana.", MessageImportance.LeastImportant, false);
          this.RpcNotEnoughMana(manaFromLabel7, this.curMana);
          break;
        }
        if ((UnityEngine.Object) component2 != (UnityEngine.Object) null && component2.ChangeState079())
        {
          this.Mana -= manaFromLabel7;
          this.AddInteractionToHistory(target, strArray[0], true);
          GameCore.Console.AddDebugLog("SCP079", "Door state changed.", MessageImportance.LeastImportant, false);
          break;
        }
        GameCore.Console.AddDebugLog("SCP079", "Door state failed to change.", MessageImportance.LeastImportant, false);
        break;
      case 3613866178:
        if (!(s == "STOPSPEAKER"))
          break;
        this.Speaker = string.Empty;
        break;
    }
  }

  public void CallCmdResetDoors()
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.lockedDoors.Clear();
  }

  public void CallCmdSwitchCamera(ushort cameraId, bool lookatRotation)
  {
    if (!this._interactRateLimit.CanExecute(true) || !this.iAm079)
      return;
    Camera079 camera079 = (Camera079) null;
    foreach (Camera079 allCamera in Scp079PlayerScript.allCameras)
    {
      if ((int) allCamera.cameraId == (int) cameraId)
        camera079 = allCamera;
    }
    if ((UnityEngine.Object) camera079 == (UnityEngine.Object) null)
      return;
    float cameraSwitchCost = this.CalculateCameraSwitchCost(camera079.transform.position);
    if ((double) cameraSwitchCost > (double) this.curMana)
    {
      this.RpcNotEnoughMana(cameraSwitchCost, this.curMana);
    }
    else
    {
      this.RpcSwitchCamera(cameraId, lookatRotation);
      this.Mana -= cameraSwitchCost;
      this.currentCamera = camera079;
    }
  }

  public void CallCmdUpdateCameraPosition(ushort cameraId, short rotation, sbyte pitch)
  {
    if (!this._cameraSyncRateLimit.CanExecute(true) || !((UnityEngine.Object) this.currentCamera != (UnityEngine.Object) null) || (int) this.currentCamera.cameraId != (int) cameraId)
      return;
    this.RpcUpdateCameraPostion(cameraId, rotation, pitch);
  }

  protected static void InvokeRpcRpcFlickerLights(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcFlickerLights called on server.");
    else
      ((Scp079PlayerScript) obj).CallRpcFlickerLights(reader.ReadString(), reader.ReadString(), reader.ReadSingle());
  }

  protected static void InvokeRpcRpcSwitchCamera(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcSwitchCamera called on server.");
    else
      ((Scp079PlayerScript) obj).CallRpcSwitchCamera(reader.ReadUInt16(), reader.ReadBoolean());
  }

  protected static void InvokeRpcRpcUpdateCameraPostion(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcUpdateCameraPostion called on server.");
    else
      ((Scp079PlayerScript) obj).CallRpcUpdateCameraPostion(reader.ReadUInt16(), reader.ReadInt16(), reader.ReadSByte());
  }

  protected static void InvokeRpcRpcNotEnoughMana(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcNotEnoughMana called on server.");
    else
      ((Scp079PlayerScript) obj).CallRpcNotEnoughMana(reader.ReadSingle(), reader.ReadSingle());
  }

  protected static void InvokeRpcRpcGainExp(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcGainExp called on server.");
    else
      ((Scp079PlayerScript) obj).CallRpcGainExp((ExpGainType) reader.ReadPackedInt32(), (RoleType) reader.ReadPackedInt32());
  }

  protected static void InvokeRpcTargetLevelChanged(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetLevelChanged called on server.");
    else
      ((Scp079PlayerScript) obj).CallTargetLevelChanged(ClientScene.readyConnection, reader.ReadPackedInt32());
  }

  public void CallRpcFlickerLights(string cr, string cz, float duration)
  {
    List<Scp079Interactable> scp079InteractableList = new List<Scp079Interactable>();
    foreach (Scp079Interactable allInteractable in Interface079.singleton.allInteractables)
    {
      if ((UnityEngine.Object) allInteractable != (UnityEngine.Object) null)
      {
        foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in allInteractable.currentZonesAndRooms)
        {
          if (currentZonesAndRoom.currentRoom == cr && currentZonesAndRoom.currentZone == cz && ((double) allInteractable.transform.position.y - 100.0 < (double) this.currentCamera.transform.position.y && !scp079InteractableList.Contains(allInteractable)))
            scp079InteractableList.Add(allInteractable);
        }
      }
    }
    foreach (Scp079Interactable scp079Interactable in scp079InteractableList)
    {
      if (scp079Interactable.type == Scp079Interactable.InteractableType.Light)
      {
        FlickerableLight component = scp079Interactable.GetComponent<FlickerableLight>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
          Debug.LogError((object) ("This object has no Flickerable Light: " + (object) scp079Interactable.transform.parent + "/" + scp079Interactable.name));
        else
          component.EnableFlickering(duration);
      }
    }
  }

  public void CallRpcSwitchCamera(ushort camId, bool lookatRotation)
  {
    Camera079 camera079 = (Camera079) null;
    foreach (Camera079 allCamera in Scp079PlayerScript.allCameras)
    {
      if ((int) allCamera.cameraId == (int) camId)
        camera079 = allCamera;
    }
    this.currentCamera = camera079;
    if (!this.isLocalPlayer)
      return;
    if ((UnityEngine.Object) camera079 != (UnityEngine.Object) null)
      GameCore.Console.AddDebugLog("SCP079", "New camera: " + camera079.cameraName + " | ID: " + (object) camId, MessageImportance.LessImportant, false);
    Interface079.singleton.transitionInProgress = 0.0f;
    this.RefreshCurrentRoom();
    if (!lookatRotation)
      return;
    this.currentCamera.head.transform.rotation = Interface079.singleton.prevCamRotation;
    float minRot = this.currentCamera.minRot;
    float maxRot = this.currentCamera.maxRot;
    float[] numArray = new float[3];
    Quaternion localRotation;
    for (int index = -1; index <= 1; ++index)
    {
      localRotation = this.currentCamera.head.localRotation;
      float num = localRotation.eulerAngles.y + (float) (360 * index);
      if ((double) num < (double) minRot)
        numArray[index + 1] = Mathf.Abs(minRot - num);
      if ((double) num > (double) maxRot)
        numArray[index + 1] = Mathf.Abs(num - maxRot);
    }
    float num1 = float.PositiveInfinity;
    int num2 = 0;
    for (int index = 0; index < 3; ++index)
    {
      if ((double) numArray[index] < (double) num1)
      {
        num1 = numArray[index];
        num2 = index - 1;
      }
    }
    Camera079 currentCamera1 = this.currentCamera;
    localRotation = this.currentCamera.head.localRotation;
    double num3 = (double) localRotation.eulerAngles.y + (double) (360 * num2);
    currentCamera1.curRot = (float) num3;
    Camera079 currentCamera2 = this.currentCamera;
    localRotation = this.currentCamera.head.localRotation;
    double x = (double) localRotation.eulerAngles.x;
    currentCamera2.curPitch = (float) x;
  }

  public void CallRpcUpdateCameraPostion(ushort cameraId, short rotation, sbyte pitch)
  {
    if (this.isLocalPlayer)
      return;
    Camera079 camera079 = (Camera079) null;
    foreach (Camera079 allCamera in Scp079PlayerScript.allCameras)
    {
      if ((int) allCamera.cameraId == (int) cameraId)
      {
        camera079 = allCamera;
        break;
      }
    }
    if (!((UnityEngine.Object) camera079 != (UnityEngine.Object) null))
      return;
    camera079.UpdatePosition((float) rotation, (float) pitch);
  }

  public void CallRpcNotEnoughMana(float required, float cur)
  {
  }

  public void CallRpcGainExp(ExpGainType type, RoleType details)
  {
    switch (type)
    {
      case ExpGainType.KillAssist:
      case ExpGainType.PocketAssist:
        Team team = this.GetComponent<CharacterClassManager>().Classes.SafeGet(details).team;
        int num1 = 6;
        float amount1;
        switch (team)
        {
          case Team.SCP:
            amount1 = this.GetManaFromLabel("SCP Kill Assist", this.expEarnWays);
            num1 = 11;
            break;
          case Team.MTF:
            amount1 = this.GetManaFromLabel("MTF Kill Assist", this.expEarnWays);
            num1 = 9;
            break;
          case Team.CHI:
            amount1 = this.GetManaFromLabel("Chaos Kill Assist", this.expEarnWays);
            num1 = 8;
            break;
          case Team.RSC:
            amount1 = this.GetManaFromLabel("Scientist Kill Assist", this.expEarnWays);
            num1 = 10;
            break;
          case Team.CDP:
            amount1 = this.GetManaFromLabel("Class-D Kill Assist", this.expEarnWays);
            num1 = 7;
            break;
          default:
            amount1 = 0.0f;
            break;
        }
        int num2 = num1 - 1;
        if (type == ExpGainType.PocketAssist)
          amount1 /= 2f;
        if (!NetworkServer.active)
          break;
        this.AddExperience(amount1);
        break;
      case ExpGainType.AdminCheat:
        try
        {
          if (!NetworkServer.active)
            break;
          this.AddExperience((float) details);
          break;
        }
        catch
        {
          GameCore.Console.AddDebugLog("SCP079", "<color=red>ERROR: An unexpected error occured in RpcGainExp() while gaining points using Admin Cheats", MessageImportance.Normal, false);
          break;
        }
      case ExpGainType.GeneralInteractions:
        float num3 = 0.0f;
        switch (details)
        {
          case RoleType.ClassD:
            num3 = this.GetManaFromLabel("Door Interaction", this.expEarnWays);
            break;
          case RoleType.Spectator:
            num3 = this.GetManaFromLabel("Tesla Gate Activation", this.expEarnWays);
            break;
          case RoleType.Scientist:
            num3 = this.GetManaFromLabel("Lockdown Activation", this.expEarnWays);
            break;
          case RoleType.Scp079:
            num3 = this.GetManaFromLabel("Elevator Use", this.expEarnWays);
            break;
        }
        if ((double) num3 == 0.0)
          break;
        float num4 = 1f / Mathf.Clamp(this.levels[this.curLvl].manaPerSecond / 1.5f, 1f, 7f);
        float amount2 = Mathf.Round((float) ((double) num3 * (double) num4 * 10.0)) / 10f;
        if (!NetworkServer.active)
          break;
        this.AddExperience(amount2);
        break;
    }
  }

  public void CallTargetLevelChanged(NetworkConnection conn, int newLvl)
  {
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this.curSpeaker);
      writer.WritePackedInt32(this.curLvl);
      writer.WriteSingle(this.curExp);
      writer.WriteSingle(this.curMana);
      writer.WriteSingle(this.maxMana);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this.curSpeaker);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WritePackedInt32(this.curLvl);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteSingle(this.curExp);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteSingle(this.curMana);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 16L) != 0L)
    {
      writer.WriteSingle(this.maxMana);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkcurSpeaker = reader.ReadString();
      this.NetworkcurLvl = reader.ReadPackedInt32();
      this.NetworkcurExp = reader.ReadSingle();
      this.NetworkcurMana = reader.ReadSingle();
      this.NetworkmaxMana = reader.ReadSingle();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkcurSpeaker = reader.ReadString();
      if ((num & 2L) != 0L)
        this.NetworkcurLvl = reader.ReadPackedInt32();
      if ((num & 4L) != 0L)
        this.NetworkcurExp = reader.ReadSingle();
      if ((num & 8L) != 0L)
        this.NetworkcurMana = reader.ReadSingle();
      if ((num & 16L) == 0L)
        return;
      this.NetworkmaxMana = reader.ReadSingle();
    }
  }

  [Serializable]
  public class Level079
  {
    public string label;
    public int unlockExp;
    [Space]
    public float manaPerSecond;
    public float maxMana;
  }

  [Serializable]
  public class Ability079
  {
    public int requiredAccessTier = 1;
    public string label;
    [FormerlySerializedAs("manaCost")]
    public float mana;
  }
}
