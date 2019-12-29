// Decompiled with JetBrains decompiler
// Type: FootstepSync
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using Mirror;
using Security;
using UnityEngine;

public class FootstepSync : NetworkBehaviour
{
  private readonly RateLimit _landRateLimit = new RateLimit(1, 0.1f, (NetworkConnection) null);
  private AnimationController _animController;
  private CharacterClassManager _ccm;
  private Scp939_VisionController _visionController;
  private FootstepHandler _footstepHandler;
  private PlayerEffectsController _effectsController;
  private bool _hasFootstepHandler;

  public FootstepHandler FootstepHandler
  {
    get
    {
      return this._footstepHandler;
    }
    set
    {
      this._footstepHandler = value;
      this._hasFootstepHandler = (Object) value != (Object) null;
    }
  }

  private void Start()
  {
    this._visionController = this.GetComponent<Scp939_VisionController>();
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._animController = this.GetComponent<AnimationController>();
    this._effectsController = this.GetComponent<PlayerEffectsController>();
  }

  public void PlayFootstepSound(float volume, bool running, bool overridemute = false)
  {
    if (this._animController.sneaking && !overridemute || !this._animController.sprinting && (this._ccm.CurClass == RoleType.Scp93953 || this._ccm.CurClass == RoleType.Scp93989))
      return;
    SinkHole effect = this._effectsController.GetEffect<SinkHole>("SinkHole");
    if (effect != null && effect.Enabled)
      volume = 1f;
    this.Scp939Noise(running || this._ccm.Classes.SafeGet(this._ccm.CurClass).team == Team.SCP ? this._animController.runSource : this._animController.walkSource, volume);
    if (!this._hasFootstepHandler)
      return;
    this.FootstepHandler.CanExecuteFootstepSound();
  }

  public void PlayLandingFootstep(bool overridemute)
  {
    if (!this._landRateLimit.CanExecute(true))
      return;
    this.PlayFootstepSound(1f, true, overridemute);
  }

  [ClientRpc]
  public void RpcPlayLandingFootstep(bool overridemute)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(overridemute);
    this.SendRPCInternal(typeof (FootstepSync), nameof (RpcPlayLandingFootstep), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void Scp939Noise(AudioSource audioSource, float volume)
  {
    if (!NetworkServer.active)
      return;
    this._visionController.MakeNoise(audioSource.maxDistance * (volume * 0.7f));
  }

  public void SetLoudness(Team team, bool is939)
  {
    switch (team)
    {
      case Team.SCP:
        if (is939)
          break;
        goto case Team.CHI;
      case Team.CHI:
        this._animController.runSource.maxDistance = 50f;
        this._animController.walkSource.maxDistance = 50f;
        return;
      case Team.RSC:
      case Team.CDP:
        this._animController.runSource.maxDistance = 20f;
        this._animController.walkSource.maxDistance = 10f;
        return;
    }
    this._animController.runSource.maxDistance = 30f;
    this._animController.walkSource.maxDistance = 15f;
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcPlayLandingFootstep(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlayLandingFootstep called on server.");
    else
      ((FootstepSync) obj).CallRpcPlayLandingFootstep(reader.ReadBoolean());
  }

  public void CallRpcPlayLandingFootstep(bool overridemute)
  {
    this.PlayLandingFootstep(overridemute);
  }

  static FootstepSync()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (FootstepSync), "RpcPlayLandingFootstep", new NetworkBehaviour.CmdDelegate(FootstepSync.InvokeRpcRpcPlayLandingFootstep));
  }
}
