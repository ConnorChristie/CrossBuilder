using CrossBuilder;
using CrossBuilder.Deb;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossBuilder2
{
    public class Program
    {
        private readonly ConcurrentDictionary<string, Package> PackageQueue;

        private readonly Browser Browser;

        public Program()
        {
            PackageQueue = new ConcurrentDictionary<string, Package>();

            Browser = new Browser("stretch", "armhf");
            Browser.SetRepos(new List<Repository>
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
        }

        public async Task Install(string packageName)
        {
            await Browser.UpdatePackageCache();

            var basePackage = Browser.FindPackage(new Dependency(new Dependency.IndividualDependency
            {
                Package = packageName,
                Comparer = Comparer.NoOp,
                Version = ""
            }));

            if (basePackage == null)
            {
                Console.WriteLine($"Could not find package '{packageName}'");
                return;
            }

            await RecurseDependencies(Browser, basePackage);

            Console.WriteLine($"About to download and install {PackageQueue.Count} packages");

            foreach (var package in PackageQueue.Values)
            {
                Console.WriteLine($"Downloading {package.PackageName}...");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                package.DownloadAndDecompress(ignoreCached: true).ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        Console.WriteLine($"Downloaded and installed {package}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to install package '{package.PackageName}'");
                        Console.WriteLine(t.Exception);
                    }

                    PackageQueue.TryRemove(package.SHA256, out _);

                    if (PackageQueue.Count == 0)
                    {
                        Console.WriteLine("We are done!");
                        Environment.Exit(0);
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

        public async Task Uninstall(string packageName)
        {
            await Browser.UpdatePackageCache();

            var package = Browser.FindPackage(new Dependency(new Dependency.IndividualDependency
            {
                Package = packageName,
                Comparer = Comparer.NoOp,
                Version = ""
            }));

            if (package == null)
            {
                Console.WriteLine($"Could not find package '{packageName}'");
                return;
            }

            package.Uninstall();
        }

        public static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Try running: CrossBuilder.exe <install|uninstall> <package-name>");
                return;
            }

            var program = new Program();
            var method = args[0];

            switch (method.ToLower())
            {
                case "install":
                    await program.Install(args[1]);
                    break;
                case "uninstall":
                    await program.Uninstall(args[1]);
                    break;
            }
        }
    }
}
