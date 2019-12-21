using DebHelper.Implementation;
using SharpCompress.Common;
using SharpCompress.Readers;
using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DebHelper
{
    public class DebReader
    {
        public const int SignatureLength = 8;
        public const string Signature = "!<arch>";
        protected readonly byte[] bytes;
        protected readonly string signature;
        protected readonly InnerFile debianBinary;
        protected readonly InnerFile control;
        protected readonly InnerFile data;

        public DebReader(string path)
        {
            bytes = File.ReadAllBytes(path);
            signature = bytes.Read(0, 8).ConvertToString();
            debianBinary = new InnerFile(bytes, 8);
            control = new InnerFile(bytes, 8 + debianBinary.Length);
            data = new InnerFile(bytes, 8 + debianBinary.Length + control.Length);
        }

        public void DecompressMisc(string outFolder)
        {
            var length = control.Identifer.IndexOf('.');
            outFolder = Path.Combine(outFolder, control.Identifer.Substring(0, length));

            DecompressArchive(outFolder, control);
        }

        public void DecompressData(string outFolder, Action<string> onFileDecompressed)
        {
            DecompressArchive(outFolder, data, onFileDecompressed);
        }

        public IEnumerable<string> GetDataFileList()
        {
            using var memoryStream = new MemoryStream(data.Content);
            using var reader = ReaderFactory.Open(memoryStream);

            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    yield return reader.Entry.Key;
                }
            }
        }

        private void DecompressArchive(string outFolder, InnerFile innerFile, Action<string> onFileDecompressed = null)
        {
            Directory.CreateDirectory(outFolder);

            using var memoryStream = new MemoryStream(innerFile.Content);
            using var reader = ReaderFactory.Open(memoryStream);

            while (reader.MoveToNextEntry())
            {
                var outFile = outFolder + Path.DirectorySeparatorChar + reader.Entry.Key;

                reader.WriteEntryToDirectory(outFolder, new ExtractionOptions
                {
                    WriteSymbolicLink = (symlink, orig) =>
                    {
                        try
                        {
                            var origFile = new FileInfo(new FileInfo(symlink).Directory.FullName + Path.DirectorySeparatorChar + orig);
                            origFile.CreateSymbolicLink(symlink, false);
                        }
                        catch (COMException e)
                        {
                            // HResult of -2147024896 is OK
                            if (e.HResult != -2147024896) throw e;
                        }
                    },
                    ExtractFullPath = true,
                    Overwrite = true
                });

                if (!reader.Entry.IsDirectory && reader.Entry.Size > 0)
                {
                    if (reader.Entry.LastModifiedTime.HasValue)
                    {
                        File.SetLastWriteTimeUtc(outFile, reader.Entry.LastModifiedTime.Value);
                    }

                    onFileDecompressed?.Invoke(outFile);
                }
            }
        }
    }
}
