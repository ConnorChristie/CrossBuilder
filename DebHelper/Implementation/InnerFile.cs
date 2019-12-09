namespace DebHelper.Implementation
{
    public class InnerFile
    {
        public const int IdentiferLength = 16;
        public const int ModificationTimeStampLength = 12;
        public const int OwnerIdStart = 28;
        public const int OwnerIdLength = 6;
        public const int GroupIdStart = 34;
        public const int GroupIdLength = 6;
        public const int FileModeStart = 40;
        public const int FileModeLength = 8;
        public const int FileSizeStart = 48;
        public const int FileSizeLength = 10;
        public const int EndCharLength = 2;
        public const int FileContentStart = 60;
        private readonly bool hasExtraBit = false;

        public InnerFile(byte[] chunk, int start)
        {
            Identifer = chunk.ReadString(start, 16);
            ModificationTimeStamp = chunk.ReadInt(start + 16, 12);
            OwnerId = chunk.ReadInt(start + 28, 6);
            GroupId = chunk.ReadInt(start + 34, 6);
            FileMode = chunk.ReadInt(start + 40, 8);
            ContentLength = chunk.ReadInt(start + 48, 10);

            hasExtraBit = false;

            // Sometimes there is an extra new line so we need to account for that
            if (chunk.Read(start + 60, 1)[0] == (byte)'\n')
                hasExtraBit = true;

            Content = chunk.Read(start + 60 + (hasExtraBit ? 1 : 0), ContentLength);
        }

        public string Identifer { get; }

        public int ModificationTimeStamp { get; }

        public int OwnerId { get; }

        public int GroupId { get; }

        public int FileMode { get; }

        public int ContentLength { get; }

        public byte[] Content { get; }

        public int Length
        {
            get
            {
                return 60 + Content.Length + (hasExtraBit ? 1 : 0);
            }
        }
    }
}
