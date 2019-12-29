using SharpCompress.Common;
using SharpCompress.Readers;
using System.IO;

namespace DebHelper
{
    public static class CustomExtractionMethods
    {
        public static bool WriteEntryToDirectoryWithFeedback(
            this IReader reader, string destinationDirectory,
            ExtractionOptions options)
        {
            string destinationFileName;
            var file = Path.GetFileName(reader.Entry.Key);
            var fullDestinationDirectoryPath = Path.GetFullPath(destinationDirectory);

            options ??= new ExtractionOptions()
            {
                Overwrite = true
            };

            if (options.ExtractFullPath)
            {
                var folder = Path.GetDirectoryName(reader.Entry.Key);
                var destdir = Path.GetFullPath(Path.Combine(fullDestinationDirectoryPath, folder));

                if (!Directory.Exists(destdir))
                {
                    if (!destdir.StartsWith(fullDestinationDirectoryPath))
                    {
                        throw new ExtractionException("Entry is trying to create a directory outside of the destination directory.");
                    }

                    Directory.CreateDirectory(destdir);
                }

                destinationFileName = Path.Combine(destdir, file);
            }
            else
            {
                destinationFileName = Path.Combine(fullDestinationDirectoryPath, file);
            }

            if (!reader.Entry.IsDirectory)
            {
                destinationFileName = Path.GetFullPath(destinationFileName);

                if (!destinationFileName.StartsWith(fullDestinationDirectoryPath))
                {
                    throw new ExtractionException("Entry is trying to write a file outside of the destination directory.");
                }

                return reader.WriteEntryToFileWithFeedback(destinationFileName, options);
            }
            else if (options.ExtractFullPath && !Directory.Exists(destinationFileName))
            {
                Directory.CreateDirectory(destinationFileName);
            }

            return false;
        }

        public static bool WriteEntryToFileWithFeedback(
            this IReader reader, string destinationFileName,
            ExtractionOptions options = null)
        {
            if (reader.Entry.LinkTarget != null)
            {
                if (null == options.WriteSymbolicLink)
                {
                    throw new ExtractionException("Entry is a symbolic link but ExtractionOptions.WriteSymbolicLink delegate is null");
                }

                options.WriteSymbolicLink(destinationFileName, reader.Entry.LinkTarget);

                return false;
            }
            else
            {
                var fm = FileMode.Create;
                options ??= new ExtractionOptions
                {
                    Overwrite = true
                };

                if (!options.Overwrite && File.Exists(destinationFileName))
                {
                    return false;
                }

                using (FileStream fs = File.Open(destinationFileName, fm))
                {
                    reader.WriteEntryTo(fs);
                }

                reader.Entry.PreserveExtractionOptions(destinationFileName, options);

                return true;
            }
        }

        public static void PreserveExtractionOptions(
            this IEntry entry, string destinationFileName,
            ExtractionOptions options)
        {
            if (options.PreserveFileTime || options.PreserveAttributes)
            {
                FileInfo nf = new FileInfo(destinationFileName);
                if (!nf.Exists)
                {
                    return;
                }

                // update file time to original packed time
                if (options.PreserveFileTime)
                {
                    if (entry.CreatedTime.HasValue)
                    {
                        nf.CreationTime = entry.CreatedTime.Value;
                    }

                    if (entry.LastModifiedTime.HasValue)
                    {
                        nf.LastWriteTime = entry.LastModifiedTime.Value;
                    }

                    if (entry.LastAccessedTime.HasValue)
                    {
                        nf.LastAccessTime = entry.LastAccessedTime.Value;
                    }
                }

                if (options.PreserveAttributes)
                {
                    if (entry.Attrib.HasValue)
                    {
                        nf.Attributes = (FileAttributes)System.Enum.ToObject(typeof(FileAttributes), entry.Attrib.Value);
                    }
                }
            }
        }
    }
}
