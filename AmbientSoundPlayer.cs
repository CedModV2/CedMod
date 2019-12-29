// Decompiled with JetBrains decompiler
// Type: AmbientSoundPlayer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using Security;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundPlayer : NetworkBehaviour
{
  public int minTime = 30;
  public int maxTime = 60;
  private List<AmbientSoundPlayer.AmbientClip> list = new List<AmbientSoundPlayer.AmbientClip>();
  public GameObject audioPrefab;
  public AmbientSoundPlayer.AmbientClip[] clips;
  private RateLimit _ambientSoundRateLimit;

  private void Start()
  {
    this._ambientSoundRateLimit = new RateLimit(4, 3f, (NetworkConnection) null);
    if (!this.isLocalPlayer || !this.isServer)
      return;
    for (int index = 0; index < this.clips.Length; ++index)
      this.clips[index].index = index;
    this.Invoke("GenerateRandom", 10f);
  }

  private void GenerateRandom()
  {
    this.list.Clear();
    foreach (AmbientSoundPlayer.AmbientClip clip in this.clips)
    {
      if (!clip.played)
        this.list.Add(clip);
    }
    int index = this.list[UnityEngine.Random.Range(0, this.list.Count)].index;
    if (!this.clips[index].repeatable)
      this.clips[index].played = true;
    this.RpcPlaySound(index);
    this.Invoke(nameof (GenerateRandom), (float) UnityEngine.Random.Range(this.minTime, this.maxTime));
  }

  [ClientRpc]
  private void RpcPlaySound(int id)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(id);
    this.SendRPCInternal(typeof (AmbientSoundPlayer), nameof (RpcPlaySound), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcPlaySound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlaySound called on server.");
    else
      ((AmbientSoundPlayer) obj).CallRpcPlaySound(reader.ReadPackedInt32());
  }

  public void CallRpcPlaySound(int id)
  {
  }

  static AmbientSoundPlayer()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (AmbientSoundPlayer), "RpcPlaySound", new NetworkBehaviour.CmdDelegate(AmbientSoundPlayer.InvokeRpcRpcPlaySound));
  }

  [Serializable]
  public class AmbientClip
  {
    public bool repeatable = true;
    public bool is3D = true;
    public AudioClip clip;
    public bool played;
    public int index;
  }
}
