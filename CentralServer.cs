// Decompiled with JetBrains decompiler
// Type: CentralServer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class CentralServer : MonoBehaviour
{
  public static object RefreshLock;
  private static string _serversPath;
  private static List<string> _workingServers;
  private static DateTime _lastReset;

  public static string MasterUrl { get; internal set; }

  public static string StandardUrl { get; internal set; }

  public static string SelectedServer { get; internal set; }

  public static bool TestServer { get; internal set; }

  public static bool ServerSelected { get; set; }

  internal static string[] Servers { get; private set; }

  public void Start()
  {
    if (File.Exists(FileManager.GetAppFolder(true, false, "") + "testserver.txt"))
    {
      CentralServer.StandardUrl = "https://test.scpslgame.com/";
      CentralServer.MasterUrl = "https://test.scpslgame.com/";
      CentralServer.SelectedServer = "TEST";
      CentralServer.TestServer = true;
      CentralServer.ServerSelected = true;
      ServerConsole.AddLog("Using TEST central server: " + CentralServer.MasterUrl);
    }
    else
    {
      CentralServer.MasterUrl = "https://api.scpslgame.com/";
      CentralServer.StandardUrl = "https://api.scpslgame.com/";
      CentralServer.TestServer = false;
      CentralServer._lastReset = DateTime.MinValue;
      CentralServer.Servers = new string[0];
      CentralServer._workingServers = new List<string>();
      CentralServer.RefreshLock = new object();
      CentralServer._serversPath = FileManager.GetAppFolder(true, false, "") + "internal/";
      if (!Directory.Exists(CentralServer._serversPath))
        Directory.CreateDirectory(CentralServer._serversPath);
      CentralServer._serversPath += "CentralServers";
      if (File.Exists(CentralServer._serversPath))
      {
        CentralServer.Servers = FileManager.ReadAllLines(CentralServer._serversPath);
        if (((IEnumerable<string>) CentralServer.Servers).Any<string>((Func<string, bool>) (server => !Regex.IsMatch(server, "^[a-zA-Z0-9]*$"))))
        {
          GameCore.Console.AddLog("Malformed server found on the list. Removing the list and redownloading it from api.scpslgame.com.", Color.yellow, false);
          CentralServer.Servers = new string[0];
          try
          {
            File.Delete(CentralServer._serversPath);
          }
          catch (Exception ex)
          {
            GameCore.Console.AddLog("Failed to delete malformed central server list.\nException: " + ex.Message, Color.red, false);
          }
          new Thread((ThreadStart) (() => CentralServer.RefreshServerList(true, false))).Start();
          return;
        }
        CentralServer._workingServers = ((IEnumerable<string>) CentralServer.Servers).ToList<string>();
        if (!ServerStatic.IsDedicated)
          GameCore.Console.AddLog("Cached central servers count: " + (object) CentralServer.Servers.Length, Color.grey, false);
        if (CentralServer.Servers.Length != 0)
        {
          CentralServer.SelectedServer = CentralServer.Servers[new System.Random().Next(CentralServer.Servers.Length)];
          CentralServer.StandardUrl = "https://" + CentralServer.SelectedServer.ToLower() + ".scpslgame.com/";
          if (ServerStatic.IsDedicated)
            ServerConsole.AddLog("Selected central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")");
          else
            GameCore.Console.AddLog("Selected central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.grey, false);
        }
      }
      new Thread((ThreadStart) (() => CentralServer.RefreshServerList(true, false))).Start();
    }
  }

  public static void RefreshServerList(bool planned = false, bool loop = false)
  {
    while (true)
    {
      lock (CentralServer.RefreshLock)
      {
        if (CentralServer.ServerSelected)
          break;
        if (CentralServer._workingServers.Count == 0)
        {
          if (CentralServer.Servers.Length == 0)
          {
            CentralServer.StandardUrl = "https://api.scpslgame.com/";
            CentralServer.SelectedServer = "Primary API";
          }
          else
          {
            CentralServer._workingServers = ((IEnumerable<string>) CentralServer.Servers).ToList<string>();
            CentralServer.StandardUrl = "https://" + CentralServer._workingServers[0] + ".scpslgame.com/";
            CentralServer.SelectedServer = CentralServer._workingServers[0];
          }
        }
        byte num = 1;
        while (num != (byte) 3)
        {
          ++num;
          try
          {
            string[] strArray = HttpQuery.Get(CentralServer.StandardUrl + "servers.php").Split(';');
            if (File.Exists(CentralServer._serversPath))
              File.Delete(CentralServer._serversPath);
            FileManager.WriteToFile((IEnumerable<string>) strArray, CentralServer._serversPath, false);
            GameCore.Console.AddLog("Updated list of central servers.", Color.green, false);
            GameCore.Console.AddLog("Central servers count: " + (object) strArray.Length, Color.cyan, false);
            CentralServer.Servers = strArray;
            if (planned && ((IEnumerable<string>) CentralServer.Servers).All<string>((Func<string, bool>) (srv => srv != CentralServer.SelectedServer)))
            {
              CentralServer._workingServers = ((IEnumerable<string>) CentralServer.Servers).ToList<string>();
              CentralServer.ChangeCentralServer(false);
            }
            CentralServer.ServerSelected = true;
            break;
          }
          catch (Exception ex)
          {
            GameCore.Console.AddLog("Can't update central servers list!", Color.red, false);
            GameCore.Console.AddLog("Error: " + ex.Message, Color.red, false);
            if (CentralServer.SelectedServer == "Primary API")
            {
              CentralServer.ServerSelected = true;
              break;
            }
            CentralServer.ChangeCentralServer(true);
          }
        }
      }
      if (loop)
        Thread.Sleep(900000);
      else
        break;
    }
  }

  public static bool ChangeCentralServer(bool remove)
  {
    CentralServer.ServerSelected = false;
    CentralServer.TestServer = false;
    if (CentralServer.SelectedServer == "Primary API")
    {
      if (CentralServer._lastReset >= DateTime.Now.AddMinutes(-2.0))
        return false;
      CentralServer.RefreshServerList(false, false);
      return true;
    }
    if (CentralServer._workingServers.Count == 0)
    {
      GameCore.Console.AddLog("All known central servers aren't working.", Color.yellow, false);
      CentralServer._workingServers.Add("API");
      CentralServer.SelectedServer = "Primary API";
      CentralServer.StandardUrl = "https://api.scpslgame.com/";
      GameCore.Console.AddLog("Changed central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.yellow, false);
      return true;
    }
    if (remove && CentralServer._workingServers.Contains(CentralServer.SelectedServer))
      CentralServer._workingServers.Remove(CentralServer.SelectedServer);
    if (CentralServer._workingServers.Count == 0)
    {
      CentralServer._workingServers.Add("API");
      CentralServer.SelectedServer = "Primary API";
      CentralServer.StandardUrl = "https://api.scpslgame.com/";
      GameCore.Console.AddLog("Changed central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.yellow, false);
      return true;
    }
    System.Random random = new System.Random();
    CentralServer.SelectedServer = CentralServer._workingServers[random.Next(0, CentralServer._workingServers.Count)];
    CentralServer.StandardUrl = "https://" + CentralServer.SelectedServer.ToLower() + ".scpslgame.com/";
    GameCore.Console.AddLog("Changed central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.yellow, false);
    return true;
  }
}
