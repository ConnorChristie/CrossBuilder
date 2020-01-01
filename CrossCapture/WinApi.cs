using System;
using System.Runtime.InteropServices;
using System.Text;
using static CrossCapture.WinEnum;
using static CrossCapture.WinType;

namespace CrossCapture
{
    public class WinApi
    {
        public static int ToInt32(byte[] buf)
        {
            return (buf[0] & 0xFF) | ((buf[1] & 0xFF) << 8) | ((buf[2] & 0xFF) << 16) | ((buf[3] & 0xFF) << 24);
        }

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32", SetLastError = true)]
        public static extern unsafe bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
            uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool GetFileInformationByHandle(IntPtr hFile, ref BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool GetFileInformationByHandleEx(IntPtr hFile, FileInformationClass FileInformationClass, IntPtr lpFileInformation, int dwBufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool GetFileInformationByHandleEx(IntPtr hFile, FileInformationClass FileInformationClass, ref FILE_FULL_DIR_INFO lpFileInformation, int dwBufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool GetFileInformationByHandleEx(IntPtr hFile, FileInformationClass FileInformationClass, ref FILE_NAME_INFO lpFileInformation, int dwBufferSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern unsafe int GetFinalPathNameByHandle(IntPtr hFile, [Out, MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpszFilePath, int cchFilePath, int dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern unsafe int GetFullPathName(string lpFileName, int nBufferLength, [Out, MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpBuffer, [Out, MarshalAs(UnmanagedType.LPTStr)]StringBuilder lpFilePart);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int ZwQueryInformationFile(int hfile, ref int ioStatusBlock, ref int fileInformation, int length, int FileInformationClass);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int ZwQueryInformationFile(int hfile, ref IO_STATUS_BLOCK ioStatusBlock, ref FILE_NAME_INFO fileInformation, int length, int FileInformationClass);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern unsafe int ZwQuerySystemInformation(int SystemInformationClass, ref SYSTEM_HANDLE_INFORMATION SystemInformation, uint SystemInformationLength, ref uint ReturnLength);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern uint GetMappedFileName(IntPtr m_hProcess, IntPtr lpv, StringBuilder lpFilename, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            FileMapAccess dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap);

        [DllImport("kernel32.dll")]
        public static extern uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            FileMapProtection flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPTStr)]string lpName);
    }
}
