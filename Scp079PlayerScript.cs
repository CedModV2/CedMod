using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets._Scripts.Dissonance;
using GameCore;
using Mirror;
using Security;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x0200018E RID: 398
public class Scp079PlayerScript : NetworkBehaviour
{
    // Token: 0x17000184 RID: 388
    // (get) Token: 0x06000993 RID: 2451 RVA: 0x0000F7A3 File Offset: 0x0000D9A3
    // (set) Token: 0x06000994 RID: 2452 RVA: 0x0000F7AB File Offset: 0x0000D9AB
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

    // Token: 0x17000185 RID: 389
    // (get) Token: 0x06000995 RID: 2453 RVA: 0x0000F7C4 File Offset: 0x0000D9C4
    // (set) Token: 0x06000996 RID: 2454 RVA: 0x0000F7CC File Offset: 0x0000D9CC
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

    // Token: 0x17000186 RID: 390
    // (get) Token: 0x06000997 RID: 2455 RVA: 0x0000F7E9 File Offset: 0x0000D9E9
    // (set) Token: 0x06000998 RID: 2456 RVA: 0x0000F7F1 File Offset: 0x0000D9F1
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

    // Token: 0x17000187 RID: 391
    // (get) Token: 0x06000999 RID: 2457 RVA: 0x0000F7FA File Offset: 0x0000D9FA
    // (set) Token: 0x0600099A RID: 2458 RVA: 0x0000F802 File Offset: 0x0000DA02
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

    // Token: 0x0600099B RID: 2459 RVA: 0x00045224 File Offset: 0x00043424
    private void Start()
    {
        this._interactRateLimit = base.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
        this._cameraSyncRateLimit = base.GetComponent<PlayerRateLimitHandler>().RateLimits[7];
        this._dissonance = base.GetComponentInChildren<DissonanceUserSetup>();
        this.roles = base.GetComponent<ServerRoles>();
        if (base.isLocalPlayer || NetworkServer.active)
        {
            Scp079PlayerScript.allCameras = UnityEngine.Object.FindObjectsOfType<Camera079>();
        }
        if (base.isLocalPlayer)
        {
            Interface079.lply = this;
        }
        if (!NetworkServer.active)
        {
            return;
        }
        foreach (Scp079PlayerScript.Ability079 ability in this.abilities)
        {
            ConfigFile.ServerConfig.GetInt("scp079_ability_" + ability.label.Replace(" ", "_").ToLower(), -1);
        }
        this.OnExpChange();
    }

    // Token: 0x0600099C RID: 2460 RVA: 0x0000F80B File Offset: 0x0000DA0B
    private void Update()
    {
        this.RefreshInstances();
        this.UpdateCameraPosition();
        this.ServerUpdateMana();
    }

