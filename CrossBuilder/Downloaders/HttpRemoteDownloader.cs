using System;
using System.IO;
using System.Net;

namespace CrossBuilder.Downloaders
{
    public class HttpRemoteDownloader : IRemoteDownloader
    {
        private readonly WebClient webClient;

        public HttpRemoteDownloader()
        {
            webClient = new WebClient();
        }

        public Stream DownloadFile(string uri)
        {
            return webClient.OpenRead(new Uri(uri));
        }
    }
}
