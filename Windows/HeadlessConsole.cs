// Decompiled with JetBrains decompiler
// Type: Windows.HeadlessConsole
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Windows
{
  public class HeadlessConsole
  {
    private TextWriter oldOutput;
    private const int STD_OUTPUT_HANDLE = -11;

    public void Initialize()
    {
      if (!HeadlessConsole.AttachConsole(uint.MaxValue))
        HeadlessConsole.AllocConsole();
      this.oldOutput = Console.Out;
      try
      {
        Console.SetOut((TextWriter) new StreamWriter((Stream) new FileStream(HeadlessConsole.GetStdHandle(-11), FileAccess.Write), Encoding.ASCII)
        {
          AutoFlush = true
        });
      }
      catch (Exception ex)
      {
        Debug.Log((object) ("Couldn't redirect output: " + ex.Message));
      }
    }

    public void Shutdown()
    {
      Console.SetOut(this.oldOutput);
      HeadlessConsole.FreeConsole();
    }

    public void SetTitle(string strName)
    {
      HeadlessConsole.SetConsoleTitle(strName);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleTitle(string lpConsoleTitle);
  }
}
