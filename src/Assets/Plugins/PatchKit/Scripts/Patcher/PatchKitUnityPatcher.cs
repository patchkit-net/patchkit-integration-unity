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

            _status.State = PatchKitUnityPatcherState.Patching;

            ThreadPool.QueueUserWorkItem(state =>
            {
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
            System.Diagnostics.Process.Start(ExecutableName, arguments);
        }

        private void Patch(AsyncCancellationToken cancellationToken)
        {
            if(!InternetConnectionTester.CheckInternetConnection(cancellationToken))
            {
                throw new NoInternetConnectionException();
            }

            int currentVersion = _api.GetAppLatestVersionId(_secretKey).Id;

            int? commonVersion = _applicationData.Cache.GetCommonVersion();

            if(commonVersion == null || !CheckVersionConsistency(commonVersion.Value))
            {
                _applicationData.Clear();

                DownloadVersionContent(currentVersion, cancellationToken);
            }
            else if(commonVersion.Value != currentVersion)
            {
                // DownloadVersionPatch(currentVersion, cancellationToken);
            }
        }

        private void DownloadVersionContent(int version, AsyncCancellationToken cancellationToken)
        {
            var contentSummary = _api.GetAppContentSummary(_secretKey, version);

            var contentTorrentUrl = _api.GetAppContentTorrentUrl(_secretKey, version);

            var contentPackagePath = Path.Combine(_applicationData.TempPath, string.Format("download-content-{0}.package", version));

            var contentTorrentDownloadPath = Path.Combine(_applicationData.TempPath, string.Format("download-content-{0}.torrent", version));

            _status.IsDownloading = true;

            _httpDownloader.DownloadFile(contentTorrentUrl.Url, contentTorrentDownloadPath, OnDownloadProgress, cancellationToken);

            _torrentDownloader.DownloadFile(contentTorrentDownloadPath, contentPackagePath, OnDownloadProgress, cancellationToken);

            _status.IsDownloading = false;

            _unarchiver.Unarchive(contentPackagePath, _applicationData.Path, OnUnarchiveProgress, cancellationToken);

            foreach (var contentFile in contentSummary.Files)
            {
                _applicationData.Cache.SetFileVersion(contentFile.Path, version);
            }
        }

        private bool CheckVersionConsistency(int version)
        {
            var commonVersionContentSummary = _api.GetAppContentSummary(_secretKey, version);

            return _applicationData.CheckFilesConsistency(version, commonVersionContentSummary);
        }

        private void OnUnarchiveProgress(float progress)
        {
            _status.Progress = progress;

            Dispatcher.Invoke(OnPatchingProgress.Invoke);
        }

        private void OnDownloadProgress(string fileName, float progress, float speed)
        {
            _status.DownloadFileName = fileName;
            _status.DownloadProgress = progress;
            _status.DownloadSpeed = speed;

            Dispatcher.Invoke(OnPatchingProgress.Invoke);
        }

        private void ResetStatus()
        {
            _status.Progress = 1.0f;

            _status.IsDownloading = false;
            _status.DownloadProgress = 1.0f;
            _status.DownloadFileName = string.Empty;
            _status.DownloadSpeed = 0.0f;
        }
    }
}