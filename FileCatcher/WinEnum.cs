using System;

namespace FileCatcher
{
    public class WinEnum
    {
        public const int PROCESS_WM_READ = 0x0010;
        public const int PROCESS_DUP_HANDLE = 0x0040;

        public const int SYSTEMHANDLEINFORMATION = 16;

        public const int MAX_PATH = 260;

        public enum FileInformationClass
        {
            FileBasicInfo,
            FileStandardInfo,
            FileNameInfo,
            FileStreamInfo,
            FileCompressionInfo,
            FileAttributeTagInfo,
            FileIdBothDirectoryInfo,
            FileIdBothDirectoryRestartInfo,
            FileRemoteProtocolInfo,
            FileFullDirectoryInfo,
            FileFullDirectoryRestartInfo,
            FileStorageInfo,
            FileAlignmentInfo,
            FileIdInfo,
            FileIdExtdDirectoryInfo,
            FileIdExtdDirectoryRestartInfo,
        }

        [Flags]
        public enum FileMapProtection : uint
        {
            PageReadonly = 0x02,
            PageReadWrite = 0x04,
            PageWriteCopy = 0x08,
            PageExecuteRead = 0x20,
            PageExecuteReadWrite = 0x40,
            SectionCommit = 0x8000000,
            SectionImage = 0x1000000,
            SectionNoCache = 0x10000000,
            SectionReserve = 0x4000000,
        }

        [Flags]
        public enum FileMapAccess : uint
        {
            FileMapCopy = 0x0001,
            FileMapWrite = 0x0002,
            FileMapRead = 0x0004,
            FileMapAllAccess = 0x001f,
            fileMapExecute = 0x0020,
        }
    }
}
