using CrossBuilder;
using CrossBuilder.Deb;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossBuilder2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var browser = new Browser("stretch", "armhf");

            browser.SetRepos(new List<Repository>
            {
                new Repository
                {
                    RepoUrl = "http://deb.debian.org/debian",
                    Dist = "stretch",
                    Component = "main",
                    Architecture = "armhf"
                },
                new Repository
                {
                    RepoUrl = "http://deb.debian.org/debian",
                    Dist = "stretch",
                    Component = "contrib",
                    Architecture = "armhf"
                },
                new Repository
                {
                    RepoUrl = "http://deb.debian.org/debian",
                    Dist = "stretch",
                    Component = "non-free",
                    Architecture = "armhf"
                },
                new Repository
                {
                    RepoUrl = "http://repos.rcn-ee.com/debian",
                    Dist = "stretch",
                    Component = "main",
                    Architecture = "armhf"
                }
            });

            await browser.UpdatePackageCache();

            var pack = browser.FindPackage(new Dependency(new Dependency.IndividualDependency
            {
                Package = "libtiopencl1",
                Comparer = Comparer.NoOp,
                Version = ""
            }));

            await pack.DownloadAndDecompress();

            //var index = new Dictionary<Repo, IList<PackageIndex>>();

            //foreach (var repo in repos)
            //{
            //    index[repo] = await GetPackagesIndexFromCacheOrDownload(repo);
            //}

            //var (foundPackageRepo, foundPackage) = FindMatchingPackage(index, new PackageDependency
            //{
            //    Package = "libtiopencl1",
            //    Comparer = Comparer.NoOp,
            //    Version = ""
            //});

            //await Recurse(index, foundPackageRepo, foundPackage);

            //string[] debs =
            //{
            //    "libtiopencl1_01.01.17.01-git20181129.0-0rcnee0_stretch+20190103_armhf",
            //    "ti-opencl_01.01.17.01-git20181129.0-0rcnee0_stretch+20190103_armhf",
            //    "ti-tidl_01.02.02-bb.org-0.2-0rcnee3_stretch+20190924_armhf"
            //};

            //foreach (var deb in debs)
            //{
            //    var debOut = $"debOut/{deb}";
            //    var reader = new DebReader($@"D:\Git\CrossBuilder\CrossBuilder\debs\{deb}.deb");

            //    Directory.CreateDirectory(debOut);

            //    reader.Decompress(debOut, "debOut/fs");
            //}
        }

        //private static BlockingCollection<string> AlreadyResolved = new BlockingCollection<string>();

        //private static async Task Recurse(IDictionary<Repo, IList<PackageIndex>> index, Repo repo, PackageIndex package, int level = 0)
        //{
        //    if (AlreadyResolved.Contains(package.SHA256))
        //    {
        //        return;
        //    }

        //    Console.WriteLine($"Package: {package.Package}, Current level: {level}");

        //    var dependencies = package
        //        .GetDependencies()
        //        .Select(x => FindMatchingPackage(index, x));

        //    try
        //    {
        //        await DownloadAndDecompress(repo, package);

        //        AlreadyResolved.Add(package.SHA256);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Failed to download package {package.Package}: {e.Message}");
        //    }

        //    foreach (var dep in dependencies)
        //    {
        //        await Recurse(index, dep.Item1, dep.Item2, level + 1);
        //    }
        //}

        //private static async Task DownloadAndDecompress(Repo repo, PackageIndex package)
        //{
        //    var packageUri = $"{repo.RepoUrl}/{package.Filename}";
        //    var packageCacheFile = $"{GetRepoCachePath(repo)}/{package.Filename}";

        //    var packageFile = await DownloadOrGetCachedFile(packageUri, packageCacheFile);

        //    var debOut = $"debOut/{package.Filename}";
        //    var reader = new DebReader(packageFile);

        //    Directory.CreateDirectory(debOut);

        //    reader.Decompress(debOut, "debOut/fs");
        //}
    }
}
