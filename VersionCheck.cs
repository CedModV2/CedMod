// Decompiled with JetBrains decompiler
// Type: VersionCheck
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class VersionCheck : NetworkBehaviour
{
  [SyncVar]
  public string serverVersion = string.Empty;
  private string _clientVersion = string.Empty;
  private CharacterClassManager _ccm;
  private bool _checked;

  private void Start()
  {
    this._clientVersion = CustomNetworkManager.CompatibleVersions[0];
    this._ccm = this.GetComponent<CharacterClassManager>();
    if (!NetworkServer.active)
      return;
    this.NetworkserverVersion = this._clientVersion;
  }

  private void Update()
  {
    if (this._checked || !this._ccm.IsHost || string.IsNullOrEmpty(this.serverVersion))
      return;
    this._checked = true;
    if (this.serverVersion == this._clientVersion)
      return;
    CustomNetworkManager objectOfType = Object.FindObjectOfType<CustomNetworkManager>();
    objectOfType.StopClient();
    objectOfType.ShowLog(16, this._clientVersion, this.serverVersion, "");
  }

  private void MirrorProcessed()
  {
  }

  public string NetworkserverVersion
  {
    get
    {
      return this.serverVersion;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.serverVersion, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this.serverVersion);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this.serverVersion);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkserverVersion = reader.ReadString();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworkserverVersion = reader.ReadString();
    }
  }
}
