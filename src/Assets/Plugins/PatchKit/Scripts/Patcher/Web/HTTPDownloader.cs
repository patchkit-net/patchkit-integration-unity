﻿using PatchKit.API.Async;

namespace PatchKit.Unity.Patcher.Web
{
    public class HttpDownloader
    {
        public delegate void DownloadProgress(string fileName, float progress, float speed);

        public void DownloadFile(string url, string destinationPath, DownloadProgress onDownloadProgress, AsyncCancellationToken cancellationToken)
        {
            
        }
    }
}