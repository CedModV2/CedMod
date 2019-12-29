// Decompiled with JetBrains decompiler
// Type: CentralServerKeyCache
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CentralServerKeyCache
{
  internal static readonly AsymmetricKeyParameter MasterKey = ECDSA.PublicKeyFromString("-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAmxZRMP03JfPEP/qt7n34Ryi74CDe\r\nRZy4er5dQynKaQ3vl1F4VRsSGN+jBrZPcX3GB2u0OTXNUA8hcIDRhVb+GgYAcDmY\r\n+7utHYAZBK3APSxGn46p1+IAChsgl9r93bQz7AJVxxWHYKEA78jMVz6qKHlqKc6a\r\nkUswVSYosQGvw/Agzb0=\r\n-----END PUBLIC KEY-----");
  private const string CacheLocation = "internal/KeyCache";
  private const string CacheSignatureLocation = "internal/KeySignatureCache";
  private const string InternalDir = "internal/";
  private const string MasterPublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAmxZRMP03JfPEP/qt7n34Ryi74CDe\r\nRZy4er5dQynKaQ3vl1F4VRsSGN+jBrZPcX3GB2u0OTXNUA8hcIDRhVb+GgYAcDmY\r\n+7utHYAZBK3APSxGn46p1+IAChsgl9r93bQz7AJVxxWHYKEA78jMVz6qKHlqKc6a\r\nkUswVSYosQGvw/Agzb0=\r\n-----END PUBLIC KEY-----";

  public static string ReadCache()
  {
    try
    {
      string appFolder = FileManager.GetAppFolder(true, false, "");
      string path1 = appFolder + "internal/KeyCache";
      string path2 = appFolder + "internal/KeySignatureCache";
      if (!File.Exists(path1))
      {
        ServerConsole.AddLog("Central server public key not found in cache.");
        return (string) null;
      }
      if (!File.Exists(path2))
      {
        ServerConsole.AddLog("Central server public key signature not found in cache.");
        return (string) null;
      }
      string[] strArray1 = FileManager.ReadAllLines(path1);
      string[] strArray2 = FileManager.ReadAllLines(path2);
      if (strArray2.Length == 0)
      {
        ServerConsole.AddLog("Can't load central server public key from cache - empty signature.");
        return (string) null;
      }
      string data = ((IEnumerable<string>) strArray1).Aggregate<string, string>("", (Func<string, string, string>) ((current, line) => current + line + "\r\n")).Trim();
      try
      {
        if (ECDSA.Verify(data, strArray2[0], CentralServerKeyCache.MasterKey))
          return data;
        GameCore.Console.AddLog("Invalid signature of Central Server Key in cache!", Color.red, false);
        return (string) null;
      }
      catch (Exception ex)
      {
        if (ServerStatic.IsDedicated)
          ServerConsole.AddLog("Can't load central server public key from cache - " + ex.Message);
        else
          GameCore.Console.AddLog("Can't load central server public key from cache - " + ex.Message, Color.magenta, false);
        return (string) null;
      }
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Can't read public key cache - " + ex.Message);
      return (string) null;
    }
  }

  public static void SaveCache(string key, string signature)
  {
    try
    {
      if (!ECDSA.Verify(key, signature, CentralServerKeyCache.MasterKey))
      {
        GameCore.Console.AddLog("Invalid signature of Central Server Key!", Color.red, false);
      }
      else
      {
        string appFolder = FileManager.GetAppFolder(true, false, "");
        string path = appFolder + "internal/KeyCache";
        if (!Directory.Exists(FileManager.GetAppFolder(true, false, "") + "internal/"))
          Directory.CreateDirectory(FileManager.GetAppFolder(true, false, "") + "internal/");
        if (File.Exists(path))
        {
          if (key == CentralServerKeyCache.ReadCache())
          {
            ServerConsole.AddLog("Key cache is up to date.");
            return;
          }
          File.Delete(path);
        }
        ServerConsole.AddLog("Updating key cache...");
        FileManager.WriteStringToFile(key, path);
        FileManager.WriteStringToFile(signature, appFolder + "internal/KeySignatureCache");
        ServerConsole.AddLog("Key cache updated.");
      }
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Can't write public key cache - " + ex.Message);
    }
  }
}
