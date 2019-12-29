// Decompiled with JetBrains decompiler
// Type: ClipLanguageReplacer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ClipLanguageReplacer : MonoBehaviour
{
  [SerializeField]
  public AudioClip englishVersion;
  private string file;

  private IEnumerator Start()
  {
    ClipLanguageReplacer languageReplacer = this;
    AudioSource asource = languageReplacer.GetComponent<AudioSource>();
    string path = (TranslationReader.path + "/Custom Audio/" + asource.clip.name + ".ogg").Replace('\\', '/');
    if (File.Exists(path))
    {
      if (asource.playOnAwake)
        asource.Stop();
      string clipname = asource.clip.name;
      using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path.StartsWith("/") ? "file://" : "file:///" + path, Misc.GetAudioType(path)))
      {
        Debug.Log((object) "Audio Downloading");
        yield return (object) www.SendWebRequest();
        Debug.Log((object) ("Audio Downloaded: " + path));
        asource.clip = DownloadHandlerAudioClip.GetContent(www);
      }
      asource.clip.name = clipname;
      if (asource.playOnAwake)
        asource.Play();
      clipname = (string) null;
    }
    else
      asource.clip = languageReplacer.englishVersion;
  }

  private void OnDestroy()
  {
    this.GetComponent<AudioSource>().clip.UnloadAudioData();
  }
}
