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
        private INktHooksEnum hooks;

        private readonly IList<string> fileNames = new List<string>();

        public Program()
        {
            spyMgr = new NktSpyMgr();
        }

        public void Run(Action<IEnumerable<FileInfo>> onExit)
        {
            spyMgr.Initialize();
            hooks = spyMgr.CreateHooksCollection();

            var Write_hook = spyMgr.CreateHook("Ntdll.dll!NtWriteFile", (int)(
                eNktHookFlags.flgOnlyPostCall | eNktHookFlags.flgAutoHookChildProcess | eNktHookFlags.flgAutoHookActive));

            Write_hook.OnFunctionCalled += OnNtWriteFile;
            hooks.Add(Write_hook);
            hooks.Hook(true);

            var psi = new ProcessStartInfo(@"make.exe")
            {
                Arguments = "install",
                WorkingDirectory = @"D:\Git\librealsense\build"
            };
            var pc = Process.Start(psi);
            var proc = spyMgr.ProcessFromPID(pc.Id);

            hooks.Attach(proc, true);

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

            //var fileInfo = new FILE_NAME_INFO();
            //var fileInfoSize = Marshal.SizeOf(fileInfo);
            //var p_fileInfo = Marshal.AllocHGlobal(fileInfoSize);

            //try
            //{
            //    Marshal.StructureToPtr(fileInfo, p_fileInfo, false);

            //    if (WinApi.GetFileInformationByHandleEx(my_hFile, FileInformationClass.FileNameInfo, ref fileInfo, fileInfoSize))
            //    {
            //        var fileName2 = new StringBuilder(MAX_PATH);
            //        var result = WinApi.GetFinalPathNameByHandle(my_hFile, fileName2, MAX_PATH, 0);

            //        var fileName = new StringBuilder(fileInfo.FileNameLength);
            //        var length = WinApi.GetFullPathName(fileInfo.FileName, fileInfo.FileNameLength, fileName, null);

            //        return fileName2.ToString();
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}
            //finally
            //{
            //    Marshal.FreeHGlobal(p_fileInfo);
            //    WinApi.CloseHandle(my_hFile);
            //}

            return null;
        }

        public static void Main(string[] args)
        {
            new Program().Run(x => { });

            while (Console.ReadKey().Key != ConsoleKey.Escape);
        }
    }
}
