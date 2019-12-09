using CrossBuilder.Deb;
using CrossBuilder.Downloaders;
using DebHelper;
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

        public Package(Repository repository)
        {
            Repository = repository;
            downloader = new HttpRemoteDownloader();
        }

        public async Task DownloadAndDecompress()
        {
            var relativePath = "packages/" + Filename;

            if (IsCached(relativePath) && false)
            {
                // TODO: Bust cache if it's been too long or hash doesn't match now
                return;
            }

            // Example url is "http://ftp.debian.org/debian" / "pool/main/g/glibc/libc-bin_2.29-3_armhf.deb"
            var fileStream = downloader.DownloadFile(Repository.RepoUrl + "/" + Filename);

            var cachedFilePath = await CacheFile(relativePath, fileStream);
            var cachedExtractionDir = CacheDirectory("debOut/" + Filename, true);

            var reader = new DebReader(cachedFilePath);

            reader.Decompress(cachedExtractionDir, "fs");
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
                        depSegment.Comparer = Comparer.NoOp;
                        depSegment.Version = string.Empty;
                        depSegment.Package = dependencyStr;
                    }

                    dependency.OrList[i] = depSegment;
                }

                dependencies.Add(dependency);
            }

            return dependencies;
        }
    }
}
