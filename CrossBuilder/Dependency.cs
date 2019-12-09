using CrossBuilder.Deb;

namespace CrossBuilder
{
    public class Dependency
    {
        public IndividualDependency[] OrList;

        private static readonly DebVersionComparer DebVersionComparer = new DebVersionComparer();

        public Dependency(params IndividualDependency[] orList)
        {
            OrList = orList;
        }

        public class IndividualDependency
        {
            public string Package;
            public Comparer Comparer;
            public string Version;
        }

        public bool SatisfiesDependencyRequirement(Package otherPackage)
        {
            foreach (var package in OrList)
            {
                if (package.Package != otherPackage.PackageName)
                {
                    return false;
                }

                var comparison = DebVersionComparer.Compare(otherPackage.Version, package.Version);

                switch (package.Comparer)
                {
                    case Comparer.LessEq:
                        return comparison <= 0;
                    case Comparer.GreaterEq:
                        return comparison >= 0;
                    case Comparer.Less:
                        return comparison < 0;
                    case Comparer.Greater:
                        return comparison > 0;
                    case Comparer.Equals:
                        return comparison == 0;
                    case Comparer.NotEquals:
                        return comparison != 0;
                    case Comparer.NoOp:
                    default:
                        return true;
                }
            }

            return false;
        }
    }
}
