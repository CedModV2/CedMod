// Decompiled with JetBrains decompiler
// Type: AmmoBox
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using Security;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class AmmoBox : NetworkBehaviour
{
  private Inventory _inv;
  private CharacterClassManager _ccm;
  public AmmoBox.AmmoType[] types;
  [SyncVar]
  public string amount;
  private RateLimit _iawRateLimit;

  public void SetOneAmount(int type, string value)
  {
    string[] strArray = this.amount.Split(':');
    strArray[type] = value;
    this.Networkamount = strArray[0] + ":" + strArray[1] + ":" + strArray[2];
  }

  private void Start()
  {
    this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._inv = this.GetComponent<Inventory>();
    this._ccm = this.GetComponent<CharacterClassManager>();
  }

  public void SetAmmoAmount()
  {
    int[] ammoTypes = this._ccm.Classes.SafeGet(this._ccm.CurClass).ammoTypes;
    this.Networkamount = ammoTypes[0].ToString() + ":" + (object) ammoTypes[1] + ":" + (object) ammoTypes[2];
  }

  public int GetAmmo(int type)
  {
    if (!this.amount.Contains(":"))
      return 0;
    int result;
    if (!int.TryParse(this.amount.Split(':')[Mathf.Clamp(type, 0, 2)], out result))
      MonoBehaviour.print((object) "Parse failed");
    return result;
  }

  [Command]
  public void CmdDrop(int _toDrop, int type)
  {
    if (this.isServer)
    {
      this.CallCmdDrop(_toDrop, type);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(_toDrop);
      writer.WritePackedInt32(type);
      this.SendCommandInternal(typeof (AmmoBox), nameof (CmdDrop), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  public string Networkamount
  {
    get
    {
      return this.amount;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.amount, 1UL);
    }
  }

  protected static void InvokeCmdCmdDrop(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdDrop called on client.");
    else
      ((AmmoBox) obj).CallCmdDrop(reader.ReadPackedInt32(), reader.ReadPackedInt32());
  }

  public void CallCmdDrop(int _toDrop, int type)
  {
    if (!this._iawRateLimit.CanExecute(true))
      return;
    for (ushort index = 0; index < (ushort) 3; ++index)
    {
      if ((int) index == type)
      {
        _toDrop = Mathf.Clamp(_toDrop, 0, this.GetAmmo((int) index));
        if (_toDrop >= 15)
        {
          string[] strArray = this.amount.Split(':');
          strArray[(int) index] = (this.GetAmmo((int) index) - _toDrop).ToString();
          this._inv.SetPickup(this.types[(int) index].inventoryID, (float) _toDrop, this.transform.position, this._inv.camera.transform.rotation, 0, 0, 0);
          this.Networkamount = strArray[0] + ":" + strArray[1] + ":" + strArray[2];
        }
      }
    }
  }

  static AmmoBox()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (AmmoBox), "CmdDrop", new NetworkBehaviour.CmdDelegate(AmmoBox.InvokeCmdCmdDrop));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this.amount);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this.amount);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networkamount = reader.ReadString();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.Networkamount = reader.ReadString();
    }
  }

  [Serializable]
  public class AmmoType
  {
    public string label;
    public ItemType inventoryID;
  }
}
