using System.Collections.Generic;

namespace CrossBuilder.Deb
{
    public class DebPackageComparer : IComparer<Package>
    {
        private readonly DebVersionComparer debVersionComparer;

        public DebPackageComparer(DebVersionComparer debVersionComparer)
        {
            this.debVersionComparer = debVersionComparer;
        }

        public int Compare(Package a, Package b)
        {
            return debVersionComparer.Compare(a.Version, b.Version);
        }
    }
}
