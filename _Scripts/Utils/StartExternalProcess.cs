// Decompiled with JetBrains decompiler
// Type: _Scripts.Utils.StartExternalProcess
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace _Scripts.Utils
{
  public static class StartExternalProcess
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateProcessW(
      string lpApplicationName,
      [In] string lpCommandLine,
      IntPtr procSecAttrs,
      IntPtr threadSecAttrs,
      bool bInheritHandles,
      StartExternalProcess.ProcessCreationFlags dwCreationFlags,
      IntPtr lpEnvironment,
      string lpCurrentDirectory,
      ref StartExternalProcess.STARTUPINFO lpStartupInfo,
      ref StartExternalProcess.PROCESS_INFORMATION lpProcessInformation);

    public static uint Start(string path, string dir, bool hidden = false)
    {
      StartExternalProcess.ProcessCreationFlags dwCreationFlags = hidden ? StartExternalProcess.ProcessCreationFlags.CREATE_NO_WINDOW : StartExternalProcess.ProcessCreationFlags.NONE;
      StartExternalProcess.STARTUPINFO lpStartupInfo = new StartExternalProcess.STARTUPINFO()
      {
        cb = (uint) Marshal.SizeOf<StartExternalProcess.STARTUPINFO>()
      };
      StartExternalProcess.PROCESS_INFORMATION lpProcessInformation = new StartExternalProcess.PROCESS_INFORMATION();
      if (!StartExternalProcess.CreateProcessW((string) null, path, IntPtr.Zero, IntPtr.Zero, false, dwCreationFlags, IntPtr.Zero, dir, ref lpStartupInfo, ref lpProcessInformation))
        throw new Win32Exception();
      return lpProcessInformation.dwProcessId;
    }

    private struct PROCESS_INFORMATION
    {
      internal IntPtr hProcess;
      internal IntPtr hThread;
      internal uint dwProcessId;
      internal uint dwThreadId;
    }

    private struct STARTUPINFO
    {
      internal uint cb;
      internal IntPtr lpReserved;
      internal IntPtr lpDesktop;
      internal IntPtr lpTitle;
      internal uint dwX;
      internal uint dwY;
      internal uint dwXSize;
      internal uint dwYSize;
      internal uint dwXCountChars;
      internal uint dwYCountChars;
      internal uint dwFillAttribute;
      internal uint dwFlags;
      internal ushort wShowWindow;
      internal ushort cbReserved2;
      internal IntPtr lpReserved2;
      internal IntPtr hStdInput;
      internal IntPtr hStdOutput;
      internal IntPtr hStdError;
    }

    [Flags]
    private enum ProcessCreationFlags : uint
    {
      NONE = 0,
      CREATE_BREAKAWAY_FROM_JOB = 16777216, // 0x01000000
      CREATE_DEFAULT_ERROR_MODE = 67108864, // 0x04000000
      CREATE_NEW_CONSOLE = 16, // 0x00000010
      CREATE_NEW_PROCESS_GROUP = 512, // 0x00000200
      CREATE_NO_WINDOW = 134217728, // 0x08000000
      CREATE_PROTECTED_PROCESS = 262144, // 0x00040000
      CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 33554432, // 0x02000000
      CREATE_SECURE_PROCESS = 4194304, // 0x00400000
      CREATE_SEPARATE_WOW_VDM = 2048, // 0x00000800
      CREATE_SHARED_WOW_VDM = 4096, // 0x00001000
      CREATE_SUSPENDED = 4,
      CREATE_UNICODE_ENVIRONMENT = 1024, // 0x00000400
      DEBUG_ONLY_THIS_PROCESS = 2,
      DEBUG_PROCESS = 1,
      DETACHED_PROCESS = 8,
      EXTENDED_STARTUPINFO_PRESENT = 524288, // 0x00080000
      INHERIT_PARENT_AFFINITY = 65536, // 0x00010000
    }
  }
}
