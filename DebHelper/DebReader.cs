using DebHelper.Implementation;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;

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

        public void DecompressData(string outFolder, bool overwrite, Action<FileInfo> onFileDecompressed)
        {
            DecompressArchive(outFolder, data, overwrite, onFileDecompressed);
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

        private void DecompressArchive(string outFolder, InnerFile innerFile, bool overwrite = false, Action<FileInfo> onFileDecompressed = null)
        {
            Directory.CreateDirectory(outFolder);

            using var memoryStream = new MemoryStream(innerFile.Content);
            using var reader = ReaderFactory.Open(memoryStream);

            while (reader.MoveToNextEntry())
            {
                var outFile = new FileInfo(outFolder + Path.DirectorySeparatorChar + reader.Entry.Key);

                var wasExtracted = reader.WriteEntryToDirectoryWithFeedback(outFolder, new ExtractionOptions
                {
                    WriteSymbolicLink = (symlinkName, destination) =>
                    {
                        // TODO: The ldconfig logic handles the symlink creation for so files. We might need to keep this for non-so files?
                        Console.WriteLine($"[TRACE] Skipping symlink extraction for '{new FileInfo(symlinkName).Name}'.");

                        //var symlink = new FileInfo(symlinkName);
                        //var origFile = new FileInfo(symlink.Directory.FullName + Path.DirectorySeparatorChar + destination);

                        //if (symlink.Exists)
                        //{
                        //    symlink.Delete();
                        //}

                        //origFile.CreateSymbolicLink(symlinkName, false);
                    },
                    Overwrite = overwrite,
                    ExtractFullPath = true,
                    PreserveFileTime = true
                });

                if (!overwrite && !wasExtracted && outFile.Exists)
                {
                    Console.WriteLine($"[TRACE] Skipping extraction of '{outFile.Name}' because the file already exists and overwrite is false.");
                }

                if (wasExtracted)
                {
                    onFileDecompressed?.Invoke(outFile);
                }
            }
        }
    }
}
