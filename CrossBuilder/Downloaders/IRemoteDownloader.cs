using System.IO;
using System.Threading.Tasks;

namespace CrossBuilder.Downloaders
{
    public interface IRemoteDownloader
    {
        Stream DownloadFile(string uri);
    }
}
