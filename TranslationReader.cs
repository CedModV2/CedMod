// Decompiled with JetBrains decompiler
// Type: TranslationReader
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class TranslationReader
{
  public static Dictionary<string, string[]> elements = new Dictionary<string, string[]>();
  public static Dictionary<string, string[]> fallback = new Dictionary<string, string[]>();
  public static string path;
  public const string NoTranslation = "NO_TRANSLATION";
  private const string prefix = "Translations/";

  static TranslationReader()
  {
    TranslationReader.Refresh();
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(TranslationReader.OnSceneWasLoaded);
  }

  private static void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
  {
    PlayerPrefsSl.Refresh();
    TranslationReader.Refresh();
  }

  private static void Refresh()
  {
    string str;
    if (!PlayerPrefsSl.Get("translation_changed", false))
      str = TranslationReader.CheckPath(CultureInfo.CurrentCulture.Name, CultureInfo.CurrentCulture.Parent.Name, "en", "English (default)");
    else
      str = TranslationReader.CheckPath(PlayerPrefsSl.Get("translation_path", "en"), "English (default)");
    TranslationReader.path = str;
    TranslationReader.LoadTranslation(TranslationReader.path, TranslationReader.elements);
    TranslationReader.LoadTranslation(TranslationReader.CheckPath("en", "English (default)"), TranslationReader.fallback);
  }

  private static void LoadTranslation(
    string translationPath,
    Dictionary<string, string[]> dictionary)
  {
    dictionary.Clear();
    if (File.Exists(translationPath + "Legacy_Interfaces.txt") && File.Exists(translationPath + "Legancy_Interfaces.txt"))
      File.Delete(translationPath + "Legancy_Interfaces.txt");
    foreach (string file in Directory.GetFiles(translationPath))
    {
      string[] strArray = FileManager.ReadAllLines(file);
      for (int index = 0; index < strArray.Length; ++index)
        strArray[index] = strArray[index].Replace("\\n", Environment.NewLine);
      string withoutExtension = Path.GetFileNameWithoutExtension(file);
      dictionary.Add(withoutExtension == "Legancy_Interfaces" ? "Legacy_Interfaces" : withoutExtension, strArray);
    }
  }

  private static string CheckPath(params string[] suffixes)
  {
    foreach (string suffix in suffixes)
    {
      string path = "Translations/" + suffix;
      if (Directory.Exists(path))
        return path;
    }
    throw new FileNotFoundException();
  }

  public static string Get(string keyName, int index, string defaultvalue = "NO_TRANSLATION")
  {
    if (keyName == "Legancy_Interfaces")
      keyName = "Legacy_Interfaces";
    string[] strArray;
    if (TranslationReader.elements.TryGetValue(keyName, out strArray))
    {
      string str;
      if (index < strArray.Length && !string.IsNullOrWhiteSpace(str = strArray[index]))
        return str;
      Debug.Log((object) string.Format("Missing translation! {0} {1} {2}", (object) keyName, (object) index, (object) defaultvalue));
      return TranslationReader.GetFallback(keyName, index) ?? defaultvalue;
    }
    Debug.Log((object) ("Tried to get Translation from nonexistant file " + keyName));
    return TranslationReader.GetFallback(keyName, index) ?? defaultvalue;
  }

  private static string GetFallback(string keyName, int index)
  {
    string[] array;
    string element;
    return !TranslationReader.fallback.TryGetValue(keyName, out array) || !array.TryGet<string>(index, out element) || string.IsNullOrWhiteSpace(element) ? (string) null : element;
  }

  public static string GetFormatted(string keyName, int index, string defaultvalue, object obj1)
  {
    return string.Format(TranslationReader.Get(keyName, index, defaultvalue), obj1);
  }

  public static string GetFormatted(
    string keyName,
    int index,
    string defaultvalue,
    object obj1,
    object obj2)
  {
    return string.Format(TranslationReader.Get(keyName, index, defaultvalue), obj1, obj2);
  }

  public static string GetFormatted(
    string keyName,
    int index,
    string defaultvalue,
    object obj1,
    object obj2,
    object obj3)
  {
    return string.Format(TranslationReader.Get(keyName, index, defaultvalue), obj1, obj2, obj3);
  }

  public static string GetFormatted(
    string keyName,
    int index,
    string defaultvalue,
    params object[] format)
  {
    return string.Format(TranslationReader.Get(keyName, index, defaultvalue), format);
  }
}
