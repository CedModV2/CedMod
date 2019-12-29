// Decompiled with JetBrains decompiler
// Type: NineTailedFoxAnnouncer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using RemoteAdmin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NineTailedFoxAnnouncer : MonoBehaviour
{
  public static List<NineTailedFoxAnnouncer.ScpDeath> scpDeaths = new List<NineTailedFoxAnnouncer.ScpDeath>();
  public Queue<NineTailedFoxAnnouncer.VoiceLine> queue = new Queue<NineTailedFoxAnnouncer.VoiceLine>();
  private List<string> newWords = new List<string>();
  private readonly List<NineTailedFoxAnnouncer.VoiceLine> newLines = new List<NineTailedFoxAnnouncer.VoiceLine>();
  public bool Free = true;
  public NineTailedFoxAnnouncer.VoiceLine[] voiceLines;
  public AudioClip[] backgroundLines;
  public AudioClip suffixPluralStandard;
  public AudioClip suffixPluralException;
  public AudioClip suffixPastStandard;
  public AudioClip suffixPastException;
  public AudioClip suffixContinuous;
  public AudioSource speakerSource;
  public AudioSource backgroundSource;
  public static NineTailedFoxAnnouncer singleton;
  private float scpListTimer;

  public void AnnounceNtfEntrance(int _scpsLeft, int _mtfNumber, char _mtfLetter)
  {
    string empty = string.Empty;
    string[] strArray = new string[2]
    {
      _mtfNumber.ToString("00")[0].ToString(),
      _mtfNumber.ToString("00")[1].ToString()
    };
    string tts;
    if (ClutterSpawner.IsHolidayActive(Holidays.Christmas))
      tts = empty + "XMAS_EPSILON11 " + "NATO_" + _mtfLetter.ToString() + " " + strArray[0] + strArray[1] + " " + "XMAS_HASENTERED " + (object) _scpsLeft + " XMAS_SCPSUBJECTS";
    else
      tts = empty + "MTFUNIT EPSILON 11 DESIGNATED " + "NATO_" + _mtfLetter.ToString() + " " + strArray[0] + strArray[1] + " " + "HASENTERED ALLREMAINING " + (_scpsLeft <= 0 ? "NOSCPSLEFT" : "AWAITINGRECONTAINMENT " + (object) _scpsLeft + (_scpsLeft == 1 ? (object) " SCPSUBJECT" : (object) " SCPSUBJECTS"));
    float num = (double) AlphaWarheadController.Host.timeToDetonation <= 0.0 ? 2.5f : 1f;
    this.ServerOnlyAddGlitchyPhrase(tts, UnityEngine.Random.Range(0.08f, 0.1f) * num, UnityEngine.Random.Range(0.07f, 0.09f) * num);
  }

  public string ConvertNumber(int num)
  {
    if (num <= 0)
      return " 0 ";
    ushort num1 = 0;
    byte num2 = 0;
    byte num3 = 0;
    for (; (double) num / 1000.0 >= 1.0; num -= 1000)
      ++num1;
    for (; (double) num / 100.0 >= 1.0; num -= 100)
      ++num2;
    if (num >= 20)
    {
      for (; (double) num / 10.0 >= 1.0; num -= 10)
        ++num3;
    }
    string str = string.Empty;
    if (num1 > (ushort) 0)
      str = str + this.ConvertNumber((int) num1) + " thousand ";
    if (num2 > (byte) 0)
      str = str + (object) num2 + " hundred ";
    if ((int) num2 + (int) num1 > 0 && (num > 0 || num3 > (byte) 0))
      str += " and ";
    if (num3 > (byte) 0)
      str = str + (object) num3 + "0 ";
    if (num > 0)
      str = str + (object) num + " ";
    return str;
  }

  public static void AnnounceScpTermination(Role scp, PlayerStats.HitInfo hit, string groupId)
  {
    NineTailedFoxAnnouncer.singleton.scpListTimer = 0.0f;
    if (!string.IsNullOrEmpty(groupId))
    {
      foreach (NineTailedFoxAnnouncer.ScpDeath scpDeath in NineTailedFoxAnnouncer.scpDeaths)
      {
        if (scpDeath.group == groupId)
        {
          scpDeath.scpSubjects.Add(scp);
          return;
        }
      }
    }
    NineTailedFoxAnnouncer.scpDeaths.Add(new NineTailedFoxAnnouncer.ScpDeath()
    {
      scpSubjects = new List<Role>((IEnumerable<Role>) new Role[1]
      {
        scp
      }),
      group = groupId,
      hitInfo = hit
    });
  }

  public static void CheckForZombies(GameObject zombie)
  {
    int num = 0;
    foreach (GameObject player in PlayerManager.players)
    {
      if (!((UnityEngine.Object) player == (UnityEngine.Object) zombie))
      {
        ReferenceHub hub = ReferenceHub.GetHub(player);
        if (hub.characterClassManager.CurClass != RoleType.Scp079 && hub.characterClassManager.Classes.SafeGet(hub.characterClassManager.CurClass).team == Team.SCP)
          ++num;
      }
    }
    if (num > 0 || Generator079.mainGenerator.totalVoltage >= 4 || Generator079.mainGenerator.forcedOvercharge)
      return;
    Generator079.mainGenerator.forcedOvercharge = true;
    Recontainer079.BeginContainment(true);
    NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase("ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE", 0.1f, 0.07f);
  }

  public float GetPitch(string word)
  {
    if (!word.StartsWith("PITCH_", StringComparison.OrdinalIgnoreCase))
      return 1f;
    string str = word.Remove(0, 6);
    if (str.Contains<char>(','))
      return 1f;
    float result;
    return float.TryParse(str, out result) ? ((double) result > 0.0 ? result : 1f) : (!str.Contains<char>('.') || !float.TryParse(str.Replace('.', ','), out result) || (double) result <= 0.0 ? 1f : result);
  }

  private float CalculateDuration(string tts, bool rawNumber = false)
  {
    float num1 = 0.0f;
    float num2 = 1f;
    string str1 = tts;
    char[] chArray = new char[1]{ ' ' };
    foreach (string str2 in str1.Split(chArray))
    {
      if (str2.StartsWith("JAM", StringComparison.OrdinalIgnoreCase))
      {
        try
        {
          if (float.TryParse(str2.Substring(4, 3), out float _))
          {
            int result;
            if (int.TryParse(str2.Substring(8, 1), out result))
              num1 += 0.13f * (float) result;
          }
        }
        catch
        {
        }
      }
      else if ((double) Math.Abs(this.GetPitch(str2) - 1f) > 0.00999999977648258)
      {
        num2 = this.GetPitch(str2);
      }
      else
      {
        int result;
        if (int.TryParse(str2, out result) && !rawNumber)
        {
          num1 += this.CalculateDuration(this.ConvertNumber(result), true);
        }
        else
        {
          bool flag = false;
          foreach (NineTailedFoxAnnouncer.VoiceLine voiceLine in this.voiceLines)
          {
            if (string.Equals(str2, voiceLine.apiName, StringComparison.OrdinalIgnoreCase))
            {
              flag = true;
              num1 += voiceLine.length / num2;
            }
          }
          if (!flag && str2.Length > 3)
          {
            for (byte index = 1; index <= (byte) 3; ++index)
            {
              foreach (NineTailedFoxAnnouncer.VoiceLine voiceLine in this.voiceLines)
              {
                if (string.Equals(str2.Remove(str2.Length - (int) index), voiceLine.apiName, StringComparison.OrdinalIgnoreCase))
                  num1 += voiceLine.length / num2;
              }
            }
          }
        }
      }
    }
    return num1;
  }

  public void ServerOnlyAddGlitchyPhrase(string tts, float glitchChance, float jamChance)
  {
    string[] strArray = tts.Split(' ');
    this.newWords.Clear();
    this.newWords.EnsureCapacity<string>(strArray.Length);
    for (int index = 0; index < strArray.Length; ++index)
    {
      this.newWords.Add(strArray[index]);
      if (index < strArray.Length - 1)
      {
        if ((double) UnityEngine.Random.value < (double) glitchChance)
          this.newWords.Add(".G" + (object) UnityEngine.Random.Range(1, 7));
        if ((double) UnityEngine.Random.value < (double) jamChance)
          this.newWords.Add("JAM_" + UnityEngine.Random.Range(0, 70).ToString("000") + "_" + (object) UnityEngine.Random.Range(2, 6));
      }
    }
    tts = "";
    foreach (string newWord in this.newWords)
      tts = tts + newWord + " ";
    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(tts, false, true);
  }

  public void AddPhraseToQueue(string tts, bool generateNoise, bool rawNumber = false, bool makeHold = false)
  {
    string[] strArray = tts.Split(' ');
    if (!rawNumber)
    {
      float duration = this.CalculateDuration(tts, false);
      int index1 = 0;
      for (int index2 = 0; index2 < this.backgroundLines.Length - 1; ++index2)
      {
        if ((double) index2 < (double) duration)
          index1 = index2 + 1;
      }
      if (generateNoise)
        this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
        {
          apiName = "BG_BACKGROUND",
          clip = this.backgroundLines[index1],
          length = 2.5f
        });
    }
    float num = 1f;
    foreach (string str in strArray)
    {
      if (str.StartsWith("PITCH_", StringComparison.OrdinalIgnoreCase))
        this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
        {
          apiName = str.ToUpper()
        });
      else if (str.StartsWith("JAM", StringComparison.OrdinalIgnoreCase))
      {
        this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
        {
          apiName = str.ToUpper()
        });
      }
      else
      {
        float result;
        if (!rawNumber && float.TryParse(str, out result))
        {
          this.AddPhraseToQueue(this.ConvertNumber((int) result), false, true, false);
        }
        else
        {
          bool flag = false;
          foreach (NineTailedFoxAnnouncer.VoiceLine voiceLine in this.voiceLines)
          {
            if (string.Equals(str, voiceLine.apiName, StringComparison.OrdinalIgnoreCase))
            {
              this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
              {
                apiName = voiceLine.apiName,
                clip = voiceLine.clip,
                length = voiceLine.length / num
              });
              flag = true;
            }
          }
          if (!flag)
          {
            NineTailedFoxAnnouncer.VoiceLine voiceLine1 = (NineTailedFoxAnnouncer.VoiceLine) null;
            if (str.Length > 3)
            {
              for (byte index = 1; index <= (byte) 4; ++index)
              {
                if (str.Length > (int) index)
                {
                  foreach (NineTailedFoxAnnouncer.VoiceLine voiceLine2 in this.voiceLines)
                  {
                    if ((string.Equals(str.Remove(str.Length - (int) index), voiceLine2.apiName, StringComparison.OrdinalIgnoreCase) || str.EndsWith("ING", StringComparison.OrdinalIgnoreCase) && string.Equals(str.Remove(str.Length - (int) index) + "E", voiceLine2.apiName, StringComparison.OrdinalIgnoreCase)) && voiceLine1 == null)
                      voiceLine1 = new NineTailedFoxAnnouncer.VoiceLine()
                      {
                        apiName = voiceLine2.apiName,
                        clip = voiceLine2.clip,
                        length = voiceLine2.length / num
                      };
                  }
                }
              }
            }
            if (voiceLine1 != null)
            {
              AudioClip audioClip = str.EndsWith("TED", StringComparison.OrdinalIgnoreCase) || str.EndsWith("DED", StringComparison.OrdinalIgnoreCase) ? this.suffixPastException : (!str.EndsWith("D", StringComparison.OrdinalIgnoreCase) ? (!str.EndsWith("ING", StringComparison.OrdinalIgnoreCase) ? (voiceLine1.apiName.EndsWith("S") || voiceLine1.apiName.EndsWith("SH") || (voiceLine1.apiName.EndsWith("CH") || voiceLine1.apiName.EndsWith("X")) || voiceLine1.apiName.EndsWith("Z") ? this.suffixPluralException : this.suffixPluralStandard) : this.suffixContinuous) : this.suffixPastStandard);
              this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
              {
                apiName = voiceLine1.apiName,
                clip = voiceLine1.clip,
                length = (voiceLine1.length - (str.EndsWith("ING", StringComparison.OrdinalIgnoreCase) ? 0.1f : 0.06f)) / num
              });
              this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
              {
                apiName = "SUFFIX_" + audioClip.name,
                clip = audioClip,
                length = audioClip.length / num
              });
            }
          }
        }
      }
    }
    this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
    {
      apiName = "PITCH_1"
    });
    if (rawNumber)
      return;
    for (byte index = 0; (int) index < (makeHold ? 3 : 1); ++index)
      this.queue.Enqueue(new NineTailedFoxAnnouncer.VoiceLine()
      {
        apiName = "END_OF_MESSAGE"
      });
  }

  private IEnumerator Start()
  {
    NineTailedFoxAnnouncer tailedFoxAnnouncer = this;
    NineTailedFoxAnnouncer.scpDeaths.Clear();
    NineTailedFoxAnnouncer.singleton = tailedFoxAnnouncer;
    float speed = 1f;
    float jammed = 0.0f;
    int jamSize = 0;
    WaitForEndOfFrame wait = new WaitForEndOfFrame();
    while ((UnityEngine.Object) tailedFoxAnnouncer != (UnityEngine.Object) null)
    {
      if (tailedFoxAnnouncer.queue.Count == 0)
      {
        speed = 1f;
        yield return (object) wait;
        tailedFoxAnnouncer.Free = true;
      }
      else
      {
        tailedFoxAnnouncer.Free = false;
        NineTailedFoxAnnouncer.VoiceLine line = tailedFoxAnnouncer.queue.Dequeue();
        if (line.apiName == "END_OF_MESSAGE")
        {
          tailedFoxAnnouncer.speakerSource.pitch = 1f;
          yield return (object) new WaitForSeconds(4f);
        }
        else
        {
          bool flag1 = line.apiName.StartsWith("BG_") || line.apiName.StartsWith("BELL_");
          bool flag2 = line.apiName.StartsWith("SUFFIX_");
          float absoluteTimeAddition = 0.0f;
          float relativeTimeAddition = 0.0f;
          if ((UnityEngine.Object) line.clip != (UnityEngine.Object) null)
          {
            if (flag1)
              tailedFoxAnnouncer.backgroundSource.PlayOneShot(line.clip);
            else if (flag2)
            {
              tailedFoxAnnouncer.speakerSource.Stop();
              tailedFoxAnnouncer.speakerSource.PlayOneShot(line.clip);
            }
            else if ((double) jammed > 0.0)
            {
              tailedFoxAnnouncer.speakerSource.Stop();
              float timeToJam = line.length * (jammed / 100f);
              tailedFoxAnnouncer.speakerSource.clip = line.clip;
              tailedFoxAnnouncer.speakerSource.time = 0.0f;
              tailedFoxAnnouncer.speakerSource.Play();
              yield return (object) new WaitForSeconds(timeToJam);
              float stepSize = 0.13f;
              for (int i = 0; i < jamSize; ++i)
              {
                absoluteTimeAddition -= stepSize * 3f;
                tailedFoxAnnouncer.speakerSource.time = timeToJam;
                yield return (object) new WaitForSeconds(stepSize);
              }
              jammed = 0.0f;
            }
            else
              tailedFoxAnnouncer.speakerSource.PlayOneShot(line.clip);
          }
          else if ((double) Math.Abs(tailedFoxAnnouncer.GetPitch(line.apiName) - 1f) > 0.00999999977648258)
          {
            speed = tailedFoxAnnouncer.GetPitch(line.apiName);
            tailedFoxAnnouncer.speakerSource.pitch = speed;
          }
          else if (line.apiName.StartsWith("JAM"))
          {
            string apiName = line.apiName;
            try
            {
              if (float.TryParse(apiName.Substring(4, 3), out jammed))
              {
                if (int.TryParse(apiName.Substring(8, 1), out jamSize))
                  goto label_25;
              }
              jammed = 0.0f;
            }
            catch
            {
              jammed = 0.0f;
            }
          }
label_25:
          yield return (object) new WaitForSeconds((line.length + relativeTimeAddition) / speed + absoluteTimeAddition);
          line = (NineTailedFoxAnnouncer.VoiceLine) null;
        }
      }
    }
  }

  private void Update()
  {
    if (NineTailedFoxAnnouncer.scpDeaths.Count <= 0)
      return;
    this.scpListTimer += Time.deltaTime;
    if ((double) this.scpListTimer <= 1.0)
      return;
    for (int index1 = 0; index1 < NineTailedFoxAnnouncer.scpDeaths.Count; ++index1)
    {
      string str1 = "";
      for (int index2 = 0; index2 < NineTailedFoxAnnouncer.scpDeaths[index1].scpSubjects.Count; ++index2)
      {
        string str2 = "";
        string fullName = NineTailedFoxAnnouncer.scpDeaths[index1].scpSubjects[index2].fullName;
        char[] chArray = new char[1]{ '-' };
        foreach (char ch in fullName.Split(chArray)[1])
          str2 = str2 + ch.ToString() + " ";
        str1 = index2 != 0 ? str1 + ". SCP " + str2 : str1 + "SCP " + str2;
      }
      DamageTypes.DamageType damageType = NineTailedFoxAnnouncer.scpDeaths[index1].hitInfo.GetDamageType();
      string tts;
      if (damageType == DamageTypes.Tesla)
        tts = str1 + "SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM";
      else if (damageType == DamageTypes.Nuke)
        tts = str1 + "SUCCESSFULLY TERMINATED BY ALPHA WARHEAD";
      else if (damageType == DamageTypes.Decont)
      {
        tts = str1 + "LOST IN DECONTAMINATION SEQUENCE";
      }
      else
      {
        CharacterClassManager characterClassManager = (CharacterClassManager) null;
        foreach (GameObject player in PlayerManager.players)
        {
          if (player.GetComponent<QueryProcessor>().PlayerId == NineTailedFoxAnnouncer.scpDeaths[index1].hitInfo.PlyId)
            characterClassManager = player.GetComponent<CharacterClassManager>();
        }
        if ((UnityEngine.Object) characterClassManager != (UnityEngine.Object) null)
        {
          string str2 = NineTailedFoxAnnouncer.scpDeaths[index1].scpSubjects[0].roleId != RoleType.Scp106 || NineTailedFoxAnnouncer.scpDeaths[index1].hitInfo.GetDamageType() != DamageTypes.RagdollLess ? "TERMINATED" : "CONTAINEDSUCCESSFULLY";
          switch (characterClassManager.Classes.SafeGet(characterClassManager.CurClass).team)
          {
            case Team.MTF:
              string str3 = NineTailedFoxUnits.host.list[characterClassManager.NtfUnit];
              char ch1 = str3[0];
              string str4 = int.Parse(str3.Split('-')[1]).ToString("00");
              string str5 = str1;
              string str6 = ch1.ToString();
              char ch2 = str4[0];
              string str7 = ch2.ToString();
              string str8 = "CONTAINEDSUCCESSFULLY CONTAINMENTUNIT NATO_" + str6 + " " + str7;
              ch2 = str4[1];
              string str9 = ch2.ToString();
              tts = str5 + str8 + str9;
              break;
            case Team.CHI:
              tts = str1 + str2 + " BY CHAOSINSURGENCY";
              break;
            case Team.RSC:
              tts = str1 + str2 + " BY SCIENCE PERSONNEL";
              break;
            case Team.CDP:
              tts = str1 + str2 + " BY CLASSD PERSONNEL";
              break;
            default:
              tts = str1 + "SUCCESSFULLY TERMINATED . CONTAINMENTUNIT UNKNOWN";
              break;
          }
        }
        else
          tts = str1 + "SUCCESSFULLY TERMINATED . TERMINATION CAUSE UNSPECIFIED";
      }
      int num1 = 0;
      bool flag = false;
      foreach (GameObject player in PlayerManager.players)
      {
        CharacterClassManager component = player.GetComponent<CharacterClassManager>();
        if (component.CurClass == RoleType.Scp079)
          flag = true;
        if (component.Classes.SafeGet(component.CurClass).team == Team.SCP)
          ++num1;
      }
      if (num1 == 1 & flag && Generator079.mainGenerator.totalVoltage < 4 && !Generator079.mainGenerator.forcedOvercharge)
      {
        Generator079.mainGenerator.forcedOvercharge = true;
        Recontainer079.BeginContainment(true);
        tts += " . ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE";
      }
      float num2 = (double) AlphaWarheadController.Host.timeToDetonation <= 0.0 ? 3.5f : 1f;
      this.ServerOnlyAddGlitchyPhrase(tts, UnityEngine.Random.Range(0.1f, 0.14f) * num2, UnityEngine.Random.Range(0.07f, 0.08f) * num2);
    }
    this.scpListTimer = 0.0f;
    NineTailedFoxAnnouncer.scpDeaths.Clear();
  }

  [Serializable]
  public class VoiceLine
  {
    public string apiName;
    public AudioClip clip;
    public float length;
    public string collection;

    public string GetName()
    {
      return this.apiName;
    }
  }

  [Serializable]
  public struct ScpDeath : IEquatable<NineTailedFoxAnnouncer.ScpDeath>
  {
    public List<Role> scpSubjects;
    public PlayerStats.HitInfo hitInfo;
    public string group;

    public bool Equals(NineTailedFoxAnnouncer.ScpDeath other)
    {
      return this.scpSubjects == other.scpSubjects && this.hitInfo == other.hitInfo && string.Equals(this.group, other.group);
    }

    public override bool Equals(object obj)
    {
      return obj is NineTailedFoxAnnouncer.ScpDeath other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((this.scpSubjects != null ? this.scpSubjects.GetHashCode() : 0) * 397 ^ this.hitInfo.GetHashCode()) * 397 ^ (this.group != null ? this.group.GetHashCode() : 0);
    }

    public static bool operator ==(
      NineTailedFoxAnnouncer.ScpDeath left,
      NineTailedFoxAnnouncer.ScpDeath right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      NineTailedFoxAnnouncer.ScpDeath left,
      NineTailedFoxAnnouncer.ScpDeath right)
    {
      return !left.Equals(right);
    }
  }

  private class ItemEqualityComparer : IEqualityComparer<NineTailedFoxAnnouncer.VoiceLine>
  {
    public bool Equals(NineTailedFoxAnnouncer.VoiceLine x, NineTailedFoxAnnouncer.VoiceLine y)
    {
      return x != null && (UnityEngine.Object) x.clip != (UnityEngine.Object) null && (UnityEngine.Object) x.clip == (UnityEngine.Object) y.clip;
    }

    public int GetHashCode(NineTailedFoxAnnouncer.VoiceLine obj)
    {
      return obj.clip.GetHashCode();
    }
  }
}
