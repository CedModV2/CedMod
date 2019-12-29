// Decompiled with JetBrains decompiler
// Type: SoundtrackManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
  public GameObject player;
  public LayerMask mask;
  public SoundtrackManager.Track[] overlayTracks;
  public SoundtrackManager.Track[] mainTracks;
  public int overlayIndex;
  public int mainIndex;
  public bool overlayPlaying;
  private bool seeSomeone;
  private float nooneSawTime;
  public static SoundtrackManager singleton;

  private void Awake()
  {
    SoundtrackManager.singleton = this;
  }

  private void Start()
  {
    Timing.RunCoroutine(this._Start(), Segment.FixedUpdate);
  }

  private void FixedUpdate()
  {
    bool flag = (UnityEngine.Object) AlphaWarheadController.Host != (UnityEngine.Object) null && AlphaWarheadController.Host.inProgress;
    if ((double) this.nooneSawTime > 140.0 && !this.overlayPlaying)
    {
      for (int index = 0; index < this.mainTracks.Length; ++index)
      {
        this.mainTracks[index].playing = index == 3 && !flag;
        this.mainTracks[index].Update(1);
      }
      for (int index = 0; index < this.overlayTracks.Length; ++index)
      {
        this.overlayTracks[index].playing = this.overlayPlaying && index == this.overlayIndex && !flag;
        this.overlayTracks[index].Update(1);
      }
    }
    else
    {
      for (int index = 0; index < this.overlayTracks.Length; ++index)
      {
        this.overlayTracks[index].playing = this.overlayPlaying && index == this.overlayIndex && !flag;
        this.overlayTracks[index].Update(1);
      }
      for (int index = 0; index < this.mainTracks.Length; ++index)
      {
        this.mainTracks[index].playing = !this.overlayPlaying && index == this.mainIndex && !flag;
        this.mainTracks[index].Update(1);
      }
    }
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.player == (UnityEngine.Object) null)
      return;
    if (this.seeSomeone)
      this.nooneSawTime = 0.0f;
    else
      this.nooneSawTime += Time.deltaTime;
  }

  private IEnumerator<float> _Start()
  {
    SoundtrackManager soundtrackManager = this;
    while ((UnityEngine.Object) soundtrackManager.player == (UnityEngine.Object) null)
    {
      soundtrackManager.player = PlayerManager.localPlayer;
      yield return float.NegativeInfinity;
    }
    Transform camera = soundtrackManager.player.GetComponent<Scp049PlayerScript>().plyCam.transform;
    CharacterClassManager ccm = soundtrackManager.player.GetComponent<CharacterClassManager>();
    while ((UnityEngine.Object) soundtrackManager != (UnityEngine.Object) null)
    {
      bool flag = false;
      switch (ccm.Classes.SafeGet(ccm.CurClass).team)
      {
        case Team.SCP:
        case Team.RIP:
        case Team.TUT:
          flag = true;
          soundtrackManager.StopOverlay(0);
          break;
        default:
          using (List<GameObject>.Enumerator enumerator = PlayerManager.players.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              GameObject current = enumerator.Current;
              try
              {
                RaycastHit hitInfo;
                if (Physics.Raycast(new Ray(soundtrackManager.player.transform.position, (current.transform.position - camera.position).normalized), out hitInfo, 20f, (int) soundtrackManager.mask))
                {
                  Transform root = hitInfo.collider.transform.root;
                  if (root.CompareTag("Player"))
                  {
                    if (ccm.Classes.SafeGet(root.GetComponent<CharacterClassManager>().CurClass).team != Team.SCP)
                    {
                      flag = true;
                      break;
                    }
                  }
                }
              }
              catch
              {
              }
            }
            break;
          }
      }
      soundtrackManager.seeSomeone = flag;
      yield return float.NegativeInfinity;
    }
  }

  public void PlayOverlay(int id)
  {
    if (id == this.overlayIndex && this.overlayPlaying)
      return;
    this.overlayPlaying = true;
    this.overlayIndex = id;
    if (!this.overlayTracks[id].restartOnPlay)
      return;
    this.overlayTracks[id].source.Stop();
    this.overlayTracks[id].source.Play();
  }

  public void StopOverlay(int id)
  {
    if (this.overlayIndex != id)
      return;
    this.overlayPlaying = false;
  }

  [Serializable]
  public class Track
  {
    public string name;
    public AudioSource source;
    public bool playing;
    public bool restartOnPlay;
    public float enterFadeDuration;
    public float exitFadeDuration;
    public float maxVolume;

    public void Update(int speed = 1)
    {
      if ((UnityEngine.Object) this.source == (UnityEngine.Object) null)
        return;
      if (this.restartOnPlay && (double) Math.Abs(this.source.volume) < 0.00999999977648258 && this.playing)
      {
        this.source.Stop();
        this.source.Play();
      }
      float num = this.source.volume + (float) (0.0199999995529652 * (double) speed * (this.playing ? 1.0 / (double) this.enterFadeDuration : -1.0 / (double) this.exitFadeDuration)) * this.maxVolume;
      this.source.volume = num;
      this.source.volume = Mathf.Clamp(num, 0.0f, this.maxVolume);
    }
  }
}
