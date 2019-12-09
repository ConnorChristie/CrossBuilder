using DebHelper.Implementation;
using SharpCompress.Common;
using SharpCompress.Readers;
using SymbolicLinkSupport;
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

        public void Decompress(string outFolder, string dataOut = null)
        {
            File.WriteAllBytes(Path.Combine(outFolder, debianBinary.Identifer), debianBinary.Content);
            DecompressArchive(outFolder, control);
            DecompressArchive(dataOut ?? outFolder, data);
        }

        private void DecompressArchive(string outFolder, InnerFile innerFile)
        {
            var length = innerFile.Identifer.IndexOf('.');
            var str = Path.Combine(outFolder, innerFile.Identifer.Substring(0, length));
            Directory.CreateDirectory(str);

            using var memoryStream = new MemoryStream(innerFile.Content);
            using (var reader = ReaderFactory.Open(memoryStream))
            {
                while (reader.MoveToNextEntry())
                {
                    reader.WriteEntryToDirectory(str, new ExtractionOptions()
                    {
                        WriteSymbolicLink = (source, dst) =>
                        {
                            try
                            {
                                var file = new FileInfo(source);
                                file.CreateSymbolicLink(dst, true);
                            }
                            catch (COMException e)
                            {
                                // HResult -2147024896 is OK
                                if (e.HResult != -2147024896) throw e;
                            }
                        },
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
    }
}
