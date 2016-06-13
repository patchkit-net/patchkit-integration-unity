using System;
using System.IO;
using System.Net;
using Assets.Plugins.PatchKit.Scripts.AssetBundles.Web;
using PatchKit.Unity.API;

namespace PatchKit.Unity.AssetBundles.Web
{
    internal class AssetBundleDownloader : IAssetBundleDownloader
    {
        public void Download(string secret, string destinationPath)
        {
            var contentUrls = PatchKitUnity.API.GetAssetBundleContentUrls(secret);

            foreach (var contentUrl in contentUrls)
            {
                try
                {
                    var httpRequest = (HttpWebRequest)WebRequest.Create(contentUrl.Url);

                    using (var response = (HttpWebResponse)httpRequest.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (var fileStream = File.Create(destinationPath))
                                {
                                    const int downloadBufferSize = 2048;

                                    byte[] downloadBuffer = new byte[downloadBufferSize];

                                    int downloadedBytes;

                                    while ((downloadedBytes = responseStream.Read(downloadBuffer, 0, downloadBufferSize)) != 0)
                                    {
                                        fileStream.Write(downloadBuffer, 0, downloadedBytes);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }

            throw new WebException(string.Format("Missing content urls for asset bundle with secret {0}.", secret));
        }
    }
}
