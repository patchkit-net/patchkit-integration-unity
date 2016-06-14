using PatchKit.API.Async;

namespace PatchKit.Unity.Patcher.Web
{
    public class TorrentDownloader
    {
        public delegate void DownloadProgress(string fileName, float progress, float speed);

        public void DownloadFile(string torrentPath, string destinationPath, DownloadProgress onDownloadProgress, AsyncCancellationToken cancellationToken)
        {

        }
    }
}