using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using PatchKit.API.Data;
using PatchKit.Unity.API;
using PatchKit.Unity.AssetBundles.Web;
using PatchKit.Unity.Common;
using UnityEngine;

namespace PatchKit.Unity.AssetBundles
{
    public class PatchKitAssetBundle : IEnumerator
    {
        private static string DownloadDirectoryPath
        {
            get { return Path.Combine(Application.persistentDataPath, "patchkit_asset_bundle_downloads"); }
        }

        private static string CacheDirectoryPath
        {
            get { return Path.Combine(Application.persistentDataPath, "patchkit_asset_bundle_cache"); }
        }

        private static readonly Dictionary<string, object> Lock = new Dictionary<string, object>();

        private static object GetLock(string secret)
        {
            lock (Lock)
            {
                if (!Lock.ContainsKey(secret))
                {
                    Lock[secret] = new object();
                }

                return Lock[secret];
            }
        }

        public UnityEngine.AssetBundle AssetBundle { get; private set; }

        public bool IsReady { get; private set; }

        public Exception Error { get; private set; }

        public readonly string Secret;

        private readonly string _downloadPath;

        private readonly string _cachePath;

        private readonly string _cacheVersionPath;

        private PatchKitAssetBundle(string secret, bool useCache)
        {
            IsReady = false;

            Secret = secret;
            _downloadPath = Path.Combine(DownloadDirectoryPath, string.Format("{0}.assetbundle.download", secret));
            _cachePath = Path.Combine(CacheDirectoryPath, string.Format("{0}.assetbundle", secret));
            _cacheVersionPath = Path.Combine(CacheDirectoryPath, string.Format("{0}.assetbundle.version", secret));

            Dispatcher.Initialize();
            PatchKitUnity.EnsureThatAPIIsCreated();

            ThreadPool.QueueUserWorkItem(o => Load(useCache));
        }

        public PatchKitAssetBundle(string secret) : this(secret, false)
        {
        }

        public static PatchKitAssetBundle LoadFromCacheOrDownload(string secret)
        {
            return new PatchKitAssetBundle(secret, true);
        }

        private void Load(bool useCache)
        {
            try
            {
                lock (GetLock(Secret))
                {
                    if (!CheckInternetConnection())
                    {
                        if (useCache && VerifyCache(null))
                        {
                            LoadAssetBundle(_cachePath);

                            return;
                        }

                        throw new WebException("No internet connection.");
                    }

                    var assetBundleVersion = PatchKitUnity.API.GetAssetBundleLatestVersion(Secret);

                    if (useCache)
                    {
                        if (VerifyCache(assetBundleVersion))
                        {
                            LoadAssetBundle(_cachePath);

                            return;
                        }

                        ClearCache();
                    }

                    var downloader = new AssetBundleDownloader();

                    EnsureThatDirectoryIsCreated(DownloadDirectoryPath);
                    downloader.Download(Secret, _downloadPath);

                    LoadAssetBundle(DownloadDirectoryPath);

                    if (useCache)
                    {
                        EnsureThatDirectoryIsCreated(CacheDirectoryPath);
                        File.WriteAllText(_cacheVersionPath, assetBundleVersion.ToString());
                        File.Move(_downloadPath, _cachePath);
                    }
                    else
                    {
                        File.Delete(_downloadPath);
                    }
                }
            }
            catch (Exception exception)
            {
                Error = exception;                
            }
        }

        private void EnsureThatDirectoryIsCreated(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private bool VerifyCache(AssetBundleVersion? assetBundleVersion)
        {
            if (File.Exists(_cachePath) && File.Exists(_cacheVersionPath))
            {
                if (!assetBundleVersion.HasValue)
                {
                    return true;
                }

                int cacheVersion;

                if (TryLoadCacheVersion(out cacheVersion))
                {
                    if (assetBundleVersion.Value.Id == cacheVersion)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ClearCache()
        {
            File.Delete(_cachePath);
            File.Delete(_cacheVersionPath);
        }

        private bool TryLoadCacheVersion(out int version)
        {
            version = 0;
            
            try
            {
                string text = File.ReadAllText(_cacheVersionPath);
                return int.TryParse(text, out version);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return false;
        }

        private void LoadAssetBundle(string path)
        {
            Dispatcher.InvokeCoroutine(LoadAssetBundleCoroutine(path)).WaitOne();
        }

        private IEnumerator LoadAssetBundleCoroutine(string path)
        {
            var loader = UnityEngine.AssetBundle.LoadFromFileAsync(path);

            while (!loader.isDone)
            {
                yield return null;
            }

            AssetBundle = loader.assetBundle;
            IsReady = true;
        }

        private static bool CheckInternetConnection()
        {
            bool[] connection = new bool[1];

            Dispatcher.InvokeCoroutine(CheckInternetConnectionCoroutine(connection)).WaitOne();

            return connection[0];
        }

        private static IEnumerator CheckInternetConnectionCoroutine(bool[] connection)
        {
            WWW www = new WWW("http://google.com");

            yield return www;

            if (www.error != null)
            {
                connection[0] = false;
            }
            else
            {
                connection[0] = true;
            }
        }

        bool IEnumerator.MoveNext()
        {
            return !IsReady;
        }

        void IEnumerator.Reset()
        {
        }

        object IEnumerator.Current
        {
            get { return null; }
        }
    }
}

