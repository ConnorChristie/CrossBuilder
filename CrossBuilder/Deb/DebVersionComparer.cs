using System.Collections.Generic;

namespace CrossBuilder.Deb
{
    // Logic taken from APT
    // https://github.com/chaos/apt/blob/master/apt/apt-pkg/deb/debversion.cc
    public class DebVersionComparer : IComparer<string>
    {
        public int Compare(string a, string b)
        {
            a.SplitIntoTwo(':', out var aEpoch, out var aRest);
            b.SplitIntoTwo(':', out var bEpoch, out var bRest);

            // Compare the epoch
            var res = CompareVersion(aEpoch, bEpoch);
            if (res != 0)
                return res;

            a.SplitIntoTwo(':', out var aMain, out var aDeb);
            b.SplitIntoTwo(':', out var bMain, out var bDeb);

            // Compare the main version
            res = CompareVersion(aMain, bMain);
            if (res != 0)
                return res;

            // Compare the debian version
            return CompareVersion(aDeb, bDeb);
        }

        private static int Order(char? x)
        {
            if (!x.HasValue)
                return 0;

            if (char.IsDigit(x.Value))
                return 0;

            if (char.IsLetter(x.Value))
                return x.Value;

            if (x == '~')
                return -1;

            if (x.Value == 0)
                return 0;

            return x.Value + 256;
        }

        private static int CompareVersion(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(a))
            {
                if (b.TryGetValue(0) == '~') return 1;
                return -1;
            }
            if (string.IsNullOrEmpty(b))
            {
                if (a.TryGetValue(0) == '~') return -1;
                return 1;
            }

            var pA = 0;
            var pB = 0;
            while (pA < a.Length || pB < b.Length)
            {
                var firstDiff = 0;
                while ((pA < a.Length && !char.IsDigit(a[pA])) || (pB < b.Length && !char.IsDigit(b[pB])))
                {
                    var ac = Order(a.TryGetValue(pA));
                    var bc = Order(b.TryGetValue(pB));

                    if (ac != bc)
                    {
                        return ac - bc;
                    }
                    pA++;
                    pB++;
                }
                while (a.TryGetValue(pA) == '0')
                {
                    pA++;
                }
                while (b.TryGetValue(pB) == '0')
                {
                    pB++;
                }
                while ((pA < a.Length && char.IsDigit(a[pA])) && (pB < b.Length && char.IsDigit(b[pB])))
                {
                    if (firstDiff == 0)
                    {
                        firstDiff = a[pA] - b[pB];
                    }
                    pA++;
                    pB++;
                }
                if (pA < a.Length && char.IsDigit(a[pA]))
                {
                    return 1;
                }
                if (pB < b.Length && char.IsDigit(b[pB]))
                {
                    return -1;
                }
                if (firstDiff != 0)
                {
                    return firstDiff;
                }
            }
            return 0;
        }
    }
}
