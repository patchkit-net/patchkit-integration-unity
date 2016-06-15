using System;
using System.IO;
using System.Threading;
using PatchKit.API;
using PatchKit.API.Async;
using PatchKit.Unity.API;
using PatchKit.Unity.Common;
using PatchKit.Unity.Patcher.Application;
using PatchKit.Unity.Patcher.Utilities;
using PatchKit.Unity.Patcher.Web;
using UnityEngine;
using UnityEngine.Events;

namespace PatchKit.Unity.Patcher
{
    /// <summary>
    /// Patcher working in Unity.
    /// </summary>
    public class PatchKitUnityPatcher : MonoBehaviour
    {
        // Configuration

        public PlatformDependentString SecretKey;

        public PlatformDependentString ExecutableName;

        public ApplicationDataLocation ApplicationDataLocation;

        // Events

        public UnityEvent OnPatchingStarted;

        public UnityEvent OnPatchingProgress;

        public UnityEvent OnPatchingFinished;

        // Status

        public PatchKitUnityPatcherStatus Status
        {
            get { return _status; }
        }

        private PatchKitUnityPatcherStatus _status;

        // Variables used during patching

        private string _secretKey;

        private AsyncCancellationTokenSource _cancellationTokenSource;

        private PatchKitAPI _api;

        private ApplicationData _applicationData;

        private HttpDownloader _httpDownloader;

        private TorrentDownloader _torrentDownloader;

        private Unarchiver _unarchiver;

        private Librsync _librsync;

        private void Awake()
        {
            Dispatcher.Initialize();

            ResetStatus();
            _status.State = PatchKitUnityPatcherState.None;
        }

        public void StartPatching()
        {
            if (_status.State == PatchKitUnityPatcherState.Patching)
            {
                throw new InvalidOperationException("Patching is already started.");
            }

            _secretKey = SecretKey;

            _cancellationTokenSource = new AsyncCancellationTokenSource();

            _api = PatchKitUnity.API;

            _applicationData = new ApplicationData(ApplicationDataLocation.GetPath());

            _httpDownloader = new HttpDownloader();

            _torrentDownloader = new TorrentDownloader();

            _unarchiver = new Unarchiver();

            _librsync = new Librsync();

            _status.State = PatchKitUnityPatcherState.Patching;

            ThreadPool.QueueUserWorkItem(state =>
            {
                Debug.Log("OnPatchingStarted");
                Dispatcher.Invoke(OnPatchingStarted.Invoke);
                try
                {
                    Patch(_cancellationTokenSource.Token);
                    _status.State = PatchKitUnityPatcherState.Succeed;
                }
                catch (NoInternetConnectionException exception)
                {
                    _status.State = PatchKitUnityPatcherState.NoInternetConnection;
                    Debug.LogException(exception);
                }
                catch (Exception exception)
                {
                    _status.State = PatchKitUnityPatcherState.Failed;
                    Debug.LogException(exception);
                }
                finally
                {
                    ResetStatus();
                    Dispatcher.Invoke(OnPatchingFinished.Invoke);

                    Debug.Log(string.Format("OnPatchingFinished - {0}.", _status.State));
                }
            });
        }

        public void CancelPatching()
        {
            _cancellationTokenSource.Cancel();
        }

        public void StartApplication()
        {
            StartApplication("");
        }

        public void StartApplication(string arguments)
        {
            System.Diagnostics.Process.Start(Path.Combine(ApplicationDataLocation.GetPath(), ExecutableName), arguments);
        }

        private void Patch(AsyncCancellationToken cancellationToken)
        {
            if (!InternetConnectionTester.CheckInternetConnection(cancellationToken))
            {
                throw new NoInternetConnectionException();
            }

            int currentVersion = _api.GetAppLatestVersionId(_secretKey).Id;

            int? commonVersion = _applicationData.Cache.GetCommonVersion();

            if(commonVersion == null || currentVersion < commonVersion.Value || !CheckVersionConsistency(commonVersion.Value))
            {
                _applicationData.Clear();

                DownloadVersionContent(currentVersion, cancellationToken);
            }
            else if(commonVersion.Value != currentVersion)
            {
                int totalVersionsCount = currentVersion - commonVersion.Value;

                int doneVersionsCount = 0;

                while (currentVersion > commonVersion.Value)
                {
                    commonVersion = commonVersion.Value + 1;

                    // ReSharper disable once AccessToModifiedClosure
                    DownloadVersionDiff(commonVersion.Value, progress => OnProgress((doneVersionsCount + progress) / totalVersionsCount), cancellationToken);

                    doneVersionsCount++;
                }
            }
        }