    // Token: 0x0600099D RID: 2461 RVA: 0x0000F81F File Offset: 0x0000DA1F
    [Server]
    public void AddExperience(float amount)
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Server] function 'System.Void Scp079PlayerScript::AddExperience(System.Single)' called on client");
            return;
        }
        this.Exp += amount;
        this.OnExpChange();
    }

    // Token: 0x0600099E RID: 2462 RVA: 0x0000F84A File Offset: 0x0000DA4A
    [Server]
    public void ForceLevel(int levelToForce, bool notifyUser)
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Server] function 'System.Void Scp079PlayerScript::ForceLevel(System.Int32,System.Boolean)' called on client");
            return;
        }
        this.Lvl = levelToForce;
        if (!notifyUser)
        {
            return;
        }
        this.TargetLevelChanged(base.connectionToClient, levelToForce);
    }

    // Token: 0x0600099F RID: 2463 RVA: 0x000452F0 File Offset: 0x000434F0
    [Server]
    public void ResetAll()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Server] function 'System.Void Scp079PlayerScript::ResetAll()' called on client");
            return;
        }
        this.interactionHistory.Clear();
        this.NetworkcurLvl = 0;
        this.NetworkcurExp = 0f;
        this.NetworkcurMana = 0f;
        this.RpcSwitchCamera(Interface079.singleton.defaultCamera.cameraId, false);
    }

    // Token: 0x060009A0 RID: 2464 RVA: 0x00045350 File Offset: 0x00043550
    [Server]
    private void OnExpChange()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Server] function 'System.Void Scp079PlayerScript::OnExpChange()' called on client");
            return;
        }
        while (this.curLvl < this.levels.Length - 1 && this.curExp >= (float)this.levels[this.curLvl + 1].unlockExp)
        {
            int num = this.curLvl + 1;
            this.Lvl = num;
            this.Exp -= (float)this.levels[num].unlockExp;
            this.NetworkmaxMana = this.levels[num].maxMana;
            this.TargetLevelChanged(base.connectionToClient, num);
        }
    }

    // Token: 0x060009A1 RID: 2465 RVA: 0x000453F0 File Offset: 0x000435F0
    [TargetRpc]
    private void TargetLevelChanged(NetworkConnection conn, int newLvl)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WritePackedInt32(newLvl);
        this.SendTargetRPCInternal(conn, typeof(Scp079PlayerScript), "TargetLevelChanged", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009A2 RID: 2466 RVA: 0x00045430 File Offset: 0x00043630
    [Server]
    public void AddInteractionToHistory(GameObject go, string cmd, bool addMana)
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Server] function 'System.Void Scp079PlayerScript::AddInteractionToHistory(UnityEngine.GameObject,System.String,System.Boolean)' called on client");
            return;
        }
        if (go == null)
        {
            return;
        }
        Scp079Interactable component = go.GetComponent<Scp079Interactable>();
        if (component == null)
        {
            return;
        }
        if (addMana)
        {
            this.RpcGainExp(ExpGainType.GeneralInteractions, (global::RoleType)component.type);
        }
        this.interactionHistory.Add(new Scp079Interaction
        {
            activationTime = Time.realtimeSinceStartup,
            interactable = component,
            command = cmd
        });
    }

    // Token: 0x060009A3 RID: 2467 RVA: 0x000454A8 File Offset: 0x000436A8
    [Command]
    public void CmdInteract(string command, GameObject target)
    {
        if (base.isServer)
        {
            this.CallCmdInteract(command, target);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(command);
        writer.WriteGameObject(target);
        base.SendCommandInternal(typeof(Scp079PlayerScript), "CmdInteract", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009A4 RID: 2468 RVA: 0x0004550C File Offset: 0x0004370C
    [Command]
    public void CmdResetDoors()
    {
        if (base.isServer)
        {
            this.CallCmdResetDoors();
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        base.SendCommandInternal(typeof(Scp079PlayerScript), "CmdResetDoors", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009A5 RID: 2469 RVA: 0x00045554 File Offset: 0x00043754
    [Server]
    public List<Scp079Interaction> ReturnRecentHistory(float timeLimit, Scp079Interactable.InteractableType[] filter = null)
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("[Server] function 'System.Collections.Generic.List`1<Scp079Interaction> Scp079PlayerScript::ReturnRecentHistory(System.Single,Scp079Interactable/InteractableType[])' called on client");
            return null;
        }
        this.interactions.Clear();
        foreach (Scp079Interaction scp079Interaction in this.interactionHistory)
        {
            if (Time.realtimeSinceStartup - scp079Interaction.activationTime <= timeLimit)
            {
                bool flag = filter == null;
                if (filter != null)
                {
                    foreach (Scp079Interactable.InteractableType interactableType in filter)
                    {
                        if (scp079Interaction.interactable.type == interactableType)
                        {
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    this.interactions.Add(scp079Interaction);
                }
            }
        }
        return this.interactions;
    }

    // Token: 0x060009A6 RID: 2470 RVA: 0x00045624 File Offset: 0x00043824
    public Scp079Interactable.ZoneAndRoom GetOtherRoom()
    {
        Scp079Interactable.ZoneAndRoom result = new Scp079Interactable.ZoneAndRoom
        {
            currentRoom = "::NONE::",
            currentZone = "::NONE::"
        };
        RaycastHit raycastHit;
        if (!Physics.Raycast(new Ray(base.transform.position, Vector3.up), out raycastHit, 100f, Interface079.singleton.roomDetectionMask))
        {
            return result;
        }
        Transform transform = raycastHit.transform;
        while (transform != null && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !(transform.gameObject.tag == "Room"))
        {
            transform = transform.transform.parent;
        }
        if (transform == null)
        {
            return result;
        }
        result.currentRoom = transform.transform.name;
        result.currentZone = transform.transform.parent.name;
        return result;
    }

    // Token: 0x060009A7 RID: 2471 RVA: 0x0004570C File Offset: 0x0004390C
    public void RefreshCurrentRoom()
    {
        int num = 0;
        try
        {
            this.currentRoom = "::NONE::";
            this.currentZone = "::NONE::";
            num = 5;
            RaycastHit raycastHit;
            if (Physics.Raycast(new Ray(this.currentCamera.transform.position, Vector3.up), out raycastHit, 100f, Interface079.singleton.roomDetectionMask) || Physics.Raycast(new Ray(this.currentCamera.transform.position, Vector3.down), out raycastHit, 100f, Interface079.singleton.roomDetectionMask))
            {
                GameCore.Console.AddDebugLog("SCP079", "Hit:" + raycastHit.transform.name, MessageImportance.LessImportant, false);
                num = 1;
                Transform transform = raycastHit.transform;
                while (transform != null && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !(transform.gameObject.tag == "Room"))
                {
                    transform = transform.transform.parent;
                }
                num = 2;
                if (transform != null)
                {
                    this.currentRoom = transform.transform.name;
                    this.currentZone = transform.transform.parent.name;
                }
            }
            GameCore.Console.AddDebugLog("SCP079", "CurrentZone:" + this.currentZone, MessageImportance.LessImportant, false);
            num = 3;
            this.nearbyInteractables.Clear();
            if (Interface079.singleton.allInteractables != null)
            {
                foreach (Scp079Interactable scp079Interactable in Interface079.singleton.allInteractables)
                {
                    if (scp079Interactable != null && Vector3.Distance(scp079Interactable.transform.position, this.currentCamera.transform.position) < (float)((this.currentZone == "Outside") ? 250 : 40) && scp079Interactable.IsVisible(this.currentZone, this.currentRoom))
                    {
                        this.nearbyInteractables.Add(scp079Interactable);
                    }
                }
            }
        }
        catch
        {
            GameCore.Console.AddDebugLog("SCP079", "<color=red>ERROR: An unexpected error has occurred. Error code: " + num + ".</color>", MessageImportance.MostImportant, false);
        }
    }

    // Token: 0x060009A8 RID: 2472 RVA: 0x00045950 File Offset: 0x00043B50
    private bool CheckInteractableLegitness(string cr, string cz, GameObject tr, bool allowNull)
    {
        if (tr == null)
        {
            return allowNull;
        }
        Scp079Interactable component = tr.GetComponent<Scp079Interactable>();
        if (component == null)
        {
            GameCore.Console.AddDebugLog("SCP079", "This gameobject (" + tr.name + ") is not a intractable element.", MessageImportance.LessImportant, false);
            return false;
        }
        foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in component.currentZonesAndRooms)
        {
            if (zoneAndRoom.currentRoom == cr && zoneAndRoom.currentZone == cz)
            {
                return true;
            }
        }
        GameCore.Console.AddDebugLog("SCP079", "No match. The received object is not a part of the current room.", MessageImportance.LessImportant, false);
        foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom2 in component.currentZonesAndRooms)
        {
            GameCore.Console.AddDebugLog("SCP079", string.Concat(new string[]
            {
                zoneAndRoom2.currentRoom,
                "!=",
                cr,
                ":",
                zoneAndRoom2.currentZone,
                "!=",
                cz
            }), MessageImportance.LeastImportant, true);
        }
        return false;
    }

    // Token: 0x060009A9 RID: 2473 RVA: 0x00045A9C File Offset: 0x00043C9C
    public float GetManaFromLabel(string label, Scp079PlayerScript.Ability079[] array)
    {
        if (this.roles.BypassMode && array == this.abilities)
        {
            return 0f;
        }
        int i = 0;
        while (i < array.Length)
        {
            Scp079PlayerScript.Ability079 ability = array[i];
            if (ability.label == label)
            {
                if (ability.requiredAccessTier > this.curLvl + 1)
                {
                    return float.PositiveInfinity;
                }
                return ability.mana;
            }
            else
            {
                i++;
            }
        }
        GameCore.Console.AddDebugLog("SCP079", "The requested ability (" + label + ") doesn't exist.", MessageImportance.Normal, false);
        return float.PositiveInfinity;
    }

    // Token: 0x060009AA RID: 2474 RVA: 0x00045B28 File Offset: 0x00043D28
    public void Init(global::RoleType classID, Role c)
    {
        if (!this.iAm079 && base.isLocalPlayer && classID == global::RoleType.Scp079)
        {
            Interface079.singleton.RefreshInteractables();
        }
        this.sameClass = (c.team == Team.SCP);
        this.iAm079 = (classID == global::RoleType.Scp079);
        if (this.iAm079)
        {
            if (!Scp079PlayerScript.instances.Contains(this))
            {
                Scp079PlayerScript.instances.Add(this);
            }
            if (NetworkServer.active && (this.currentCamera == null || this.currentCamera.transform.position == Vector3.zero))
            {
                this.RpcSwitchCamera(Interface079.singleton.defaultCamera.cameraId, false);
            }
        }
    }

    // Token: 0x060009AB RID: 2475 RVA: 0x00045BD4 File Offset: 0x00043DD4
    [ServerCallback]
    private void ServerUpdateMana()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (!NetworkServer.active || !this.iAm079)
        {
            return;
        }
        if (!string.IsNullOrEmpty(this.Speaker))
        {
            this.Mana -= this.GetManaFromLabel("Speaker Update", this.abilities) * Time.deltaTime;
            if (this.Mana <= 0f)
            {
                this.Speaker = string.Empty;
                return;
            }
        }
        else if (this.lockedDoors.Count > 0)
        {
            this.Mana -= this.GetManaFromLabel("Door Lock", this.abilities) * Time.deltaTime * (float)this.lockedDoors.Count;
            if (this.Mana <= 0f)
            {
                this.lockedDoors.Clear();
                return;
            }
        }
        else
        {
            this.Mana = Mathf.Clamp(this.curMana + this.levels[this.curLvl].manaPerSecond * Time.deltaTime * this.generatorAuxRegenerationFactor[Generator079.mainGenerator.totalVoltage], 0f, this.levels[this.curLvl].maxMana);
        }
    }

    // Token: 0x060009AC RID: 2476 RVA: 0x00045CF4 File Offset: 0x00043EF4
    private void RefreshInstances()
    {
        bool flag;
        do
        {
            flag = true;
            foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
            {
                if (scp079PlayerScript == null || !scp079PlayerScript.iAm079)
                {
                    Scp079PlayerScript.instances.Remove(scp079PlayerScript);
                    flag = false;
                    break;
                }
            }
        }
        while (!flag);
    }

    // Token: 0x060009AD RID: 2477 RVA: 0x00045D68 File Offset: 0x00043F68
    [Command]
    public void CmdSwitchCamera(ushort cameraId, bool lookatRotation)
    {
        if (base.isServer)
        {
            this.CallCmdSwitchCamera(cameraId, lookatRotation);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteUInt16(cameraId);
        writer.WriteBoolean(lookatRotation);
        base.SendCommandInternal(typeof(Scp079PlayerScript), "CmdSwitchCamera", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009AE RID: 2478 RVA: 0x00045DCC File Offset: 0x00043FCC
    public float CalculateCameraSwitchCost(Vector3 cameraPosition)
    {
        if (this.currentCamera == null)
        {
            return 0f;
        }
        if (this.roles.BypassMode)
        {
            return 0f;
        }
        return Vector3.Distance(cameraPosition, this.currentCamera.transform.position) * this.GetManaFromLabel("Camera Switch", this.abilities) / 10f;
    }

    // Token: 0x060009AF RID: 2479 RVA: 0x00045E30 File Offset: 0x00044030
    [ClientRpc]
    private void RpcFlickerLights(string cr, string cz, float duration)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(cr);
        writer.WriteString(cz);
        writer.WriteSingle(duration);
        this.SendRPCInternal(typeof(Scp079PlayerScript), "RpcFlickerLights", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009B0 RID: 2480 RVA: 0x00045E84 File Offset: 0x00044084
    [ClientRpc]
    private void RpcSwitchCamera(ushort camId, bool lookatRotation)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteUInt16(camId);
        writer.WriteBoolean(lookatRotation);
        this.SendRPCInternal(typeof(Scp079PlayerScript), "RpcSwitchCamera", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009B1 RID: 2481 RVA: 0x00045ECC File Offset: 0x000440CC
    private void UpdateCameraPosition()
    {
        if (!base.isLocalPlayer || !this.iAm079)
        {
            return;
        }
        if (this.currentCamera == null)
        {
            return;
        }
        this.ucpTimer += Time.deltaTime;
        if (this.ucpTimer > 0.15f)
        {
            this.CmdUpdateCameraPosition(this.currentCamera.cameraId, (short)this.currentCamera.curRot, (sbyte)this.currentCamera.curPitch);
            this.ucpTimer = 0f;
        }
    }

    // Token: 0x060009B2 RID: 2482 RVA: 0x00045F4C File Offset: 0x0004414C
    [Command]
    private void CmdUpdateCameraPosition(ushort cameraId, short rotation, sbyte pitch)
    {
        if (base.isServer)
        {
            this.CallCmdUpdateCameraPosition(cameraId, rotation, pitch);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteUInt16(cameraId);
        writer.WriteInt16(rotation);
        writer.WriteSByte(pitch);
        base.SendCommandInternal(typeof(Scp079PlayerScript), "CmdUpdateCameraPosition", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009B3 RID: 2483 RVA: 0x00045FC0 File Offset: 0x000441C0
    [ClientRpc]
    private void RpcUpdateCameraPostion(ushort cameraId, short rotation, sbyte pitch)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteUInt16(cameraId);
        writer.WriteInt16(rotation);
        writer.WriteSByte(pitch);
        this.SendRPCInternal(typeof(Scp079PlayerScript), "RpcUpdateCameraPostion", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009B4 RID: 2484 RVA: 0x00046014 File Offset: 0x00044214
    [ClientRpc]
    private void RpcNotEnoughMana(float required, float cur)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteSingle(required);
        writer.WriteSingle(cur);
        this.SendRPCInternal(typeof(Scp079PlayerScript), "RpcNotEnoughMana", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009B5 RID: 2485 RVA: 0x0004605C File Offset: 0x0004425C
    [ClientRpc]
    public void RpcGainExp(ExpGainType type, global::RoleType details)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WritePackedInt32((int)type);
        writer.WritePackedInt32((int)details);
        this.SendRPCInternal(typeof(Scp079PlayerScript), "RpcGainExp", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x060009B6 RID: 2486 RVA: 0x000460A4 File Offset: 0x000442A4
    public Scp079PlayerScript()
    {
        base.InitSyncObject(this.lockedDoors);
    }

    // Token: 0x060009B7 RID: 2487 RVA: 0x00046108 File Offset: 0x00044308
    static Scp079PlayerScript()
    {
        NetworkBehaviour.RegisterCommandDelegate(typeof(Scp079PlayerScript), "CmdInteract", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdInteract));
        NetworkBehaviour.RegisterCommandDelegate(typeof(Scp079PlayerScript), "CmdResetDoors", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdResetDoors));
        NetworkBehaviour.RegisterCommandDelegate(typeof(Scp079PlayerScript), "CmdSwitchCamera", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdSwitchCamera));
        NetworkBehaviour.RegisterCommandDelegate(typeof(Scp079PlayerScript), "CmdUpdateCameraPosition", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeCmdCmdUpdateCameraPosition));
        NetworkBehaviour.RegisterRpcDelegate(typeof(Scp079PlayerScript), "RpcFlickerLights", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcFlickerLights));
        NetworkBehaviour.RegisterRpcDelegate(typeof(Scp079PlayerScript), "RpcSwitchCamera", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcSwitchCamera));
        NetworkBehaviour.RegisterRpcDelegate(typeof(Scp079PlayerScript), "RpcUpdateCameraPostion", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcUpdateCameraPostion));
        NetworkBehaviour.RegisterRpcDelegate(typeof(Scp079PlayerScript), "RpcNotEnoughMana", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcNotEnoughMana));
        NetworkBehaviour.RegisterRpcDelegate(typeof(Scp079PlayerScript), "RpcGainExp", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcRpcGainExp));
        NetworkBehaviour.RegisterRpcDelegate(typeof(Scp079PlayerScript), "TargetLevelChanged", new NetworkBehaviour.CmdDelegate(Scp079PlayerScript.InvokeRpcTargetLevelChanged));
    }

    // Token: 0x060009B8 RID: 2488 RVA: 0x00002FD1 File Offset: 0x000011D1
    private void MirrorProcessed()
    {
    }

    // Token: 0x17000188 RID: 392
    // (get) Token: 0x060009B9 RID: 2489 RVA: 0x00046260 File Offset: 0x00044460
    // (set) Token: 0x060009BA RID: 2490 RVA: 0x0000F879 File Offset: 0x0000DA79
    public string NetworkcurSpeaker
    {
        get
        {
            return this.curSpeaker;
        }
        [param: In]
        set
        {
            base.SetSyncVar<string>(value, ref this.curSpeaker, 1UL);
        }
    }

    // Token: 0x17000189 RID: 393
    // (get) Token: 0x060009BB RID: 2491 RVA: 0x00046274 File Offset: 0x00044474
    // (set) Token: 0x060009BC RID: 2492 RVA: 0x0000F891 File Offset: 0x0000DA91
    public int NetworkcurLvl
    {
        get
        {
            return this.curLvl;
        }
        [param: In]
        set
        {
            base.SetSyncVar<int>(value, ref this.curLvl, 2UL);
        }
    }

    // Token: 0x1700018A RID: 394
    // (get) Token: 0x060009BD RID: 2493 RVA: 0x00046288 File Offset: 0x00044488
    // (set) Token: 0x060009BE RID: 2494 RVA: 0x0000F8A9 File Offset: 0x0000DAA9
    public float NetworkcurExp
    {
        get
        {
            return this.curExp;
        }
        [param: In]
        set
        {
            base.SetSyncVar<float>(value, ref this.curExp, 4UL);
        }
    }

    // Token: 0x1700018B RID: 395
    // (get) Token: 0x060009BF RID: 2495 RVA: 0x0004629C File Offset: 0x0004449C
    // (set) Token: 0x060009C0 RID: 2496 RVA: 0x0000F8C1 File Offset: 0x0000DAC1
    public float NetworkcurMana
    {
        get
        {
            return this.curMana;
        }
        [param: In]
        set
        {
            base.SetSyncVar<float>(value, ref this.curMana, 8UL);
        }
    }

    // Token: 0x1700018C RID: 396
    // (get) Token: 0x060009C1 RID: 2497 RVA: 0x000462B0 File Offset: 0x000444B0
    // (set) Token: 0x060009C2 RID: 2498 RVA: 0x0000F8D9 File Offset: 0x0000DAD9
    public float NetworkmaxMana
    {
        get
        {
            return this.maxMana;
        }
        [param: In]
        set
        {
            base.SetSyncVar<float>(value, ref this.maxMana, 16UL);
        }
    }

    // Token: 0x060009C3 RID: 2499 RVA: 0x0000F8F1 File Offset: 0x0000DAF1
    protected static void InvokeCmdCmdInteract(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdInteract called on client.");
            return;
        }
        ((Scp079PlayerScript)obj).CallCmdInteract(reader.ReadString(), reader.ReadGameObject());
    }

    // Token: 0x060009C4 RID: 2500 RVA: 0x0000F920 File Offset: 0x0000DB20
    protected static void InvokeCmdCmdResetDoors(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdResetDoors called on client.");
            return;
        }
        ((Scp079PlayerScript)obj).CallCmdResetDoors();
    }

    // Token: 0x060009C5 RID: 2501 RVA: 0x0000F943 File Offset: 0x0000DB43
    protected static void InvokeCmdCmdSwitchCamera(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdSwitchCamera called on client.");
            return;
        }
        ((Scp079PlayerScript)obj).CallCmdSwitchCamera(reader.ReadUInt16(), reader.ReadBoolean());
    }

    // Token: 0x060009C6 RID: 2502 RVA: 0x0000F972 File Offset: 0x0000DB72
    protected static void InvokeCmdCmdUpdateCameraPosition(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdUpdateCameraPosition called on client.");
            return;
        }
        ((Scp079PlayerScript)obj).CallCmdUpdateCameraPosition(reader.ReadUInt16(), reader.ReadInt16(), reader.ReadSByte());
    }

    // Token: 0x060009C7 RID: 2503 RVA: 0x000462C4 File Offset: 0x000444C4
    public void CallCmdInteract(string command, GameObject target)
    {
        if (!this._interactRateLimit.CanExecute(true))
        {
            return;
        }
        if (!this.iAm079)
        {
            return;
        }
        GameCore.Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant, false);
        if (!command.Contains(":"))
        {
            return;
        }
        string[] array = command.Split(new char[]
        {
            ':'
        });
        this.RefreshCurrentRoom();
        if (!this.CheckInteractableLegitness(this.currentRoom, this.currentZone, target, true))
        {
            return;
        }
        List<string> list = ConfigFile.ServerConfig.GetStringList("scp079_door_blacklist") ?? new List<string>();
        string text = array[0];
        uint num = PrivateImplementationDetails.ComputeStringHash(text);
        if (num <= 2214547930U)
        {
            if (num <= 1024196740U)
            {
                if (num != 762402896U)
                {
                    if (num != 1024196740U)
                    {
                        return;
                    }
                    if (!(text == "TESLA"))
                    {
                        return;
                    }
                    float manaFromLabel = this.GetManaFromLabel("Tesla Gate Burst", this.abilities);
                    if (manaFromLabel > this.curMana)
                    {
                        this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                        return;
                    }
                    GameObject gameObject = GameObject.Find(this.currentZone + "/" + this.currentRoom + "/Gate");
                    if (gameObject != null)
                    {
                        gameObject.GetComponent<TeslaGate>().RpcInstantBurst();
                        this.AddInteractionToHistory(gameObject, array[0], true);
                        this.Mana -= manaFromLabel;
                        return;
                    }
                }
                else
                {
                    if (!(text == "ELEVATORTELEPORT"))
                    {
                        return;
                    }
                    float manaFromLabel = this.GetManaFromLabel("Elevator Teleport", this.abilities);
                    if (manaFromLabel > this.curMana)
                    {
                        this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                        return;
                    }
                    Camera079 camera = null;
                    foreach (Scp079Interactable scp079Interactable in this.nearbyInteractables)
                    {
                        if (scp079Interactable.type == Scp079Interactable.InteractableType.ElevatorTeleport)
                        {
                            camera = scp079Interactable.optionalObject.GetComponent<Camera079>();
                        }
                    }
                    if (camera != null)
                    {
                        this.RpcSwitchCamera(camera.cameraId, false);
                        this.Mana -= manaFromLabel;
                        this.AddInteractionToHistory(target, array[0], true);
                        return;
                    }
                }
            }
            else if (num != 1881799010U)
            {
                if (num != 2214547930U)
                {
                    return;
                }
                if (!(text == "LOCKDOWN"))
                {
                    return;
                }
                if (AlphaWarheadController.Host.inProgress)
                {
                    return;
                }
                float manaFromLabel = this.GetManaFromLabel("Room Lockdown", this.abilities);
                if (manaFromLabel > this.curMana)
                {
                    this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                    return;
                }
                if (GameObject.Find(this.currentZone + "/" + this.currentRoom) != null)
                {
                    List<Scp079Interactable> list2 = new List<Scp079Interactable>();
                    foreach (Scp079Interactable scp079Interactable2 in Interface079.singleton.allInteractables)
                    {
                        foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interactable2.currentZonesAndRooms)
                        {
                            if (zoneAndRoom.currentRoom == this.currentRoom && zoneAndRoom.currentZone == this.currentZone && scp079Interactable2.transform.position.y - 100f < this.currentCamera.transform.position.y && !list2.Contains(scp079Interactable2))
                            {
                                list2.Add(scp079Interactable2);
                            }
                        }
                    }
                    GameObject gameObject2 = null;
                    foreach (Scp079Interactable scp079Interactable3 in list2)
                    {
                        Scp079Interactable.InteractableType type = scp079Interactable3.type;
                        if (type != Scp079Interactable.InteractableType.Door)
                        {
                            if (type == Scp079Interactable.InteractableType.Lockdown)
                            {
                                gameObject2 = scp079Interactable3.gameObject;
                            }
                        }
                        else if (scp079Interactable3.GetComponent<Door>().destroyed)
                        {
                            GameCore.Console.AddDebugLog("SCP079", "Lockdown can't initiate, one of the doors were destroyed.", MessageImportance.LessImportant, false);
                            return;
                        }
                    }
                    if (list2.Count == 0 || gameObject2 == null)
                    {
                        GameCore.Console.AddDebugLog("SCP079", "This room can't be locked down.", MessageImportance.LessImportant, false);
                        return;
                    }
                    foreach (Scp079Interactable scp079Interactable4 in list2)
                    {
                        if (scp079Interactable4.type == Scp079Interactable.InteractableType.Door)
                        {
                            Door component = scp079Interactable4.GetComponent<Door>();
                            if (component.locked || component.scp079Lockdown > -2.5f)
                            {
                                return;
                            }
                            if (component.isOpen)
                            {
                                component.ChangeState(false);
                            }
                            component.scp079Lockdown = 10f;
                        }
                    }
                    this.RpcFlickerLights(this.currentRoom, this.currentZone, 8f);
                    this.AddInteractionToHistory(gameObject2, array[0], true);
                    this.Mana -= this.GetManaFromLabel("Room Lockdown", this.abilities);
                    return;
                }
                else
                {
                    GameCore.Console.AddDebugLog("SCP079", "Room couldn't be specified.", MessageImportance.Normal, false);
                }
            }
            else
            {
                if (!(text == "SPEAKER"))
                {
                    return;
                }
                string text2 = this.currentZone + "/" + this.currentRoom + "/Scp079Speaker";
                GameObject gameObject = GameObject.Find(text2);
                float manaFromLabel = this.GetManaFromLabel("Speaker Start", this.abilities);
                if (manaFromLabel * 1.5f > this.curMana)
                {
                    this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                    return;
                }
                if (gameObject != null)
                {
                    this.Mana -= manaFromLabel;
                    this.Speaker = text2;
                    this.AddInteractionToHistory(gameObject, array[0], true);
                    return;
                }
            }
            return;
        }
        if (num <= 3149585726U)
        {
            if (num != 3114290502U)
            {
                if (num != 3149585726U)
                {
                    return;
                }
                if (!(text == "ELEVATORUSE"))
                {
                    return;
                }
                float manaFromLabel = this.GetManaFromLabel("Elevator Use", this.abilities);
                if (manaFromLabel > this.curMana)
                {
                    this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                    return;
                }
                string b = string.Empty;
                if (array.Length > 1)
                {
                    b = array[1];
                }
                foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
                {
                    if (lift.elevatorName == b && lift.UseLift())
                    {
                        this.Mana -= manaFromLabel;
                        bool flag = false;
                        foreach (Lift.Elevator elevator in lift.elevators)
                        {
                            this.AddInteractionToHistory(elevator.door.GetComponentInParent<Scp079Interactable>().gameObject, array[0], !flag);
                            flag = true;
                        }
                    }
                }
                return;
            }
            else
            {
                if (!(text == "DOORLOCK"))
                {
                    return;
                }
                if (AlphaWarheadController.Host.inProgress)
                {
                    return;
                }
                if (target == null)
                {
                    GameCore.Console.AddDebugLog("SCP079", "The door lock command requires a target.", MessageImportance.LessImportant, false);
                    return;
                }
                Door component = target.GetComponent<Door>();
                if (component == null)
                {
                    return;
                }
                if (list != null && list.Count > 0 && list != null && list.Contains(component.DoorName))
                {
                    GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                }
                if (component.sound_checkpointWarning != null)
                {
                    return;
                }
                float manaFromLabel = this.GetManaFromLabel("Door Lock Minimum", this.abilities);
                if (manaFromLabel > this.curMana)
                {
                    this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                    return;
                }
                if (component.locked)
                {
                    return;
                }
                string item = component.transform.parent.name + "/" + component.transform.name;
                if (!this.lockedDoors.Contains(item))
                {
                    this.lockedDoors.Add(item);
                }
                component.LockBy079();
                this.AddInteractionToHistory(component.gameObject, array[0], true);
                this.Mana -= this.GetManaFromLabel("Door Lock Start", this.abilities);
                return;
            }
        }
        else if (num != 3406927017U)
        {
            if (num != 3613866178U)
            {
                return;
            }
            if (!(text == "STOPSPEAKER"))
            {
                return;
            }
            this.Speaker = string.Empty;
            return;
        }
        else
        {
            if (!(text == "DOOR"))
            {
                return;
            }
            if (AlphaWarheadController.Host.inProgress)
            {
                return;
            }
            if (target == null)
            {
                GameCore.Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant, false);
                return;
            }
            Door component = target.GetComponent<Door>();
            if (component == null)
            {
                return;
            }
            if (list != null && list.Count > 0 && list != null && list.Contains(component.DoorName))
            {
                GameCore.Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant, false);
                return;
            }
            float manaFromLabel = this.GetManaFromLabel("Door Interaction " + (string.IsNullOrEmpty(component.permissionLevel) ? "DEFAULT" : component.permissionLevel), this.abilities);
            if (manaFromLabel > this.curMana)
            {
                GameCore.Console.AddDebugLog("SCP079", "Not enough mana.", MessageImportance.LeastImportant, false);
                this.RpcNotEnoughMana(manaFromLabel, this.curMana);
                return;
            }
            if (component != null && component.ChangeState079())
            {
                this.Mana -= manaFromLabel;
                this.AddInteractionToHistory(target, array[0], true);
                GameCore.Console.AddDebugLog("SCP079", "Door state changed.", MessageImportance.LeastImportant, false);
                return;
            }
            GameCore.Console.AddDebugLog("SCP079", "Door state failed to change.", MessageImportance.LeastImportant, false);
            return;
        }
    }

    // Token: 0x060009C8 RID: 2504 RVA: 0x0000F9A7 File Offset: 0x0000DBA7
    public void CallCmdResetDoors()
    {
        if (!this._interactRateLimit.CanExecute(true))
        {
            return;
        }
        this.lockedDoors.Clear();
    }

    // Token: 0x060009C9 RID: 2505 RVA: 0x00046BE0 File Offset: 0x00044DE0
    public void CallCmdSwitchCamera(ushort cameraId, bool lookatRotation)
    {
        if (!this._interactRateLimit.CanExecute(true))
        {
            return;
        }
        if (!this.iAm079)
        {
            return;
        }
        Camera079 camera = null;
        foreach (Camera079 camera2 in Scp079PlayerScript.allCameras)
        {
            if (camera2.cameraId == cameraId)
            {
                camera = camera2;
            }
        }
        if (camera == null)
        {
            return;
        }
        float num = this.CalculateCameraSwitchCost(camera.transform.position);
        if (num > this.curMana)
        {
            this.RpcNotEnoughMana(num, this.curMana);
            return;
        }
        this.RpcSwitchCamera(cameraId, lookatRotation);
        this.Mana -= num;
        this.currentCamera = camera;
    }

    // Token: 0x060009CA RID: 2506 RVA: 0x0000F9C3 File Offset: 0x0000DBC3
    public void CallCmdUpdateCameraPosition(ushort cameraId, short rotation, sbyte pitch)
    {
        if (!this._cameraSyncRateLimit.CanExecute(true))
        {
            return;
        }
        if (this.currentCamera != null && this.currentCamera.cameraId == cameraId)
        {
            this.RpcUpdateCameraPostion(cameraId, rotation, pitch);
        }
    }

    // Token: 0x060009CB RID: 2507 RVA: 0x0000F9F9 File Offset: 0x0000DBF9
    protected static void InvokeRpcRpcFlickerLights(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcFlickerLights called on server.");
            return;
        }
        ((Scp079PlayerScript)obj).CallRpcFlickerLights(reader.ReadString(), reader.ReadString(), reader.ReadSingle());
    }

    // Token: 0x060009CC RID: 2508 RVA: 0x0000FA2F File Offset: 0x0000DC2F
    protected static void InvokeRpcRpcSwitchCamera(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcSwitchCamera called on server.");
            return;
        }
        ((Scp079PlayerScript)obj).CallRpcSwitchCamera(reader.ReadUInt16(), reader.ReadBoolean());
    }

    // Token: 0x060009CD RID: 2509 RVA: 0x0000FA5E File Offset: 0x0000DC5E
    protected static void InvokeRpcRpcUpdateCameraPostion(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcUpdateCameraPostion called on server.");
            return;
        }
        ((Scp079PlayerScript)obj).CallRpcUpdateCameraPostion(reader.ReadUInt16(), reader.ReadInt16(), reader.ReadSByte());
    }

    // Token: 0x060009CE RID: 2510 RVA: 0x0000FA93 File Offset: 0x0000DC93
    protected static void InvokeRpcRpcNotEnoughMana(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcNotEnoughMana called on server.");
            return;
        }
        ((Scp079PlayerScript)obj).CallRpcNotEnoughMana(reader.ReadSingle(), reader.ReadSingle());
    }

    // Token: 0x060009CF RID: 2511 RVA: 0x0000FAC4 File Offset: 0x0000DCC4
    protected static void InvokeRpcRpcGainExp(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcGainExp called on server.");
            return;
        }
        ((Scp079PlayerScript)obj).CallRpcGainExp((ExpGainType)reader.ReadPackedInt32(), (global::RoleType)reader.ReadPackedInt32());
    }

    // Token: 0x060009D0 RID: 2512 RVA: 0x0000FAF3 File Offset: 0x0000DCF3
    protected static void InvokeRpcTargetLevelChanged(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetLevelChanged called on server.");
            return;
        }
        ((Scp079PlayerScript)obj).CallTargetLevelChanged(ClientScene.readyConnection, reader.ReadPackedInt32());
    }

    // Token: 0x060009D1 RID: 2513 RVA: 0x00046C80 File Offset: 0x00044E80
    public void CallRpcFlickerLights(string cr, string cz, float duration)
    {
        List<Scp079Interactable> list = new List<Scp079Interactable>();
        foreach (Scp079Interactable scp079Interactable in Interface079.singleton.allInteractables)
        {
            if (scp079Interactable != null)
            {
                foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interactable.currentZonesAndRooms)
                {
                    if (zoneAndRoom.currentRoom == cr && zoneAndRoom.currentZone == cz && scp079Interactable.transform.position.y - 100f < this.currentCamera.transform.position.y && !list.Contains(scp079Interactable))
                    {
                        list.Add(scp079Interactable);
                    }
                }
            }
        }
        foreach (Scp079Interactable scp079Interactable2 in list)
        {
            if (scp079Interactable2.type == Scp079Interactable.InteractableType.Light)
            {
                FlickerableLight component = scp079Interactable2.GetComponent<FlickerableLight>();
                if (component == null)
                {
                    Debug.LogError(string.Concat(new object[]
                    {
                        "This object has no Flickerable Light: ",
                        scp079Interactable2.transform.parent,
                        "/",
                        scp079Interactable2.name
                    }));
                }
                else
                {
                    component.EnableFlickering(duration);
                }
            }
        }
    }

    // Token: 0x060009D2 RID: 2514 RVA: 0x00046DF8 File Offset: 0x00044FF8
    public void CallRpcSwitchCamera(ushort camId, bool lookatRotation)
    {
        Camera079 camera = null;
        foreach (Camera079 camera2 in Scp079PlayerScript.allCameras)
        {
            if (camera2.cameraId == camId)
            {
                camera = camera2;
            }
        }
        this.currentCamera = camera;
        if (!base.isLocalPlayer)
        {
            return;
        }
        if (camera != null)
        {
            GameCore.Console.AddDebugLog("SCP079", string.Concat(new object[]
            {
                "New camera: ",
                camera.cameraName,
                " | ID: ",
                camId
            }), MessageImportance.LessImportant, false);
        }
        Interface079.singleton.transitionInProgress = 0f;
        this.RefreshCurrentRoom();
        if (!lookatRotation)
        {
            return;
        }
        this.currentCamera.head.transform.rotation = Interface079.singleton.prevCamRotation;
        float minRot = this.currentCamera.minRot;
        float maxRot = this.currentCamera.maxRot;
        float[] array2 = new float[3];
        for (int j = -1; j <= 1; j++)
        {
            float num = this.currentCamera.head.localRotation.eulerAngles.y + (float)(360 * j);
            if (num < minRot)
            {
                array2[j + 1] = Mathf.Abs(minRot - num);
            }
            if (num > maxRot)
            {
                array2[j + 1] = Mathf.Abs(num - maxRot);
            }
        }
        float num2 = float.PositiveInfinity;
        int num3 = 0;
        for (int k = 0; k < 3; k++)
        {
            if (array2[k] < num2)
            {
                num2 = array2[k];
                num3 = k - 1;
            }
        }
        this.currentCamera.curRot = this.currentCamera.head.localRotation.eulerAngles.y + (float)(360 * num3);
        this.currentCamera.curPitch = this.currentCamera.head.localRotation.eulerAngles.x;
    }

    // Token: 0x060009D3 RID: 2515 RVA: 0x00046FCC File Offset: 0x000451CC
    public void CallRpcUpdateCameraPostion(ushort cameraId, short rotation, sbyte pitch)
    {
        if (base.isLocalPlayer)
        {
            return;
        }
        Camera079 camera = null;
        foreach (Camera079 camera2 in Scp079PlayerScript.allCameras)
        {
            if (camera2.cameraId == cameraId)
            {
                camera = camera2;
                break;
            }
        }
        if (camera != null)
        {
            camera.UpdatePosition((float)rotation, (float)pitch);
        }
    }

    // Token: 0x060009D4 RID: 2516 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallRpcNotEnoughMana(float required, float cur)
    {
    }

    // Token: 0x060009D5 RID: 2517 RVA: 0x0004701C File Offset: 0x0004521C
    public void CallRpcGainExp(ExpGainType type, global::RoleType details)
    {
        switch (type)
        {
            case ExpGainType.KillAssist:
            case ExpGainType.PocketAssist:
                {
                    Team team = base.GetComponent<CharacterClassManager>().Classes.SafeGet(details).team;
                    int num = 6;
                    float num2;
                    switch (team)
                    {
                        case Team.SCP:
                            num2 = this.GetManaFromLabel("SCP Kill Assist", this.expEarnWays);
                            num = 11;
                            break;
                        case Team.MTF:
                            num2 = this.GetManaFromLabel("MTF Kill Assist", this.expEarnWays);
                            num = 9;
                            break;
                        case Team.CHI:
                            num2 = this.GetManaFromLabel("Chaos Kill Assist", this.expEarnWays);
                            num = 8;
                            break;
                        case Team.RSC:
                            num2 = this.GetManaFromLabel("Scientist Kill Assist", this.expEarnWays);
                            num = 10;
                            break;
                        case Team.CDP:
                            num2 = this.GetManaFromLabel("Class-D Kill Assist", this.expEarnWays);
                            num = 7;
                            break;
                        default:
                            num2 = 0f;
                            break;
                    }
                    num--;
                    if (type == ExpGainType.PocketAssist)
                    {
                        num2 /= 2f;
                    }
                    if (NetworkServer.active)
                    {
                        this.AddExperience(num2);
                        return;
                    }
                    break;
                }
            case ExpGainType.DirectKill:
            case ExpGainType.HardwareHack:
                break;
            case ExpGainType.AdminCheat:
                try
                {
                    if (NetworkServer.active)
                    {
                        this.AddExperience((float)details);
                    }
                }
                catch
                {
                    GameCore.Console.AddDebugLog("SCP079", "<color=red>ERROR: An unexpected error occured in RpcGainExp() while gaining points using Admin Cheats", MessageImportance.Normal, false);
                }
                break;
            case ExpGainType.GeneralInteractions:
                {
                    float num3 = 0f;
                    switch (details)
                    {
                        case global::RoleType.ClassD:
                            num3 = this.GetManaFromLabel("Door Interaction", this.expEarnWays);
                            break;
                        case global::RoleType.Spectator:
                            num3 = this.GetManaFromLabel("Tesla Gate Activation", this.expEarnWays);
                            break;
                        case global::RoleType.Scientist:
                            num3 = this.GetManaFromLabel("Lockdown Activation", this.expEarnWays);
                            break;
                        case global::RoleType.Scp079:
                            num3 = this.GetManaFromLabel("Elevator Use", this.expEarnWays);
                            break;
                    }
                    if (num3 != 0f)
                    {
                        float num4 = 1f / Mathf.Clamp(this.levels[this.curLvl].manaPerSecond / 1.5f, 1f, 7f);
                        num3 = Mathf.Round(num3 * num4 * 10f) / 10f;
                        if (NetworkServer.active)
                        {
                            this.AddExperience(num3);
                            return;
                        }
                    }
                    break;
                }
            default:
                return;
        }
    }

    // Token: 0x060009D6 RID: 2518 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetLevelChanged(NetworkConnection conn, int newLvl)
    {
    }

    // Token: 0x060009D7 RID: 2519 RVA: 0x00047228 File Offset: 0x00045428
    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
        bool result = base.OnSerialize(writer, forceAll);
        if (forceAll)
        {
            writer.WriteString(this.curSpeaker);
            writer.WritePackedInt32(this.curLvl);
            writer.WriteSingle(this.curExp);
            writer.WriteSingle(this.curMana);
            writer.WriteSingle(this.maxMana);
            return true;
        }
        writer.WritePackedUInt64(base.syncVarDirtyBits);
        if ((base.syncVarDirtyBits & 1UL) != 0UL)
        {
            writer.WriteString(this.curSpeaker);
            result = true;
        }
        if ((base.syncVarDirtyBits & 2UL) != 0UL)
        {
            writer.WritePackedInt32(this.curLvl);
            result = true;
        }
        if ((base.syncVarDirtyBits & 4UL) != 0UL)
        {
            writer.WriteSingle(this.curExp);
            result = true;
        }
        if ((base.syncVarDirtyBits & 8UL) != 0UL)
        {
            writer.WriteSingle(this.curMana);
            result = true;
        }
        if ((base.syncVarDirtyBits & 16UL) != 0UL)
        {
            writer.WriteSingle(this.maxMana);
            result = true;
        }
        return result;
    }

    // Token: 0x060009D8 RID: 2520 RVA: 0x00047344 File Offset: 0x00045544
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
        if (initialState)
        {
            string networkcurSpeaker = reader.ReadString();
            this.NetworkcurSpeaker = networkcurSpeaker;
            int networkcurLvl = reader.ReadPackedInt32();
            this.NetworkcurLvl = networkcurLvl;
            float networkcurExp = reader.ReadSingle();
            this.NetworkcurExp = networkcurExp;
            float networkcurMana = reader.ReadSingle();
            this.NetworkcurMana = networkcurMana;
            float networkmaxMana = reader.ReadSingle();
            this.NetworkmaxMana = networkmaxMana;
            return;
        }
        long num = (long)reader.ReadPackedUInt64();
        if ((num & 1L) != 0L)
        {
            string networkcurSpeaker2 = reader.ReadString();
            this.NetworkcurSpeaker = networkcurSpeaker2;
        }
        if ((num & 2L) != 0L)
        {
            int networkcurLvl2 = reader.ReadPackedInt32();
            this.NetworkcurLvl = networkcurLvl2;
        }
        if ((num & 4L) != 0L)
        {
            float networkcurExp2 = reader.ReadSingle();
            this.NetworkcurExp = networkcurExp2;
        }
        if ((num & 8L) != 0L)
        {
            float networkcurMana2 = reader.ReadSingle();
            this.NetworkcurMana = networkcurMana2;
        }
        if ((num & 16L) != 0L)
        {
            float networkmaxMana2 = reader.ReadSingle();
            this.NetworkmaxMana = networkmaxMana2;
        }
    }
    internal sealed class PrivateImplementationDetails
    {
        internal static uint ComputeStringHash(string s)
        {
            uint num = new uint();
            if (s != null)
            {
                num = 0x811c9dc5;
                for (int i = 0; i < s.Length; i++)
                {
                    num = (s[i] ^ num) * 0x1000193;
                }
            }
            return num;
        }
    }

    // Token: 0x04000BC4 RID: 3012
    public Scp079PlayerScript.Level079[] levels;

    // Token: 0x04000BC5 RID: 3013
    public Scp079PlayerScript.Ability079[] abilities;

    // Token: 0x04000BC6 RID: 3014
    public float[] generatorAuxRegenerationFactor;

    // Token: 0x04000BC7 RID: 3015
    public static List<Scp079PlayerScript> instances = new List<Scp079PlayerScript>();

    // Token: 0x04000BC8 RID: 3016
    public static Camera079[] allCameras;

    // Token: 0x04000BC9 RID: 3017
    private ServerRoles roles;

    // Token: 0x04000BCA RID: 3018
    public GameObject plyCamera;

    // Token: 0x04000BCB RID: 3019
    public Scp079PlayerScript.Ability079[] expEarnWays;

    // Token: 0x04000BCC RID: 3020
    private List<Scp079Interactable> interactables = new List<Scp079Interactable>();

    // Token: 0x04000BCD RID: 3021
    [SyncVar]
    private string curSpeaker;

    // Token: 0x04000BCE RID: 3022
    public SyncListString lockedDoors = new SyncListString();

    // Token: 0x04000BCF RID: 3023
    private DissonanceUserSetup _dissonance;

    // Token: 0x04000BD0 RID: 3024
    [SyncVar]
    private int curLvl;

    // Token: 0x04000BD1 RID: 3025
    [SyncVar]
    private float curExp;

    // Token: 0x04000BD2 RID: 3026
    [SyncVar]
    private float curMana = 100f;

    // Token: 0x04000BD3 RID: 3027
    [SyncVar]
    public float maxMana = 100f;

    // Token: 0x04000BD4 RID: 3028
    public Camera079 currentCamera;

    // Token: 0x04000BD5 RID: 3029
    public bool sameClass;

    // Token: 0x04000BD6 RID: 3030
    public bool iAm079;

    // Token: 0x04000BD7 RID: 3031
    public List<Scp079Interaction> interactionHistory;

    // Token: 0x04000BD8 RID: 3032
    private RateLimit _interactRateLimit;

    // Token: 0x04000BD9 RID: 3033
    private RateLimit _cameraSyncRateLimit;

    // Token: 0x04000BDA RID: 3034
    public string currentZone;

    // Token: 0x04000BDB RID: 3035
    public string currentRoom;

    // Token: 0x04000BDC RID: 3036
    public Camera079[] nearestCameras;

    // Token: 0x04000BDD RID: 3037
    public List<Scp079Interactable> nearbyInteractables = new List<Scp079Interactable>();

    // Token: 0x04000BDE RID: 3038
    private List<Scp079Interaction> interactions = new List<Scp079Interaction>();

    // Token: 0x04000BDF RID: 3039
    private float ucpTimer;

    // Token: 0x0200018F RID: 399
    [Serializable]
    public class Level079
    {
        // Token: 0x04000BE0 RID: 3040
        public string label;

        // Token: 0x04000BE1 RID: 3041
        public int unlockExp;

        // Token: 0x04000BE2 RID: 3042
        [Space]
        public float manaPerSecond;

        // Token: 0x04000BE3 RID: 3043
        public float maxMana;
    }

    // Token: 0x02000190 RID: 400
    [Serializable]
    public class Ability079
    {
        // Token: 0x04000BE4 RID: 3044
        public string label;

        // Token: 0x04000BE5 RID: 3045
        [FormerlySerializedAs("manaCost")]
        public float mana;

        // Token: 0x04000BE6 RID: 3046
        public int requiredAccessTier = 1;
    }
}
