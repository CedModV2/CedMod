// Decompiled with JetBrains decompiler
// Type: SpeechGen
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

public class SpeechGen : MonoBehaviour
{
  public string allPhones = "a sz ą b c ć d e ę f g h i j k l ł m n ń o ó p r s ś t u w y z ź ż";
  public AudioClip dictionary;
  private string[] phones;
  public string testText;
  private AudioSource source;

  private void Awake()
  {
    this.source = this.GetComponent<AudioSource>();
    this.phones = this.allPhones.Split(' ');
  }

  private void Update()
  {
    if (!Input.GetKeyDown(KeyCode.Home))
      return;
    this.StartCoroutine(this.Synth());
  }

  private IEnumerator Synth()
  {
    string[] strArray = this.testText.Split(' ');
    for (int index1 = 0; index1 < strArray.Length; ++index1)
    {
      string str = strArray[index1];
      for (int index2 = 0; index2 < str.Length; ++index2)
      {
        char ch = str[index2];
        for (int index = 0; index < this.phones.Length; ++index)
        {
          if (ch.ToString() == this.phones[index])
          {
            this.source.Play();
            this.source.time = (float) index;
            Debug.Log((object) ch);
          }
        }
        yield return (object) new WaitForSeconds(0.2f);
      }
      str = (string) null;
      this.source.Stop();
      yield return (object) new WaitForSeconds(0.2f);
    }
    strArray = (string[]) null;
    yield return (object) new WaitForSeconds(0.2f);
  }
}
