// Decompiled with JetBrains decompiler
// Type: CustomBroadcastTrigger
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using Dissonance;
using UnityEngine;

public class CustomBroadcastTrigger : VoiceBroadcastTrigger
{
  private DissonanceUserSetup _setup;

  private new void Start()
  {
    base.Start();
    this._setup = this.GetComponent<DissonanceUserSetup>();
  }

  protected override bool IsUserActivated()
  {
    if (this.InputName == "Voice Chat")
      return Input.GetKey(NewInput.GetKey("Voice Chat"));
    return (this.InputName == "Alt Voice Chat" || this.RoomName == TriggerType.Proximity.TriggerTypeToString() && this._setup.RadioAsHuman) && Input.GetKey(NewInput.GetKey("Alt Voice Chat"));
  }
}
