using CrossBuilder.Deb;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossBuilder
{
    public class Browser
    {
        private readonly string Dist;
        private readonly string Architecture;

        private IList<Repository> Repositories;
        private List<Package> Packages = new List<Package>();

        private static readonly DebPackageComparer DebPackageComparer = new DebPackageComparer(new DebVersionComparer());

        public Browser(string dist, string architecture)
        {
            Dist = dist;
            Architecture = architecture;
        }

        public void SetRepos(IList<Repository> repositories)
        {
            Repositories = repositories;
        }

        public async Task UpdatePackageCache()
        {
            foreach (var repo in Repositories)
            {
                Packages.AddRange(await repo.GetPackageIndex());
            }
        }

        public Package FindPackage(Dependency dependency)
        {
            var foundPackages = Packages
                .Where(x => dependency.SatisfiesDependencyRequirement(x))
                .ToList();

            foundPackages.Sort((x, y) => DebPackageComparer.Compare(x, y));

            return foundPackages.ElementAtOrDefault(0);
        }
    }
}
