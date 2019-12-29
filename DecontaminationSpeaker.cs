// Decompiled with JetBrains decompiler
// Type: DecontaminationSpeaker
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class DecontaminationSpeaker : MonoBehaviour
{
  private static AudioSource source;
  private static bool isGlobal;
  private static DecontaminationSpeaker singleton;
  public Door[] doorsToOpen;
  private Transform lplayer;

  private void Awake()
  {
    DecontaminationSpeaker.singleton = this;
    DecontaminationSpeaker.isGlobal = false;
    DecontaminationSpeaker.source = this.GetComponent<AudioSource>();
  }

  public static void OpenDoors()
  {
    foreach (Door door in DecontaminationSpeaker.singleton.doorsToOpen)
    {
      if ((double) door.curCooldown <= 0.0 && !door.isOpen)
        door.OpenDecontamination();
    }
  }

  private void Start()
  {
    this.lplayer = Object.FindObjectOfType<SpectatorCamera>().cam.transform;
  }

  private void Update()
  {
    if (!DecontaminationSpeaker.source.isPlaying)
      DecontaminationSpeaker.isGlobal = false;
    float y = this.lplayer.position.y;
    int num = DecontaminationSpeaker.isGlobal || (double) y > -100.0 && (double) y < 100.0 ? 1 : 0;
    if (num == 0 && (double) DecontaminationSpeaker.source.volume > 0.850000023841858 && DecontaminationSpeaker.source.isPlaying)
      return;
    DecontaminationSpeaker.source.volume = Mathf.Lerp(DecontaminationSpeaker.source.volume, (float) num, Time.deltaTime * 2f);
  }

  public static void PlaySound(AudioClip clip, bool global)
  {
    DecontaminationSpeaker.isGlobal = global;
    DecontaminationSpeaker.source.PlayOneShot(clip);
    if (!DecontaminationSpeaker.isGlobal)
      return;
    DecontaminationSpeaker.source.volume = 1f;
  }
}