        private void DownloadVersionContent(int version, AsyncCancellationToken cancellationToken)
        {
            var contentSummary = _api.GetAppContentSummary(_secretKey, version);

            var contentTorrentUrl = _api.GetAppContentTorrentUrl(_secretKey, version);

            var contentPackagePath = Path.Combine(_applicationData.TempPath, string.Format("download-content-{0}.package", version));

            var contentTorrentPath = Path.Combine(_applicationData.TempPath, string.Format("download-content-{0}.torrent", version));

            try
            {
                _status.IsDownloading = true;

                _httpDownloader.DownloadFile(contentTorrentUrl.Url, contentTorrentPath, 0, OnDownloadProgress,
                    cancellationToken);

                _torrentDownloader.DownloadFile(contentTorrentPath, contentPackagePath, OnDownloadProgress,
                    cancellationToken);

                _status.IsDownloading = false;

                _unarchiver.Unarchive(contentPackagePath, _applicationData.Path, OnProgress, cancellationToken);

                foreach (var contentFile in contentSummary.Files)
                {
                    _applicationData.Cache.SetFileVersion(contentFile.Path, version);
                }
            }
            finally
            {
                if (File.Exists(contentTorrentPath))
                {
                    File.Delete(contentTorrentPath);
                }

                if (File.Exists(contentPackagePath))
                {
                    File.Delete(contentPackagePath);
                }
            }
        }

        private void DownloadVersionDiff(int version, Action<float> onProgress, AsyncCancellationToken cancellationToken)
        {
            var diffSummary = _api.GetAppDiffSummary(_secretKey, version);

            var diffTorrentUrl = _api.GetAppDiffTorrentUrl(_secretKey, version);

            var diffPackagePath = Path.Combine(_applicationData.TempPath, string.Format("download-diff-{0}.package", version));

            var diffTorrentPath = Path.Combine(_applicationData.TempPath, string.Format("download-diff-{0}.torrent", version));

            var diffDirectoryPath = Path.Combine(_applicationData.TempPath, string.Format("diff-{0}", version));

            try
            {
                _status.IsDownloading = true;

                _httpDownloader.DownloadFile(diffTorrentUrl.Url, diffTorrentPath, 0, OnDownloadProgress, cancellationToken);

                _torrentDownloader.DownloadFile(diffTorrentPath, diffPackagePath, OnDownloadProgress, cancellationToken);

                _unarchiver.Unarchive(diffPackagePath, diffDirectoryPath, progress => onProgress(progress * 0.1f), cancellationToken);

                _status.IsDownloading = false;

                int totalFilesCount = diffSummary.RemovedFiles.Length + diffSummary.AddedFiles.Length +
                                        diffSummary.ModifiedFiles.Length;

                int doneFilesCount = 0;

                onProgress(0.1f);

                foreach (var removedFile in diffSummary.RemovedFiles)
                {
                    _applicationData.ClearFile(removedFile);

                    doneFilesCount++;

                    onProgress(0.1f + (float) doneFilesCount/totalFilesCount*0.9f);
                }

                foreach (var addedFile in diffSummary.AddedFiles)
                {
                    // HACK: Workaround for directories included in diff summary.
                    if (Directory.Exists(Path.Combine(diffDirectoryPath, addedFile)))
                    {
                        continue;
                    }

                    File.Copy(Path.Combine(diffDirectoryPath, addedFile), _applicationData.GetFilePath(addedFile), true);

                    _applicationData.Cache.SetFileVersion(addedFile, version);

                    doneFilesCount++;

                    onProgress(0.1f + (float)doneFilesCount / totalFilesCount * 0.9f);
                }

                foreach (var modifiedFile in diffSummary.ModifiedFiles)
                {
                    // HACK: Workaround for directories included in diff summary.
                    if (Directory.Exists(_applicationData.GetFilePath(modifiedFile)))
                    {
                        continue;
                    }

                    _applicationData.Cache.SetFileVersion(modifiedFile, -1);

                    _librsync.Patch(_applicationData.GetFilePath(modifiedFile), Path.Combine(diffDirectoryPath, modifiedFile), cancellationToken);

                    _applicationData.Cache.SetFileVersion(modifiedFile, version);

                    doneFilesCount++;

                    onProgress(0.1f + (float)doneFilesCount / totalFilesCount * 0.9f);
                }

                onProgress(1.0f);
            }
            finally
            {
                if (File.Exists(diffTorrentPath))
                {
                    File.Delete(diffTorrentPath);
                }

                if (File.Exists(diffPackagePath))
                {
                    File.Delete(diffPackagePath);
                }

                if (Directory.Exists(diffDirectoryPath))
                {
                    Directory.Delete(diffDirectoryPath, true);
                }
            }
        }

        private bool CheckVersionConsistency(int version)
        {
            var commonVersionContentSummary = _api.GetAppContentSummary(_secretKey, version);

            return _applicationData.CheckFilesConsistency(version, commonVersionContentSummary);
        }

        private void OnProgress(float progress)
        {
            Debug.Log(string.Format("OnProgress - {0}", progress));

            _status.Progress = progress;

            Dispatcher.Invoke(OnPatchingProgress.Invoke);
        }

        private void OnDownloadProgress(float progress, float speed)
        {
            Debug.Log(string.Format("OnDownloadProgress - {0} with speed {1}", progress, speed));

            _status.DownloadProgress = progress;
            _status.DownloadSpeed = speed;

            Dispatcher.Invoke(OnPatchingProgress.Invoke);
        }

        private void ResetStatus()
        {
            _status.Progress = 1.0f;

            _status.IsDownloading = false;
            _status.DownloadProgress = 1.0f;
            _status.DownloadSpeed = 0.0f;
        }
    }
}