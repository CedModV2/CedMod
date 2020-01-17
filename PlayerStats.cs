// PlayerStats
using Dissonance.Integrations.MirrorIgnorance;
using GameCore;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
	[Serializable]
	public struct HitInfo : IEquatable<HitInfo>
	{
		public float Amount;

		public int Tool;

		public int Time;

		public string Attacker;

		public int PlyId;

		public HitInfo(float amnt, string attackerName, DamageTypes.DamageType weapon, int attackerId)
		{
			Amount = amnt;
			Tool = DamageTypes.ToIndex(weapon);
			Attacker = attackerName;
			PlyId = attackerId;
			Time = ServerTime.time;
		}

		public GameObject GetPlayerObject()
		{
			foreach (GameObject player in PlayerManager.players)
			{
				if (player.GetComponent<QueryProcessor>().PlayerId == PlyId)
				{
					return player;
				}
			}
			return null;
		}

		public DamageTypes.DamageType GetDamageType()
		{
			return DamageTypes.FromIndex(Tool);
		}

		public string GetDamageName()
		{
			return DamageTypes.FromIndex(Tool).name;
		}

		public bool Equals(HitInfo other)
		{
			if (Math.Abs(Amount - other.Amount) < 0.005f && Tool == other.Tool && Time == other.Time && string.Equals(Attacker, other.Attacker))
			{
				return PlyId == other.PlyId;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			object obj2;
			if ((obj2 = obj) is HitInfo)
			{
				HitInfo other = (HitInfo)obj2;
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((Amount.GetHashCode() * 397) ^ Tool) * 397) ^ Time) * 397) ^ ((Attacker != null) ? Attacker.GetHashCode() : 0)) * 397) ^ PlyId;
		}

		public static bool operator ==(HitInfo left, HitInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HitInfo left, HitInfo right)
		{
			return !left.Equals(right);
		}
	}

	public HitInfo lastHitInfo = new HitInfo(0f, "NONE", DamageTypes.None, 0);

	public Transform[] grenadePoints;

	public CharacterClassManager ccm;

	private UserMainInterface _ui;

	private static DateTime rrTime;

	private static Lift[] _lifts;

	public bool used914;

	private bool _pocketCleanup;

	private bool _allowSPDmg;

	private int _maxHP;

	private float _health;

	private bool _hpDirty;

	[SyncVar]
	public byte syncArtificialHealth;

	public float unsyncedArtificialHealth;

	public float artificialNormalRatio = 0.7f;

	public int maxArtificialHealth = 75;

	private RateLimit _interactRateLimit;

	private float killstreak_time;

	private int killstreak;

	private readonly Scp079Interactable.InteractableType[] _filters = new Scp079Interactable.InteractableType[5]
	{
		Scp079Interactable.InteractableType.Door,
		Scp079Interactable.InteractableType.Light,
		Scp079Interactable.InteractableType.Lockdown,
		Scp079Interactable.InteractableType.Tesla,
		Scp079Interactable.InteractableType.ElevatorUse
	};

	public int maxHP
	{
		get
		{
			return _maxHP;
		}
		set
		{
			_maxHP = value;
		}
	}

	public float health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = value;
			_hpDirty = true;
		}
	}

	public byte NetworksyncArtificialHealth
	{
		get
		{
			return syncArtificialHealth;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref syncArtificialHealth, 1uL);
		}
	}

	public void MakeHpDirty()
	{
		_hpDirty = true;
	}

	private void Start()
	{
		_interactRateLimit = GetComponent<PlayerRateLimitHandler>().RateLimits[0];
		_pocketCleanup = ConfigFile.ServerConfig.GetBool("SCP106_CLEANUP");
		_allowSPDmg = ConfigFile.ServerConfig.GetBool("spawn_protect_allow_dmg", def: true);
		ccm = GetComponent<CharacterClassManager>();
		_ui = UserMainInterface.singleton;
		if (_lifts.Length == 0)
		{
			_lifts = UnityEngine.Object.FindObjectsOfType<Lift>();
		}
		if (NetworkServer.active)
		{
			if (!PlayerPrefsSl.HasKey("LastRoundrestartTime", PlayerPrefsSl.DataType.Int))
			{
				PlayerPrefsSl.Set("LastRoundrestartTime", 5000);
			}
			TimeSpan timeSpan = DateTime.Now - rrTime;
			if (timeSpan.TotalSeconds > 20.0)
			{
				Debug.Log("Restart too long or the server has just started.");
				return;
			}
			int num = (int)timeSpan.TotalMilliseconds;
			PlayerPrefsSl.Set("LastRoundrestartTime", (PlayerPrefsSl.Get("LastRoundrestartTime", 5000) + num) / 2);
		}
	}

	private void Update()
	{
		if (_hpDirty)
		{
			_hpDirty = false;
			if (NetworkServer.active)
			{
				TargetSyncHp(base.connectionToClient, _health);
			}
			foreach (GameObject player in PlayerManager.players)
			{
				CharacterClassManager component = player.GetComponent<CharacterClassManager>();
				if (component.CurClass == RoleType.Spectator && component.IsVerified)
				{
					TargetSyncHp(component.connectionToClient, _health);
				}
			}
		}
	}

	[TargetRpc]
	public void TargetSyncHp(NetworkConnection conn, float hp)
	{
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteSingle(hp);
		SendTargetRPCInternal(conn, typeof(PlayerStats), "TargetSyncHp", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public float GetHealthPercent()
	{
		if (ccm.CurClass < RoleType.Scp173)
		{
			return 0f;
		}
		return Mathf.Clamp01(1f - health / (float)ccm.Classes.SafeGet(ccm.CurClass).maxHP);
	}

	[Command]
	public void CmdSelfDeduct(HitInfo info)
	{
		if (base.isServer)
		{
			CallCmdSelfDeduct(info);
			return;
		}
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(writer, info);
		SendCommandInternal(typeof(PlayerStats), "CmdSelfDeduct", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public bool Explode(bool inElevator)
	{
		bool flag = health > 0f && (inElevator || base.transform.position.y < 900f);
		switch (ccm.CurClass)
		{
			case RoleType.Scp079:
				flag = true;
				break;
			case RoleType.Scp106:
				{
					Scp106PlayerScript component = GetComponent<Scp106PlayerScript>();
					if (component != null)
					{
						component.DeletePortal();
						if (component.goingViaThePortal)
						{
							flag = true;
						}
					}
					break;
				}
		}
		if (flag)
		{
			return HurtPlayer(new HitInfo(-1f, "WORLD", DamageTypes.Nuke, 0), base.gameObject);
		}
		return false;
	}

	[Command]
	public void CmdTesla()
	{
		if (base.isServer)
		{
			CallCmdTesla();
			return;
		}
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(PlayerStats), "CmdTesla", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void SetHPAmount(int hp)
	{
		health = hp;
	}

	public bool HealHPAmount(float hp)
	{
		float num = Mathf.Clamp(hp, 0f, (float)maxHP - health);
		health += num;
		return num > 0f;
	}

	public bool HurtPlayer(HitInfo info, GameObject go)
	{
		bool result = false;
		bool flag = go == null;
		if (info.Amount < 0f)
		{
			if (flag)
			{
				info.Amount = Mathf.Abs(999999f);
			}
			else
			{
				PlayerStats component = go.GetComponent<PlayerStats>();
				info.Amount = ((component != null) ? Mathf.Abs(component.health + (float)(int)component.syncArtificialHealth + 10f) : Mathf.Abs(999999f));
			}
		}
		if (info.Amount > 2.14748365E+09f)
		{
			info.Amount = 2.14748365E+09f;
		}
		if (flag)
		{
			return result;
		}
		PlayerStats component2 = go.GetComponent<PlayerStats>();
		CharacterClassManager component3 = go.GetComponent<CharacterClassManager>();
		if (component2 == null || component3 == null)
		{
			return false;
		}
		if (component3.GodMode)
		{
			return false;
		}
		if (ccm.Classes.SafeGet(ccm.CurClass).team == Team.SCP && ccm.Classes.SafeGet(component3.CurClass).team == Team.SCP && ccm != component3)
		{
			return false;
		}
		if (component3.SpawnProtected && !_allowSPDmg)
		{
			return false;
		}
		if (base.isLocalPlayer && info.PlyId != go.GetComponent<QueryProcessor>().PlayerId)
		{
			RoundSummary.Damages += ((component2.health < info.Amount) ? component2.health : info.Amount);
		}
		if (lastHitInfo.Attacker == "ARTIFICIALDEGEN")
		{
			component2.unsyncedArtificialHealth -= info.Amount;
			if (component2.unsyncedArtificialHealth < 0f)
			{
				component2.unsyncedArtificialHealth = 0f;
			}
		}
		else
		{
			if (component2.unsyncedArtificialHealth > 0f)
			{
				float num = info.Amount * artificialNormalRatio;
				float num2 = info.Amount - num;
				component2.unsyncedArtificialHealth -= num;
				if (component2.unsyncedArtificialHealth < 0f)
				{
					num2 += Mathf.Abs(component2.unsyncedArtificialHealth);
					component2.unsyncedArtificialHealth = 0f;
				}
				component2.health -= num2;
				if (component2.health > 0f && component2.health - num <= 0f)
				{
					TargetAchieve(base.connectionToClient, "didntevenfeelthat");
				}
			}
			else
			{
				component2.health -= info.Amount;
			}
			if (component2.health < 0f)
			{
				component2.health = 0f;
			}
			component2.lastHitInfo = info;
		}
		if (component2.health < 1f && component3.CurClass != RoleType.Spectator)
		{
			foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
			{
				Scp079Interactable.ZoneAndRoom otherRoom = go.GetComponent<Scp079PlayerScript>().GetOtherRoom();
				bool flag2 = false;
				foreach (Scp079Interaction item in instance.ReturnRecentHistory(12f, _filters))
				{
					foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in item.interactable.currentZonesAndRooms)
					{
						if (currentZonesAndRoom.currentZone == otherRoom.currentZone && currentZonesAndRoom.currentRoom == otherRoom.currentRoom)
						{
							flag2 = true;
						}
					}
				}
				if (flag2)
				{
					instance.RpcGainExp(ExpGainType.KillAssist, component3.CurClass);
				}
			}
			if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60)
			{
				TargetAchieve(component3.connectionToClient, "wowreally");
			}
			if (base.isLocalPlayer && info.PlyId != go.GetComponent<QueryProcessor>().PlayerId)
			{
				RoundSummary.Kills++;
			}
			result = true;
			Scp049PlayerScript[] array = UnityEngine.Object.FindObjectsOfType<Scp049PlayerScript>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RpcSetDeathTime(go);
			}
			if (component3.CurClass == RoleType.Scp096 && go.GetComponent<Scp096PlayerScript>().enraged == Scp096PlayerScript.RageState.Panic)
			{
				TargetAchieve(component3.connectionToClient, "unvoluntaryragequit");
			}
			else if (info.GetDamageType() == DamageTypes.Pocket)
			{
				TargetAchieve(component3.connectionToClient, "newb");
			}
			else if (info.GetDamageType() == DamageTypes.Scp173)
			{
				TargetAchieve(component3.connectionToClient, "firsttime");
			}
			else if (info.GetDamageType() == DamageTypes.Grenade && info.PlyId == go.GetComponent<QueryProcessor>().PlayerId)
			{
				TargetAchieve(component3.connectionToClient, "iwanttobearocket");
			}
			else if (info.GetDamageType().isWeapon)
			{
				Inventory component4 = component3.GetComponent<Inventory>();
				if (component3.CurClass == RoleType.Scientist)
				{
					Item itemByID = component4.GetItemByID(component4.curItem);
					if (itemByID != null && itemByID.itemCategory == ItemCategory.Keycard && GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD)
					{
						TargetAchieve(base.connectionToClient, "betrayal");
					}
				}
				if (Time.realtimeSinceStartup - killstreak_time > 30f || killstreak == 0)
				{
					killstreak = 0;
					killstreak_time = Time.realtimeSinceStartup;
				}
				if (GetComponent<WeaponManager>().GetShootPermission(component3, forceFriendlyFire: true))
				{
					killstreak++;
				}
				if (killstreak > 5)
				{
					TargetAchieve(base.connectionToClient, "pewpew");
				}
				if ((ccm.Classes.SafeGet(ccm.CurClass).team == Team.MTF || ccm.Classes.SafeGet(ccm.CurClass).team == Team.RSC) && component3.CurClass == RoleType.ClassD)
				{
					TargetStats(base.connectionToClient, "dboys_killed", "justresources", 50);
				}
				if (ccm.Classes.SafeGet(ccm.CurClass).team == Team.RSC && ccm.Classes.SafeGet(component3.CurClass).team == Team.SCP)
				{
					TargetAchieve(base.connectionToClient, "timetodoitmyself");
				}
			}
			else if (ccm.Classes.SafeGet(ccm.CurClass).team == Team.SCP && go.GetComponent<MicroHID>().CurrentHidState != 0)
			{
				TargetAchieve(base.connectionToClient, "illpassthanks");
			}
			ServerLogs.AddLog(ServerLogs.Modules.ClassChange, go.GetComponent<NicknameSync>().MyNick + " (" + go.GetComponent<CharacterClassManager>().UserId + ") killed by " + info.Attacker + " using " + info.GetDamageName() + ".", ServerLogs.ServerLogType.KillLog);
			if (!_pocketCleanup || info.GetDamageType() != DamageTypes.Pocket)
			{
				go.GetComponent<Inventory>().ServerDropAll();
				if (component3.Classes.CheckBounds(component3.CurClass) && info.GetDamageType() != DamageTypes.RagdollLess)
				{
					GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, (int)component3.CurClass, info, component3.Classes.SafeGet(component3.CurClass).team != Team.SCP, go.GetComponent<MirrorIgnorancePlayer>().PlayerId, go.GetComponent<NicknameSync>().MyNick, go.GetComponent<QueryProcessor>().PlayerId);
				}
			}
			else
			{
				go.GetComponent<Inventory>().Clear();
			}
			component3.NetworkDeathPosition = go.transform.position;
			if (component3.Classes.SafeGet(component3.CurClass).team == Team.SCP)
			{
				if (component3.CurClass == RoleType.Scp0492)
				{
					NineTailedFoxAnnouncer.CheckForZombies(go);
				}
				else
				{
					GameObject x = null;
					foreach (GameObject player in PlayerManager.players)
					{
						if (player.GetComponent<QueryProcessor>().PlayerId == info.PlyId)
						{
							x = player;
						}
					}
					if (x != null)
					{
						NineTailedFoxAnnouncer.AnnounceScpTermination(component3.Classes.SafeGet(component3.CurClass), info, "");
					}
					else
					{
						DamageTypes.DamageType damageType = info.GetDamageType();
						if (damageType == DamageTypes.Tesla)
						{
							NineTailedFoxAnnouncer.AnnounceScpTermination(component3.Classes.SafeGet(component3.CurClass), info, "TESLA");
						}
						else if (damageType == DamageTypes.Nuke)
						{
							NineTailedFoxAnnouncer.AnnounceScpTermination(component3.Classes.SafeGet(component3.CurClass), info, "WARHEAD");
						}
						else if (damageType == DamageTypes.Decont)
						{
							NineTailedFoxAnnouncer.AnnounceScpTermination(component3.Classes.SafeGet(component3.CurClass), info, "DECONTAMINATION");
						}
						else if (component3.CurClass != RoleType.Scp079)
						{
							NineTailedFoxAnnouncer.AnnounceScpTermination(component3.Classes.SafeGet(component3.CurClass), info, "UNKNOWN");
						}
					}
				}
			}
			component2.SetHPAmount(100);
			component3.SetClassID(RoleType.Spectator);
		}
		else
		{
			Vector3 pos = Vector3.zero;
			float num3 = 40f;
			if (info.GetDamageType().isWeapon)
			{
				GameObject playerOfID = GetPlayerOfID(info.PlyId);
				if (playerOfID != null)
				{
					pos = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
					num3 = 100f;
				}
			}
			else if (info.GetDamageType() == DamageTypes.Pocket)
			{
				PlyMovementSync component5 = ccm.GetComponent<PlyMovementSync>();
				if (component5.RealModelPosition.y > -1900f)
				{
					component5.OverridePosition(Vector3.down * 1998.5f, 0f, forceGround: true);
				}
			}
			TargetOofEffect(go.GetComponent<NetworkIdentity>().connectionToClient, pos, Mathf.Clamp01(info.Amount / num3));
		}
		return result;
	}

	[TargetRpc]
	public void TargetAchieve(NetworkConnection conn, string key)
	{
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(key);
		SendTargetRPCInternal(conn, typeof(PlayerStats), "TargetAchieve", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetStats(NetworkConnection conn, string key, string targetAchievement, int maxValue)
	{
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(key);
		writer.WriteString(targetAchievement);
		writer.WritePackedInt32(maxValue);
		SendTargetRPCInternal(conn, typeof(PlayerStats), "TargetStats", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private GameObject GetPlayerOfID(int id)
	{
		return PlayerManager.players.FirstOrDefault((GameObject ply) => ply.GetComponent<QueryProcessor>().PlayerId == id);
	}

	[TargetRpc]
	private void TargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
	{
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(pos);
		writer.WriteSingle(overall);
		SendTargetRPCInternal(conn, typeof(PlayerStats), "TargetOofEffect", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcRoundrestart(float timeOffset)
	{
		NetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteSingle(timeOffset);
		SendRPCInternal(typeof(PlayerStats), "RpcRoundrestart", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void Roundrestart()
	{
		MirrorIgnorancePlayer[] array = UnityEngine.Object.FindObjectsOfType<MirrorIgnorancePlayer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnDisable();
		}
		RpcRoundrestart(PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000);
		Invoke("ChangeLevel", 2.5f);
	}

	private void ChangeLevel()
	{
		if (NetworkServer.active)
		{
			if (ServerStatic.StopNextRound)
			{
				ServerConsole.AddLog("Stopping the server (StopNextRound command was used)...");
				Application.Quit();
			}
			else
			{
				GC.Collect();
				rrTime = DateTime.Now;
				NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
			}
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
	}

	public string HealthToString()
	{
		float num = Mathf.Round(health);
		double num2 = (double)num / (double)maxHP * 100.0;
		return num.ToString(CultureInfo.InvariantCulture) + "/" + maxHP + " (" + num2.ToString("####0.##", CultureInfo.InvariantCulture) + "%)";
	}

	static PlayerStats()
	{
		_lifts = new Lift[0];
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), "CmdSelfDeduct", InvokeCmdCmdSelfDeduct);
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), "CmdTesla", InvokeCmdCmdTesla);
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), "RpcRoundrestart", InvokeRpcRpcRoundrestart);
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), "TargetSyncHp", InvokeRpcTargetSyncHp);
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), "TargetAchieve", InvokeRpcTargetAchieve);
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), "TargetStats", InvokeRpcTargetStats);
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), "TargetOofEffect", InvokeRpcTargetOofEffect);
	}

	private void MirrorProcessed()
	{
	}

	protected static void InvokeCmdCmdSelfDeduct(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSelfDeduct called on client.");
		}
		else
		{
			((PlayerStats)obj).CallCmdSelfDeduct(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	protected static void InvokeCmdCmdTesla(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTesla called on client.");
		}
		else
		{
			((PlayerStats)obj).CallCmdTesla();
		}
	}

	public void CallCmdSelfDeduct(HitInfo info)
	{
		if (_interactRateLimit.CanExecute())
		{
			HurtPlayer(info, base.gameObject);
		}
	}

	public void CallCmdTesla()
	{
		if (_interactRateLimit.CanExecute())
		{
			HurtPlayer(new HitInfo(UnityEngine.Random.Range(100, 200), GetComponent<MirrorIgnorancePlayer>().PlayerId, DamageTypes.Tesla, 0), base.gameObject);
		}
	}

	protected static void InvokeRpcRpcRoundrestart(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRoundrestart called on server.");
		}
		else
		{
			((PlayerStats)obj).CallRpcRoundrestart(reader.ReadSingle());
		}
	}

	protected static void InvokeRpcTargetSyncHp(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSyncHp called on server.");
		}
		else
		{
			((PlayerStats)obj).CallTargetSyncHp(ClientScene.readyConnection, reader.ReadSingle());
		}
	}

	protected static void InvokeRpcTargetAchieve(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetAchieve called on server.");
		}
		else
		{
			((PlayerStats)obj).CallTargetAchieve(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetStats(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetStats called on server.");
		}
		else
		{
			((PlayerStats)obj).CallTargetStats(ClientScene.readyConnection, reader.ReadString(), reader.ReadString(), reader.ReadPackedInt32());
		}
	}

	protected static void InvokeRpcTargetOofEffect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOofEffect called on server.");
		}
		else
		{
			((PlayerStats)obj).CallTargetOofEffect(ClientScene.readyConnection, reader.ReadVector3(), reader.ReadSingle());
		}
	}

	public void CallRpcRoundrestart(float timeOffset)
	{
		if (!base.isServer)
		{
			UnityEngine.Object.FindObjectOfType<CustomNetworkManager>().reconnectTime = timeOffset;
			Invoke("ChangeLevel", 0.5f);
		}
	}

	public void CallTargetSyncHp(NetworkConnection conn, float hp)
	{
		_health = hp;
	}

	public void CallTargetAchieve(NetworkConnection conn, string key)
	{
	}

	public void CallTargetStats(NetworkConnection conn, string key, string targetAchievement, int maxValue)
	{
	}

	public void CallTargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			NetworkWriterExtensions.WriteByte(writer, syncArtificialHealth);
			return true;
		}
		writer.WritePackedUInt64(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			NetworkWriterExtensions.WriteByte(writer, syncArtificialHealth);
			result = true;
		}
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			byte b2 = NetworksyncArtificialHealth = NetworkReaderExtensions.ReadByte(reader);
			return;
		}
		long num = (long)reader.ReadPackedUInt64();
		if ((num & 1L) != 0L)
		{
			byte b4 = NetworksyncArtificialHealth = NetworkReaderExtensions.ReadByte(reader);
		}
	}
}
