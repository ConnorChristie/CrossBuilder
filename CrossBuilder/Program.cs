using CommandLine;
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

        public async Task Install(InstallOptions opts)
        {
            await Browser.UpdatePackageCache();

            foreach (var packageName in opts.Packages)
            {
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
                    package.DownloadAndDecompress(ignoreCached: false).ContinueWith(t =>
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
                            Console.WriteLine($"Finished installing {basePackage}.");
                            //Environment.Exit(0);
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
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

        public async Task Uninstall(UninstallOptions opts)
        {
            await Browser.UpdatePackageCache();

            foreach (var packageName in opts.Packages)
            {
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
        }

        public static async Task Main(string[] args)
        {
            var program = new Program();

            await Parser.Default.ParseArguments<InstallOptions, UninstallOptions>(args)
                .MapResult(
                    (InstallOptions opts) => program.Install(opts),
                    (UninstallOptions opts) => program.Uninstall(opts),
                    errs => Task.FromResult(0));
        }

        public abstract class Options
        {
            public abstract IEnumerable<string> Packages { get; set; }

            [Option('f', "force", Required = false, HelpText = "Forces the package to be fully re-installed.")]
            public bool Force { get; set; }
        }

        [Verb("install", HelpText = "Installs a package.")]
        public class InstallOptions : Options
        {
            [Value(0, Required = true, HelpText = "Packages to install.")]
            public override IEnumerable<string> Packages { get; set; }
        }

        [Verb("uninstall", HelpText = "Uninstalls a package.")]
        public class UninstallOptions : Options
        {
            [Value(0, Required = true, HelpText = "Packages to uninstall.")]
            public override IEnumerable<string> Packages { get; set; }
        }
    }
}
