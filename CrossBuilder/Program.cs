using CommandLine;
using CrossBuilder.Deb;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrossBuilder
{
    public class Program
    {
        private readonly ConcurrentDictionary<string, Package> PackageQueue;
        private readonly Stopwatch StopWatch = new Stopwatch();

        private readonly ILogger Logger;
        private readonly Browser Browser;

        public Program()
        {
            Logger = LogManager.GetCurrentClassLogger();
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
            StopWatch.Start();

            Directory.CreateDirectory(opts.Sysroot);

            Logger.Info($"Installing packages to '{opts.Sysroot}'");

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
                    Logger.Error($"Could not find package '{packageName}'");
                    continue;
                }

                await RecursivelyFindDependencies(PackageQueue, Browser, basePackage);
            }

            var installCount = PackageQueue.Count;

            if (installCount == 0)
            {
                Logger.Error($"Could not find any packages matching '{string.Join(", ", opts.Packages)}'");
                return;
            }

            Logger.Info($"About to download and install {installCount} packages.");

            foreach (var package in PackageQueue.Values)
            {
                Logger.Info($"About to install {package.PackageName}...");

#pragma warning disable CS4014
                package.DownloadAndDecompress(opts.Sysroot, opts.Force, ignoreCached: false).ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        Logger.Info($"Installed {package}");
                    }
                    else
                    {
                        // TODO: Should probably retry

                        Logger.Error(t.Exception, $"Failed to install package '{package.PackageName}'");
                    }

                    PackageQueue.TryRemove(package.SHA256, out _);

                    if (PackageQueue.Count == 0)
                    {
                        DoneInstalling(opts.Packages, installCount);
                    }
                });
#pragma warning restore CS4014
            }

            while (true) { }
        }

        private void DoneInstalling(IEnumerable<string> packages, int installCount)
        {
            StopWatch.Stop();

            var ts = StopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}.{2:00}",
                ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            Logger.Info($"Finished installing {string.Join(", ", packages)}");
            Logger.Info($"Total time to install {installCount} packages: {elapsedTime}");

            Environment.Exit(0);
        }

        private async Task RecursivelyFindDependencies(ConcurrentDictionary<string, Package> packageQueue, Browser browser, Package package)
        {
            if (packageQueue.ContainsKey(package.SHA256))
            {
                return;
            }

            packageQueue.TryAdd(package.SHA256, package);

            foreach (var dep in package.GetDependencies())
            {
                var depPackage = browser.FindPackage(dep);

                if (depPackage == null)
                {
                    Logger.Warn($"Could not resolve dependency: {dep.OrList[0].Package} ({dep.OrList[0].Package})");
                    continue;
                }

                await RecursivelyFindDependencies(packageQueue, browser, depPackage);
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
                    Logger.Warn($"Could not find package '{packageName}'");
                    continue;
                }

                await package.Uninstall(opts.Sysroot);
            }
        }

        public async Task Info(InfoOptions opts)
        {
            Logger.Info($"Information about '{opts.FileOrPackageName}':");

            var elfReader = new ElfReader();

            if (File.Exists(opts.FileOrPackageName) && elfReader.TryProcessElfFile(opts.FileOrPackageName, out var soName, out var depends))
            {
                Logger.Info($"SoName: {soName}");
                Logger.Info($"Depends: {string.Join(", ", depends)}");

                return;
            }

            await Browser.UpdatePackageCache();

            var package = Browser.FindPackage(new Dependency(new Dependency.IndividualDependency
            {
                Package = opts.FileOrPackageName,
                Comparer = Comparer.NoOp,
                Version = ""
            }));

            if (package == null)
            {
                Logger.Error($"Could not find file or package '{opts.FileOrPackageName}'");
                return;
            }

            Logger.Info($"Package: {package}");

            var deps = new ConcurrentDictionary<string, Package>();
            await RecursivelyFindDependencies(deps, Browser, package);

            Logger.Info($"Has {deps.Count} dependencies");

            foreach (var dep in deps)
            {
                Logger.Info($"- {dep.Value}");
            }

            var files = (await package.GetFileList()).ToList();

            Logger.Info($"Installs the following {files.Count} files:");

            foreach (var file in files)
            {
                Logger.Info($"- {file}");
            }
        }

        public static async Task Main(string[] args)
        {
            var program = new Program();

            await Parser.Default.ParseArguments<InstallOptions, UninstallOptions, InfoOptions>(args)
                .WithParsed<Options>(x =>
                {
                    LogManager.GlobalThreshold = x.Verbose ? LogLevel.Debug : LogLevel.Info;
                })
                .MapResult(
                    (InstallOptions opts) => program.Install(opts),
                    (UninstallOptions opts) => program.Uninstall(opts),
                    (InfoOptions opts) => program.Info(opts),
                    errs => Task.FromResult(0));
        }

        public abstract class Options
        {
            public abstract IEnumerable<string> Packages { get; set; }

            [Option('f', "force", HelpText = "Overwrites any files with the same name.")]
            public bool Force { get; set; }

            [Option('s', "sysroot", Default = "fsNew", HelpText = "Base path to install packages to.")]
            public string Sysroot { get; set; }

            [Option('v', "verbose", HelpText = "Enables verbose logging.")]
            public bool Verbose { get; set; }
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

        [Verb("info", HelpText = "Inspects a file.")]
        public class InfoOptions
        {
            [Value(0, Required = true, HelpText = "File or package to get information about.")]
            public string FileOrPackageName { get; set; }
        }
    }
}
