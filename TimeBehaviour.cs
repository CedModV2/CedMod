// Decompiled with JetBrains decompiler
// Type: TimeBehaviour
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

public static class TimeBehaviour
{
  private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

  public static long CurrentTimestamp()
  {
    return DateTime.UtcNow.Ticks;
  }

  public static ulong CurrentUnixTimestamp
  {
    get
    {
      return (ulong) DateTime.UtcNow.Subtract(TimeBehaviour.UnixEpoch).TotalSeconds;
    }
  }

  public static long GetBanExpieryTime(uint minutes)
  {
    DateTime dateTime = DateTime.UtcNow;
    dateTime = dateTime.AddMinutes((double) minutes);
    return dateTime.Ticks;
  }

  public static bool ValidateTimestamp(long timestampentry, long timestampexit, long limit)
  {
    return timestampexit - timestampentry < limit;
  }

  public static string FormatTime(string format)
  {
    return TimeBehaviour.FormatTime(format, DateTimeOffset.Now);
  }

  public static string FormatTime(string format, DateTimeOffset date)
  {
    string str1 = format.Replace("yyyy", date.Year.ToString()).Replace("MM", Misc.LeadingZeroes(date.Month, 2U, false)).Replace("M", date.Month.ToString()).Replace("dd", Misc.LeadingZeroes(date.Day, 2U, false)).Replace("d", date.Day.ToString()).Replace("HH", Misc.LeadingZeroes(date.Hour, 2U, false)).Replace("H", date.Hour.ToString()).Replace("mm", Misc.LeadingZeroes(date.Minute, 2U, false)).Replace("m", date.Minute.ToString()).Replace("ss", Misc.LeadingZeroes(date.Second, 2U, false)).Replace("s", date.Second.ToString()).Replace("fff", Misc.LeadingZeroes(date.Millisecond, 3U, false)).Replace("ff", Misc.LeadingZeroes(date.Millisecond / 10, 2U, false)).Replace("f", (date.Millisecond / 100).ToString());
    TimeSpan offset = date.Offset;
    string str2 = Misc.LeadingZeroes(offset.Hours, 2U, true);
    offset = date.Offset;
    string str3 = Misc.LeadingZeroes(offset.Minutes, 2U, false);
    string newValue = str2 + ":" + str3;
    return str1.Replace("zzz", newValue).Replace("zz", Misc.LeadingZeroes(date.Offset.Hours, 2U, true)).Replace("z", Misc.LeadingZeroes(date.Offset.Hours, 1U, true));
  }
}
