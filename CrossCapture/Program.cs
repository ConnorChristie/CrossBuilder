using Nektra.Deviare2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static CrossCapture.WinEnum;

namespace CrossCapture
{
    public class Program
    {
        private readonly INktSpyMgr spyMgr;
        private readonly IList<string> fileNames = new List<string>();

        public Program()
        {
            spyMgr = new NktSpyMgr();
        }

        public void Run(string program, string arguments, string workingDir, Action<IEnumerable<FileInfo>> onExit)
        {
            spyMgr.Initialize();

            var NtWriteFile_hook = spyMgr.CreateHook("Ntdll.dll!NtWriteFile", (int)(
                eNktHookFlags.flgOnlyPostCall | eNktHookFlags.flgAutoHookChildProcess | eNktHookFlags.flgAutoHookActive));

            NtWriteFile_hook.OnFunctionCalled += OnNtWriteFile;
            NtWriteFile_hook.Hook(true);

            var psi = new ProcessStartInfo(program)
            {
                Arguments = arguments,
                WorkingDirectory = workingDir
            };
            var pc = Process.Start(psi);
            var proc = spyMgr.ProcessFromPID(pc.Id);

            NtWriteFile_hook.Attach(proc, true);

            pc.EnableRaisingEvents = true;
            pc.Exited += (sender, e) =>
            {
                var actualFiles = fileNames
                    .Select(x => new FileInfo(x))
                    .Where(x => !x.Name.Contains(":"));

                onExit(actualFiles);
            };
        }

        private void OnNtWriteFile(INktHook hook, INktProcess proc, INktHookCallInfo callInfo)
        {
            var fileHandle = callInfo.Params().GetAt(0).SizeTVal;
            var fileName = ReadFileInfo(proc.Handle(PROCESS_WM_READ), fileHandle);

            if (fileName == null)
                return;

            lock (fileNames)
            {
                if (!fileNames.Contains(fileName))
                {
                    fileNames.Add(fileName);
                }
            }
        }

        private unsafe string ReadFileInfo(IntPtr processHandle, IntPtr p_hfile)
        {
            WinApi.DuplicateHandle(processHandle, p_hfile, WinApi.GetCurrentProcess(), out var my_hFile, 0x80000000, true, 2);

            try
            {
                var fileName = new StringBuilder(MAX_PATH);
                var result = WinApi.GetFinalPathNameByHandle(my_hFile, fileName, MAX_PATH, 0);

                if (result > 0)
                {
                    var fileNameStr = fileName.ToString();

                    if (fileNameStr.StartsWith(@"\\?\"))
                    {
                        fileNameStr = fileNameStr.Substring(4);
                    }

                    return fileNameStr;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                WinApi.CloseHandle(my_hFile);
            }

            return null;
        }
    }
}
