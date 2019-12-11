using CrossBuilder;
using CrossBuilder.Deb;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossBuilder2
{
    public class Program
    {
        private readonly ConcurrentDictionary<string, Package> PackageQueue;

        public Program()
        {
            PackageQueue = new ConcurrentDictionary<string, Package>();
        }

        public async Task Run()
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
                Package = "samba",
                Comparer = Comparer.NoOp,
                Version = ""
            }));

            await RecurseDependencies(browser, pack);

            Console.WriteLine($"About to download and install {PackageQueue.Count} packages");

            foreach (var package in PackageQueue.Values)
            {
                Console.WriteLine($"Downloading {package.PackageName}...");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                package.DownloadAndDecompress().ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        Console.WriteLine($"Downloaded and installed {package.PackageName} ({package.Version}) [{package.Architecture}]");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to install package {package.PackageName}");
                        Console.WriteLine(t.Exception);
                    }

                    PackageQueue.TryRemove(package.SHA256, out _);

                    if (PackageQueue.Count == 0)
                    {
                        Console.WriteLine("We are done!");
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            while (true) { }
        }

        private async Task RecurseDependencies(Browser browser, Package package)
        {
            if (PackageQueue.ContainsKey(package.SHA256))
            {
                return;
            }

            PackageQueue.TryAdd(package.SHA256, package);

            foreach (var dep in package.GetDependencies())
            {
                var depPackage = browser.FindPackage(dep);

                if (depPackage == null)
                {
                    Console.WriteLine($"Could not resolve dependency: {dep.OrList[0].Package} ({dep.OrList[0].Package})");

                    continue;
                }

                await RecurseDependencies(browser, depPackage);
            }
        }

        public static async Task Main(string[] args)
        {
            await new Program().Run();
        }
    }
}
