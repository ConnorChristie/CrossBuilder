using System;
using System.IO;
using System.Threading.Tasks;

namespace CrossBuilder
{
    public abstract class Cacheable
    {
        private static readonly string CachePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + $"/CrossBuilder/cache";

        protected async Task<string> CacheFile(string filename, Stream fileStream)
        {
            var cachePath = GetCachedPath(filename);

            Directory.CreateDirectory(Directory.GetParent(cachePath).FullName);

            using var writer = File.OpenWrite(cachePath);
            await fileStream.CopyToAsync(writer);

            return cachePath;
        }

        protected string CacheDirectory(string dirname, bool getParent = false)
        {
            var cachePath = GetCachedPath(dirname);
            var fixedCachePath = getParent ? Directory.GetParent(cachePath).FullName : cachePath;

            Directory.CreateDirectory(fixedCachePath);

            return fixedCachePath;
        }

        protected string GetCachedPath(string filename)
        {
            return CachePath + Path.DirectorySeparatorChar + filename;
        }

        protected bool IsCached(string filename)
        {
            return IsCached(filename, out _);
        }

        protected bool IsCached(string filename, out string cachedPath)
        {
            cachedPath = GetCachedPath(filename);
            return File.Exists(cachedPath);
        }

        protected void UnCache(string filename)
        {
            File.Delete(GetCachedPath(filename));
        }
    }
}
