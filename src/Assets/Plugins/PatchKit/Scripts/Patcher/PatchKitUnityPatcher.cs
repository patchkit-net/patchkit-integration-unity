using System;
using System.Threading;
using PatchKit.API;
using PatchKit.API.Async;
using PatchKit.Unity.API;
using PatchKit.Unity.Common;
using PatchKit.Unity.Patcher.Application;
using UnityEngine;
using UnityEngine.Events;

namespace PatchKit.Unity.Patcher
{
    /// <summary>
    /// Patcher working in Unity.
    /// </summary>
    public class PatchKitUnityPatcher : MonoBehaviour
    {
        public PlatformDependentString SecretKey;

        public PlatformDependentString ExecutableName;

        public ApplicationDataLocation ApplicationDataLocation;

        public UnityEvent OnPatchingStarted;

        public UnityEvent OnPatching;

        public UnityEvent OnPatchingFinished;

        public PatchKitUnityPatcherStatus Status
        {
            get { return _status; }
        }

        private PatchKitUnityPatcherStatus _status;

        private AsyncCancellationTokenSource _cancellationTokenSource = new AsyncCancellationTokenSource();

        private string _secretKey;

        private PatchKitAPI _api;

        private ApplicationData _applicationData;

        private void Awake()
        {
            ClearStatus();
            _status.State = PatchKitUnityPatcherState.None;
            Dispatcher.Initialize();
        }

        private void ClearStatus()
        {
            _status.Progress = 1.0f;
            _status.DownloadProgress = 1.0f;
            _status.DownloadFileName = string.Empty;
            _status.DownloadSpeed = 0.0f;
            _status.IsDownloading = false;
        }

        public void Start()
        {
            if (_status.State == PatchKitUnityPatcherState.Patching)
            {
                throw new InvalidOperationException("Patching is already started.");
            }

            _cancellationTokenSource = new AsyncCancellationTokenSource();

            _secretKey = SecretKey;
            _api = PatchKitUnity.API;
            _applicationData = new ApplicationData(ApplicationDataLocation.GetPath());

            ThreadPool.QueueUserWorkItem(state =>
            {
                _status.State = PatchKitUnityPatcherState.Patching;
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
                    ClearStatus();
                    Dispatcher.Invoke(OnPatchingFinished.Invoke);
                }
            });
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void RunApplication()
        {
            RunApplication("");
        }

        public void RunApplication(string arguments)
        {
            System.Diagnostics.Process.Start(ExecutableName, arguments);
        }

        public void Close()
        {
            UnityEngine.Application.Quit();
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
                commonVersion = null;
                _applicationData.Clear();
            }
            else if(commonVersion.Value == currentVersion)
            {
                return;
            }

            if(commonVersion == null)
            {
                
            }
        }

        private void DownloadVersionContent(string secretKey, int version)
        {
            var contentSummary = _api.GetAppContentSummary(secretKey, version);

            var 
        }

        private bool CheckVersionConsistency(int version)
        {   
            var commonVersionContentSummary = _api.GetAppContentSummary(_secretKey, version);

            return _applicationData.CheckFilesConsistency(version, commonVersionContentSummary);
        }
    }
}