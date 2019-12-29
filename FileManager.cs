// Decompiled with JetBrains decompiler
// Type: FileManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class FileManager
{
  private static string _appfolder = "";
  private static string _configfolder = "";
  public static Encoding Utf8Encoding = (Encoding) new UTF8Encoding(false);
  private static readonly List<string> stringList = new List<string>();

  public static void RefreshAppFolder()
  {
    FileManager._appfolder = !ServerStatic.IsDedicated || ConfigFile.HosterPolicy == null || !ConfigFile.HosterPolicy.GetBool("gamedir_for_configs", false) ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FileManager.GetPathSeparator().ToString() + "SCP Secret Laboratory" : "AppData";
  }

  public static string GetAppFolder(bool addSeparator = true, bool serverConfig = false, string centralConfig = "")
  {
    if (string.IsNullOrEmpty(FileManager._appfolder))
      FileManager.RefreshAppFolder();
    if (serverConfig && !string.IsNullOrEmpty(FileManager._configfolder) && string.IsNullOrEmpty(centralConfig))
      return FileManager._configfolder + (addSeparator ? FileManager.GetPathSeparator().ToString() : "");
    string appfolder = FileManager._appfolder;
    char pathSeparator;
    string str1;
    if (!(addSeparator | serverConfig))
    {
      str1 = "";
    }
    else
    {
      pathSeparator = FileManager.GetPathSeparator();
      str1 = pathSeparator.ToString();
    }
    string str2;
    if (!serverConfig)
    {
      str2 = "";
    }
    else
    {
      string str3 = string.IsNullOrEmpty(centralConfig) ? (ServerStatic.IsDedicated ? ServerStatic.ServerPort.ToString() : "nondedicated") : centralConfig;
      string str4;
      if (!addSeparator)
      {
        str4 = "";
      }
      else
      {
        pathSeparator = FileManager.GetPathSeparator();
        str4 = pathSeparator.ToString();
      }
      str2 = "config/" + str3 + str4;
    }
    return appfolder + str1 + str2;
  }

  public static string StripPath(string path)
  {
    path = path.Replace("\"", "").Trim();
    while (path.EndsWith("\\") || path.EndsWith("/") || path.EndsWith(FileManager.GetPathSeparator().ToString()))
      path = path.Substring(0, path.Length - 1);
    return path;
  }

  public static void SetAppFolder(string path)
  {
    path = FileManager.StripPath(path);
    if (!Directory.Exists(path))
      FileManager._appfolder = "";
    else
      FileManager._appfolder = path;
  }

  public static void SetConfigFolder(string path)
  {
    path = FileManager.StripPath(path);
    if (!Directory.Exists(path))
      FileManager._configfolder = "";
    else
      FileManager._configfolder = path;
  }

  public static string ReplacePathSeparators(string path)
  {
    return path.Replace('/', FileManager.GetPathSeparator()).Replace('\\', FileManager.GetPathSeparator());
  }

  public static char GetPathSeparator()
  {
    return Path.DirectorySeparatorChar;
  }

  public static bool FileExists(string path)
  {
    return File.Exists(path);
  }

  public static bool DictionaryExists(string path)
  {
    return Directory.Exists(path);
  }

  public static void FileCreate(string path)
  {
    File.Create(path).Dispose();
  }

  public static FileStream FileStreamCreate(string path)
  {
    return File.Create(path);
  }

  public static string[] ReadAllLines(string path)
  {
    FileManager.stringList.Clear();
    using (StreamReader streamReader = new StreamReader(path))
    {
      string str;
      while ((str = streamReader.ReadLine()) != null)
        FileManager.stringList.Add(str);
    }
    return FileManager.stringList.ToArray();
  }

  public static List<string> ReadAllLinesList(string path)
  {
    List<string> stringList = new List<string>();
    using (StreamReader streamReader = new StreamReader(path))
    {
      string str;
      while ((str = streamReader.ReadLine()) != null)
        stringList.Add(str);
    }
    return stringList;
  }

  public static void ReadAllLinesList(string path, List<string> list)
  {
    list.Clear();
    using (StreamReader streamReader = new StreamReader(path))
    {
      string str;
      while ((str = streamReader.ReadLine()) != null)
        list.Add(str);
    }
  }

  public static string[] ReadAllLinesSafe(string path)
  {
    FileManager.stringList.Clear();
    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
      using (StreamReader streamReader = new StreamReader((Stream) fileStream))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
          FileManager.stringList.Add(str);
      }
    }
    return FileManager.stringList.ToArray();
  }

  public static List<string> ReadAllLinesSafeList(string path)
  {
    List<string> stringList = new List<string>();
    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
      using (StreamReader streamReader = new StreamReader((Stream) fileStream))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
          stringList.Add(str);
      }
    }
    return stringList;
  }

  public static void ReadAllLinesSafeList(string path, List<string> list)
  {
    list.Clear();
    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
      using (StreamReader streamReader = new StreamReader((Stream) fileStream))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
          list.Add(str);
      }
    }
  }

  public static string ReadAllText(string path)
  {
    return File.ReadAllText(path);
  }

  public static string ReadAllTextSafe(string path)
  {
    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
      using (StreamReader streamReader = new StreamReader((Stream) fileStream))
        return streamReader.ReadToEnd();
    }
  }

  public static void WriteToFile(IEnumerable<string> data, string path, bool removeempty = false)
  {
    File.WriteAllLines(path, removeempty ? data.Where<string>((Func<string, bool>) (line => !string.IsNullOrWhiteSpace(line.Replace(Environment.NewLine, "").Replace("\r\n", "").Replace("\n", "").Replace(" ", "")))) : data, FileManager.Utf8Encoding);
  }

  public static void WriteStringToFile(string data, string path)
  {
    File.WriteAllText(path, data, FileManager.Utf8Encoding);
  }

  public static void WriteToFileSafe(IEnumerable<string> data, string path, bool removeempty = false)
  {
    using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
    {
      using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream, FileManager.Utf8Encoding))
        streamWriter.Write(string.Join("\r\n", data));
    }
  }

  public static void WriteStringToFileSafe(string data, string path)
  {
    using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
    {
      using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream, FileManager.Utf8Encoding))
        streamWriter.Write(data);
    }
  }

  public static void AppendFile(string data, string path, bool newLine = true)
  {
    string[] strArray = FileManager.ReadAllLines(path);
    if (!newLine || strArray.Length == 0 || (strArray[strArray.Length - 1].EndsWith(Environment.NewLine) || strArray[strArray.Length - 1].EndsWith("\n")))
      File.AppendAllText(path, data, FileManager.Utf8Encoding);
    else
      File.AppendAllText(path, Environment.NewLine + data, FileManager.Utf8Encoding);
  }

  public static void AppendFileSafe(string data, string path, bool newLine = true)
  {
    using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
    {
      using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream, FileManager.Utf8Encoding))
        streamWriter.Write(newLine ? "\r\n" + data : data);
    }
  }

  public static void RenameFile(string path, string newpath)
  {
    File.Move(path, newpath);
  }

  public static void DeleteFile(string path)
  {
    File.Delete(path);
  }

  public static void ReplaceLine(int line, string text, string path)
  {
    string[] strArray = FileManager.ReadAllLines(path);
    strArray[line] = text;
    FileManager.WriteToFile((IEnumerable<string>) strArray, path, false);
  }

  public static void RemoveEmptyLines(string path)
  {
    string[] strArray = FileManager.ReadAllLines(path);
    string[] array = ((IEnumerable<string>) strArray).Where<string>((Func<string, bool>) (s => !string.IsNullOrWhiteSpace(s.Replace(Environment.NewLine, "").Replace("\r\n", "").Replace("\n", "").Replace(" ", "")))).ToArray<string>();
    if (strArray == array)
      return;
    FileManager.WriteToFile((IEnumerable<string>) array, path, false);
  }

  private static void DirectoryCopy(
    string sourceDirName,
    string destDirName,
    bool copySubDirs = true,
    bool overwrite = true)
  {
    DirectoryInfo directoryInfo1 = new DirectoryInfo(sourceDirName);
    if (!directoryInfo1.Exists)
      throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
    DirectoryInfo[] directories = directoryInfo1.GetDirectories();
    if (Directory.Exists(destDirName))
      Directory.Delete(destDirName, true);
    Directory.CreateDirectory(destDirName);
    foreach (FileInfo file in directoryInfo1.GetFiles())
    {
      string destFileName = Path.Combine(destDirName, file.Name);
      file.CopyTo(destFileName, overwrite);
    }
    if (!copySubDirs)
      return;
    foreach (DirectoryInfo directoryInfo2 in directories)
    {
      string destDirName1 = Path.Combine(destDirName, directoryInfo2.Name);
      FileManager.DirectoryCopy(directoryInfo2.FullName, destDirName1, overwrite, true);
    }
  }
}
