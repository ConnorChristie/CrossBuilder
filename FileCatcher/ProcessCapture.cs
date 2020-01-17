using Nektra.Deviare2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FileCatcher
{
    public class ProcessCapture
    {
        private readonly string Program;
        private readonly string Arguments;
        private readonly string WorkingDir;

        private readonly INktSpyMgr spyMgr;
        private readonly IList<string> fileNames;

        public ProcessCapture(string program, string arguments, string workingDir)
        {
            Program = program;
            Arguments = arguments;
            WorkingDir = workingDir;

            spyMgr = new NktSpyMgr();
            fileNames = new List<string>();
        }

        public void Capture(Action<IEnumerable<FileInfo>> onExit)
        {
            spyMgr.Initialize();

            var NtWriteFile_hook = spyMgr.CreateHook("Ntdll.dll!NtWriteFile", (int)(
                eNktHookFlags.flgOnlyPostCall | eNktHookFlags.flgAutoHookChildProcess | eNktHookFlags.flgAutoHookActive));

            NtWriteFile_hook.OnFunctionCalled += OnNtWriteFile;
            NtWriteFile_hook.Hook(true);

            var psi = new ProcessStartInfo(Program)
            {
                Arguments = Arguments,
                WorkingDirectory = WorkingDir
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
            var fileName = ReadFileInfo(proc.Handle(WinEnum.PROCESS_WM_READ), fileHandle);

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
                var fileName = new StringBuilder(WinEnum.MAX_PATH);
                var result = WinApi.GetFinalPathNameByHandle(my_hFile, fileName, WinEnum.MAX_PATH, 0);

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
