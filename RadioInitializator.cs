// Decompiled with JetBrains decompiler
// Type: RadioInitializator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.MirrorIgnorance;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RadioInitializator : NetworkBehaviour
{
  public float multipl = 3f;
  public GameObject prefab;
  public Radio radio;
  public MirrorIgnorancePlayer hlapiPlayer;
  private DissonanceUserSetup _dissonanceSetup;
  public AnimationCurve noiseOverLoudness;
  public float curAmplitude;

  private void Start()
  {
    this._dissonanceSetup = this.GetComponentInChildren<DissonanceUserSetup>();
  }

  private void LateUpdate()
  {
    if (!this.isLocalPlayer)
      return;
    try
    {
      foreach (GameObject player in PlayerManager.players)
      {
        try
        {
          if (!((UnityEngine.Object) player == (UnityEngine.Object) this.gameObject))
          {
            if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
            {
              RadioInitializator component1 = player.GetComponent<RadioInitializator>();
              if (!((UnityEngine.Object) component1 == (UnityEngine.Object) null))
              {
                if (!((UnityEngine.Object) component1.radio == (UnityEngine.Object) null))
                {
                  component1.radio.ApplySpatialization();
                  if (!((UnityEngine.Object) component1.radio.playerSource == (UnityEngine.Object) null))
                  {
                    if (!((UnityEngine.Object) component1.hlapiPlayer == (UnityEngine.Object) null))
                    {
                      string playerId = component1.hlapiPlayer.PlayerId;
                      VoicePlayback component2 = component1.radio.playerSource.GetComponent<VoicePlayback>();
                      if (!((UnityEngine.Object) component2 == (UnityEngine.Object) null))
                      {
                        int num = (double) Math.Abs(component1.radio.playerSource.spatialBlend) >= 0.00999999977648258 || component2.Priority == ChannelPriority.None ? 0 : (component1.radio.ShouldBeVisible(this.gameObject) || component1._dissonanceSetup.IntercomAsHuman || (component1._dissonanceSetup.SpectatorChat || component1._dissonanceSetup.SCPChat) || !Radio.roundStarted ? 1 : (Radio.roundEnded ? 1 : 0));
                        this.curAmplitude = component2.Amplitude * this.multipl;
                        if (NetworkServer.active)
                          player.GetComponent<Scp939_VisionController>().MakeNoise(this.noiseOverLoudness.Evaluate(this.curAmplitude));
                      }
                    }
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Debug.LogException(ex);
        }
      }
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
    }
  }

  private void OnDestroy()
  {
  }

  private void MirrorProcessed()
  {
  }

  private class VoiceIndicator
  {
    public GameObject indicator;
    public string id;

    public VoiceIndicator(GameObject indicator, string id)
    {
      this.indicator = indicator;
      this.id = id;
    }
  }

  private class VoiceIndicatorManager
  {
    private List<RadioInitializator.VoiceIndicator> voices = new List<RadioInitializator.VoiceIndicator>();

    public bool ContainsId(string id)
    {
      if (string.IsNullOrEmpty(id))
        return false;
      foreach (RadioInitializator.VoiceIndicator voice in this.voices)
      {
        if (voice != null && voice.id != null && voice.id == id)
          return true;
      }
      return false;
    }

    public RadioInitializator.VoiceIndicator GetFromId(string id)
    {
      if (string.IsNullOrEmpty(id))
        return (RadioInitializator.VoiceIndicator) null;
      foreach (RadioInitializator.VoiceIndicator voice in this.voices)
      {
        if (voice != null && voice.id != null && voice.id == id)
          return voice;
      }
      return (RadioInitializator.VoiceIndicator) null;
    }

    public void RemoveId(string id)
    {
      if (string.IsNullOrEmpty(id))
        return;
      foreach (RadioInitializator.VoiceIndicator voice in this.voices)
      {
        if (voice.id == id)
        {
          this.Remove(voice);
          break;
        }
      }
    }

    public void Add(RadioInitializator.VoiceIndicator voiceObject)
    {
      if (voiceObject == null || this.voices.Contains(voiceObject))
        return;
      this.voices.Add(voiceObject);
    }

    private void Remove(RadioInitializator.VoiceIndicator voiceObject)
    {
      if (voiceObject == null || !this.voices.Contains(voiceObject))
        return;
      if ((UnityEngine.Object) voiceObject.indicator != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) voiceObject.indicator);
      this.voices.Remove(voiceObject);
    }
  }
}
