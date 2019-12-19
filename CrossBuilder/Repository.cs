using CrossBuilder.Downloaders;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.LZMA;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrossBuilder
{
    public class Repository : Cacheable
    {
        public string RepoUrl { get; set; }
        public string Dist { get; set; }
        public string Component { get; set; }
        public string Architecture { get; set; }

        private readonly IRemoteDownloader downloader;

        public Repository()
        {
            downloader = new HttpRemoteDownloader();
        }

        public async Task<IList<Package>> GetPackageIndex()
        {
            var (url, friendlyPath) = GetFullyQualifiedRepoPath();
            var remoteIndexUri = url + "/Packages";

            if (!IsCached(friendlyPath + "/Packages", out var cachedFilePath))
            {
                using var fileStream = GetRemoteFileMaybeCompressed(remoteIndexUri);
                cachedFilePath = await CacheFile(friendlyPath + "/Packages", fileStream);
            }

            var packageFileLines = await File.ReadAllLinesAsync(cachedFilePath);

            var packages = new List<Package>();
            var tempPackage = new Package(this);
            var readingDescription = false;

            foreach (var line in packageFileLines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    // TODO: Verify package has mandatory values
                    packages.Add(tempPackage);

                    tempPackage = new Package(this);
                    readingDescription = false;

                    continue;
                }

                if (readingDescription && line.ContainsAndRetreive(" ", out string desc))
                {
                    tempPackage.Description += $"\n {desc}";
                    continue;
                }

                if (tempPackage.PackageName == null && line.ContainsAndRetreive("Package: ", out tempPackage.PackageName)) continue;
                if (tempPackage.Version == null && line.ContainsAndRetreive("Version: ", out tempPackage.Version)) continue;
                if (tempPackage.Architecture == null && line.ContainsAndRetreive("Architecture: ", out tempPackage.Architecture)) continue;
                if (tempPackage.Maintainer == null && line.ContainsAndRetreive("Maintainer: ", out tempPackage.Maintainer)) continue;
                if (tempPackage.Depends == null && line.ContainsAndRetreive("Depends: ", out tempPackage.Depends)) continue;
                if (tempPackage.InstalledSize == null && line.ContainsAndRetreive("Installed-Size: ", out tempPackage.InstalledSize)) continue;
                if (tempPackage.Priority == null && line.ContainsAndRetreive("Priority: ", out tempPackage.Priority)) continue;
                if (tempPackage.Section == null && line.ContainsAndRetreive("Section: ", out tempPackage.Section)) continue;
                if (tempPackage.Filename == null && line.ContainsAndRetreive("Filename: ", out tempPackage.Filename)) continue;
                if (tempPackage.Size == null && line.ContainsAndRetreive("Size: ", out tempPackage.Size)) continue;
                if (tempPackage.SHA256 == null && line.ContainsAndRetreive("SHA256: ", out tempPackage.SHA256)) continue;
                if (tempPackage.SHA1 == null && line.ContainsAndRetreive("SHA1: ", out tempPackage.SHA1)) continue;
                if (tempPackage.MD5sum == null && line.ContainsAndRetreive("MD5sum: ", out tempPackage.MD5sum)) continue;
                if (tempPackage.Description == null && line.ContainsAndRetreive("Description: ", out tempPackage.Description))
                {
                    readingDescription = true;
                    continue;
                }
            }

            return packages;
        }

        private (string url, string friendlyPath) GetFullyQualifiedRepoPath()
        {
            string fqr(string repoUrl) { return $"{repoUrl}/dists/{Dist}/{Component}/binary-{Architecture}"; }

            return (
                fqr(RepoUrl),
                fqr(Regex.Replace(RepoUrl, "[_:/.]", "_"))
            );
        }

        private Stream GetRemoteFileMaybeCompressed(string path)
        {
            try { return new GZipStream(downloader.DownloadFile($"{path}.gz"), CompressionMode.Decompress); }
            catch { Console.WriteLine("Didn't find a .gz compressed file."); }

            try { return new BZip2Stream(downloader.DownloadFile($"{path}.bz2"), CompressionMode.Decompress, true); }
            catch { Console.WriteLine("Didn't find a .bz2 compressed file."); }

            try { return new LZipStream(downloader.DownloadFile($"{path}.lzma"), CompressionMode.Decompress); }
            catch { Console.WriteLine("Didn't find a .lzma compressed file."); }

            try { return new XZStream(downloader.DownloadFile($"{path}.xz")); }
            catch { Console.WriteLine("Didn't find a .xz compressed file."); }

            try { return downloader.DownloadFile(path); }
            catch { Console.WriteLine("Didn't find an uncompressed file."); }

            throw new Exception("Failed to find any matching files (checked for xz, gz, bz2, lzma, and uncompressed).");
        }
    }
}
