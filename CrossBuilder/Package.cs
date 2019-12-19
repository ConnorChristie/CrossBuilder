using CrossBuilder.Deb;
using CrossBuilder.Downloaders;
using DebHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CrossBuilder
{
    public class Package : Cacheable
    {
        // === Fields part of the package index ===
        public string PackageName;
        public string Version;
        public string Architecture;
        public string Maintainer;
        public string Depends;
        public int? InstalledSize;
        public string Priority;
        public string Section;
        public string Filename;
        public int? Size;
        public string SHA256;
        public string SHA1;
        public string MD5sum;
        public string Description;
        // ========================================

        public Repository Repository;

        private readonly IRemoteDownloader downloader;
        private readonly ElfReader elfReader;

        public Package(Repository repository)
        {
            Repository = repository;
            downloader = new HttpRemoteDownloader();
            elfReader = new ElfReader();
        }

        public async Task DownloadAndDecompress(bool ignoreCached = false)
        {
            var debCachePath = "packages/" + Filename;
            var miscCachePath = "debOut/" + Filename;
            var fsPath = "fs";

            if (!ignoreCached && IsCached(debCachePath))
            {
                // TODO: Bust cache if it's been too long or hash doesn't match anymore
                return;
            }

            // Example url is "http://ftp.debian.org/debian" / "pool/main/g/glibc/libc-bin_2.29-3_armhf.deb"
            var fileStream = downloader.DownloadFile(Repository.RepoUrl + "/" + Filename);

            var cachedDeb = await CacheFile(debCachePath, fileStream);
            var cachedMiscDir = CacheDirectory(miscCachePath, getParent: true);

            var reader = new DebReader(cachedDeb);

            reader.DecompressMisc(cachedMiscDir);
            reader.DecompressData(fsPath, ProcessSharedLibrary);
        }

        private void ProcessSharedLibrary(string filePath)
        {
            var sdf = CompareLibVersions("libc.so.1.2.3", "libc.so.1.2.9");
            var sdf1 = CompareLibVersions("libc.so.1.2.3", "libc.so.1.1.3");
            var sdf2 = CompareLibVersions("libc.so.1", "libc.so.1.2");

            var sdf3 = CompareLibVersions("libc.so.1.3", "libc.so.1.2");
            var sdf4 = CompareLibVersions("libc.so.1.3.99", "libc.so.1.2.12");

            if (filePath.Contains(".so") && elfReader.TryProcessElfFile(filePath, out var soName, out var depends))
            {
                // Only create a symlink if the SO name differs from the current file name
                if (soName != null && soName != Path.GetFileName(filePath))
                {
                    var f = new DirectoryInfo(filePath);

                    Console.WriteLine($"{soName} -> {Path.GetFileName(filePath)} : {f.Parent.FullName}");
                }
            }
        }

        /// <summary>
        /// Returns a negative value if <paramref name="b"/> is greater than <paramref name="a"/>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int CompareLibVersions(string a, string b)
        {
            var p1 = 0;
            var p2 = 0;

            while (p1 < a.Length)
            {
                if (a.GetValueOrDefault(p1) >= '0' && a.GetValueOrDefault(p1) <= '9')
                {
                    if (b.GetValueOrDefault(p2) >= '0' && b.GetValueOrDefault(p2) <= '9')
                    {
                        /* Must compare this numerically.  */
                        var val1 = a.GetValueOrDefault(p1++) - '0';
                        var val2 = b.GetValueOrDefault(p2++) - '0';

                        while (a.GetValueOrDefault(p1) >= '0' && a.GetValueOrDefault(p1) <= '9')
                            val1 = val1 * 10 + a.GetValueOrDefault(p1++) - '0';
                        while (b.TryGetValue(p2) >= '0' && b.GetValueOrDefault(p2) <= '9')
                            val2 = val2 * 10 + b.GetValueOrDefault(p2++) - '0';

                        if (val1 != val2)
                        {
                            return val1 - val2;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (b.GetValueOrDefault(p2) >= '0' && b.GetValueOrDefault(p2) <= '9')
                {
                    return -1;
                }
                else if (a.GetValueOrDefault(p1) != b.GetValueOrDefault(p2))
                {
                    return a.GetValueOrDefault(p1) - b.GetValueOrDefault(p2);
                }
                else
                {
                    ++p1;
                    ++p2;
                }
            }

            return a.GetValueOrDefault(p1) - b.GetValueOrDefault(p2);
        }

        public IList<Dependency> GetDependencies()
        {
            var dependencies = new List<Dependency>();

            if (string.IsNullOrEmpty(Depends))
            {
                return dependencies;
            }

            var depends = Depends.Split(", ");

            foreach (var dependencyMaybeMultiple in depends)
            {
                var orPackages = dependencyMaybeMultiple.Split(" | ");
                var dependency = new Dependency(new Dependency.IndividualDependency[orPackages.Length]);

                for (var i = 0; i < dependency.OrList.Length; i++)
                {
                    var depSegment = new Dependency.IndividualDependency();
                    var dependencyStr = orPackages[i];
                    var openParenIndex = dependencyStr.IndexOf('(');

                    if (openParenIndex != -1)
                    {
                        // Minus 1 to get rid of the space before parenthesis
                        depSegment.Package = dependencyStr.Substring(0, openParenIndex - 1);

                        var versionStuff = dependencyStr.Substring(openParenIndex + 1, dependencyStr.Length - openParenIndex - 2);
                        var spaceIndex = versionStuff.IndexOf(' ');

                        if (spaceIndex != -1)
                        {
                            depSegment.Comparer = versionStuff.Substring(0, spaceIndex).ToComparisonOperator();
                            depSegment.Version = versionStuff.Substring(spaceIndex + 1, versionStuff.Length - spaceIndex - 1);
                        }
                        else
                        {
                            depSegment.Comparer = Comparer.NoOp;
                            depSegment.Version = versionStuff;
                        }
                    }
                    else
                    {
                        depSegment.Package = dependencyStr;
                        depSegment.Comparer = Comparer.NoOp;
                        depSegment.Version = string.Empty;
                    }

                    if (depSegment.Package.EndsWith(":any"))
                    {
                        Console.WriteLine($"[WARNING] Package '{depSegment.Package}' was configured for 'multi-arch' support but used the package name to symbolize that.");
                        Console.WriteLine("  Aka we are removing the ':any' from the name and matching against that instead.");

                        depSegment.Package = depSegment.Package.Replace(":any", "");
                    }

                    dependency.OrList[i] = depSegment;
                }

                dependencies.Add(dependency);
            }

            return dependencies;
        }

        public override string ToString()
        {
            return $"{PackageName} ({Version}) [{Architecture}]";
        }
    }
}
