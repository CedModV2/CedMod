// Decompiled with JetBrains decompiler
// Type: BanHandler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class BanHandler
{
  public static BanHandler.BanType GetBanType(int type)
  {
    return type > Enum.GetValues(typeof (BanHandler.BanType)).Cast<int>().Max() || type < Enum.GetValues(typeof (BanHandler.BanType)).Cast<int>().Min() ? BanHandler.BanType.UserId : (BanHandler.BanType) type;
  }

  public static void Init()
  {
    try
    {
      if (!File.Exists(BanHandler.GetPath(BanHandler.BanType.UserId)))
        File.Create(BanHandler.GetPath(BanHandler.BanType.UserId)).Close();
      else
        FileManager.RemoveEmptyLines(BanHandler.GetPath(BanHandler.BanType.UserId));
      if (!File.Exists(BanHandler.GetPath(BanHandler.BanType.IP)))
        File.Create(BanHandler.GetPath(BanHandler.BanType.IP)).Close();
      else
        FileManager.RemoveEmptyLines(BanHandler.GetPath(BanHandler.BanType.IP));
    }
    catch
    {
      ServerConsole.AddLog("Can't create ban files!");
    }
    BanHandler.ValidateBans();
  }

  public static bool IssueBan(BanDetails ban, BanHandler.BanType banType)
  {
    try
    {
      ban.OriginalName = ban.OriginalName.Replace(";", ":").Replace(Environment.NewLine, "").Replace("\n", "");
      if (!BanHandler.GetBans(banType).Any<BanDetails>((Func<BanDetails, bool>) (b => b.Id == ban.Id)))
      {
        FileManager.AppendFile(ban.ToString(), BanHandler.GetPath(banType), true);
        FileManager.RemoveEmptyLines(BanHandler.GetPath(banType));
      }
      else
      {
        BanHandler.RemoveBan(ban.Id, banType);
        BanHandler.IssueBan(ban, banType);
      }
      return true;
    }
    catch
    {
      return false;
    }
  }

  public static void ValidateBans()
  {
    ServerConsole.AddLog("Validating bans...");
    BanHandler.ValidateBans(BanHandler.BanType.UserId);
    BanHandler.ValidateBans(BanHandler.BanType.IP);
    ServerConsole.AddLog("Bans has been validated.");
  }

  public static void ValidateBans(BanHandler.BanType banType)
  {
    List<string> stringList = FileManager.ReadAllLinesList(BanHandler.GetPath(banType));
    List<int> intList = new List<int>();
    for (int index = stringList.Count - 1; index >= 0; --index)
    {
      string ban = stringList[index];
      if (BanHandler.ProcessBanItem(ban, banType) == null || !BanHandler.CheckExpiration(BanHandler.ProcessBanItem(ban, banType), BanHandler.BanType.NULL))
        intList.Add(index);
    }
    List<int> source = new List<int>();
    foreach (int num in intList)
    {
      if (!source.Contains(num))
        source.Add(num);
    }
    foreach (int index in (IEnumerable<int>) source.OrderByDescending<int, int>((Func<int, int>) (index => index)))
      stringList.RemoveAt(index);
    if (FileManager.ReadAllLines(BanHandler.GetPath(banType)) == stringList.ToArray())
      return;
    FileManager.WriteToFile((IEnumerable<string>) stringList.ToArray(), BanHandler.GetPath(banType), false);
  }

  public static bool CheckExpiration(BanDetails ban, BanHandler.BanType banType)
  {
    int num = (int) banType;
    if (ban == null)
      return false;
    if (TimeBehaviour.ValidateTimestamp(ban.Expires, TimeBehaviour.CurrentTimestamp(), 0L))
      return true;
    if (num >= 0)
      BanHandler.RemoveBan(ban.Id, banType);
    return false;
  }

  public static BanDetails ReturnChecks(BanDetails ban, BanHandler.BanType banType)
  {
    return !BanHandler.CheckExpiration(ban, banType) ? (BanDetails) null : ban;
  }

  public static void RemoveBan(string id, BanHandler.BanType banType)
  {
    id = id.Replace(";", ":").Replace(Environment.NewLine, "").Replace("\n", "");
    FileManager.WriteToFile((IEnumerable<string>) ((IEnumerable<string>) FileManager.ReadAllLines(BanHandler.GetPath(banType))).Where<string>((Func<string, bool>) (l => BanHandler.ProcessBanItem(l, banType) != null && BanHandler.ProcessBanItem(l, banType).Id != id)).ToArray<string>(), BanHandler.GetPath(banType), false);
  }

  public static List<BanDetails> GetBans(BanHandler.BanType banType)
  {
    return ((IEnumerable<string>) FileManager.ReadAllLines(BanHandler.GetPath(banType))).Select<string, BanDetails>((Func<string, BanDetails>) (b => BanHandler.ProcessBanItem(b, banType))).Where<BanDetails>((Func<BanDetails, bool>) (b => b != null)).ToList<BanDetails>();
  }

  public static KeyValuePair<BanDetails, BanDetails> QueryBan(
    string userId,
    string ip)
  {
    string ban1 = (string) null;
    string ban2 = (string) null;
    if (!string.IsNullOrEmpty(userId))
    {
      userId = userId.Replace(";", ":").Replace(Environment.NewLine, "").Replace("\n", "");
      ban1 = ((IEnumerable<string>) FileManager.ReadAllLines(BanHandler.GetPath(BanHandler.BanType.UserId))).FirstOrDefault<string>((Func<string, bool>) (b => BanHandler.ProcessBanItem(b, BanHandler.BanType.UserId)?.Id == userId));
    }
    if (!string.IsNullOrEmpty(ip))
    {
      ip = ip.Replace(";", ":").Replace(Environment.NewLine, "").Replace("\n", "");
      ban2 = ((IEnumerable<string>) FileManager.ReadAllLines(BanHandler.GetPath(BanHandler.BanType.IP))).FirstOrDefault<string>((Func<string, bool>) (b => BanHandler.ProcessBanItem(b, BanHandler.BanType.IP)?.Id == ip));
    }
    return new KeyValuePair<BanDetails, BanDetails>(BanHandler.ReturnChecks(BanHandler.ProcessBanItem(ban1, BanHandler.BanType.UserId), BanHandler.BanType.UserId), BanHandler.ReturnChecks(BanHandler.ProcessBanItem(ban2, BanHandler.BanType.IP), BanHandler.BanType.IP));
  }

  public static BanDetails ProcessBanItem(string ban, BanHandler.BanType banType)
  {
    if (string.IsNullOrEmpty(ban) || !ban.Contains(";"))
      return (BanDetails) null;
    string[] strArray = ban.Split(';');
    if (strArray.Length != 6)
      return (BanDetails) null;
    if (banType == BanHandler.BanType.UserId && !strArray[1].Contains("@"))
      strArray[1] = strArray[1].Trim() + "@steam";
    return new BanDetails()
    {
      OriginalName = strArray[0],
      Id = strArray[1].Trim(),
      Expires = Convert.ToInt64(strArray[2].Trim()),
      Reason = strArray[3],
      Issuer = strArray[4],
      IssuanceTime = Convert.ToInt64(strArray[5].Trim())
    };
  }

  public static string GetPath(BanHandler.BanType banType)
  {
    return banType == BanHandler.BanType.UserId || banType != BanHandler.BanType.IP ? ConfigSharing.Paths[0] + "UserIdBans.txt" : ConfigSharing.Paths[0] + "IpBans.txt";
  }

  public enum BanType
  {
    NULL = -1, // 0xFFFFFFFF
    UserId = 0,
    IP = 1,
  }
}
