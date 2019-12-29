using CrossBuilder.Deb;
using CrossBuilder.Downloaders;
using DebHelper;
using SymbolicLinkSupport;
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

        //private readonly string fsPath = "fsNew";

        public Package(Repository repository)
        {
            Repository = repository;
            downloader = new HttpRemoteDownloader();
            elfReader = new ElfReader();
        }

        public async Task DownloadAndDecompress(string sysroot, bool overwrite, bool ignoreCached = false)
        {
            var debCachePath = "packages" + Path.DirectorySeparatorChar + Filename;

            if (ignoreCached || !IsCached(debCachePath))
            {
                var fileStream = downloader.DownloadFile(Repository.RepoUrl + "/" + Filename);

                await CacheFile(debCachePath, fileStream);
            }

            // TODO: Bust cache if it's been too long or hash doesn't match anymore
            // TODO: do we need the control files for anything? it does have the md5sums in there

            var reader = new DebReader(GetCachedPath(debCachePath));
            reader.DecompressData(sysroot, overwrite, OnFileDecompressed);
        }

        public void Uninstall(string sysroot)
        {
            var debCachePath = "packages" + Path.DirectorySeparatorChar + Filename;

            if (!IsCached(debCachePath))
            {
                // TODO: Maybe re-download the package so we can get the list of files it extracted?
                throw new Exception($"The package '{PackageName}' is no longer cached so we can't figure out what files it laid down.");
            }

            var reader = new DebReader(GetCachedPath(debCachePath));

            foreach (var file in reader.GetDataFileList())
            {
                // TODO: Do we need to delete all the symlinks pointing to these files?

                Console.WriteLine($"Removing: {file}");

                File.Delete(sysroot + Path.DirectorySeparatorChar + file);
            }
        }

        private void OnFileDecompressed(FileInfo file)
        {
            if (file.Name.Contains(".so") && elfReader.TryProcessElfFile(file.FullName, out var soName, out var depends))
            {
                if (soName == null)
                {
                    return;
                }

                // Only create a symlink if the soname differs from the current file name
                if (soName != file.Name)
                {
                    string soSymlinkName = null;

                    // If the current path ends with "lib" then we are not inside an arch specific directory
                    if (file.Directory.FullName.EndsWith("lib"))
                    {
                        soSymlinkName = file.Directory.FullName + Path.DirectorySeparatorChar + soName;
                    }
                    else if (file.Directory.Parent.FullName.EndsWith("lib"))
                    {
                        soSymlinkName = file.Directory.Parent.FullName + Path.DirectorySeparatorChar + soName;
                    }

                    if (soSymlinkName != null)
                    {
                        var doLink = true;
                        var soSymlink = new FileInfo(soSymlinkName);

                        if (soSymlink.Exists)
                        {
                            var existingTarget = new FileInfo(soSymlink.GetSymbolicLinkTarget());

                            if (CompareLibVersions(file.Name, existingTarget.Name) > 0)
                            {
                                Console.WriteLine($"[INFO] Updating symlink '{soSymlink.Name}' from '{existingTarget.Name}' to '{file.Name}'.");

                                soSymlink.Delete();
                            }
                            else
                            {
                                // Version being installed is either the same version or a lower version than what's already installed
                                // TODO: Add override option in cases of downgrading
                                doLink = false;
                            }
                        }

                        if (doLink)
                        {
                            file.CreateSymbolicLink(soSymlinkName, false);

                            var doLibLink = true;
                            var libPath = soSymlink.Directory.FullName + Path.DirectorySeparatorChar + soName.Substring(0, soName.LastIndexOf(".so") + 3);
                            var libFile = new FileInfo(libPath);

                            if (libFile.Exists)
                            {
                                var existingTarget = new FileInfo(libFile.GetSymbolicLinkTarget());

                                if (existingTarget.Name != soSymlink.Name)
                                {
                                    Console.WriteLine($"[INFO] Updating library symlink '{libFile.Name}' from '{existingTarget.Name}' to '{soSymlink.Name}'.");

                                    libFile.Delete();
                                }
                                else
                                {
                                    doLibLink = false;
                                }
                            }

                            if (doLibLink)
                            {
                                soSymlink.CreateSymbolicLink(libPath, false);
                            }
                        }
                    }
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
