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

            string secretKey = SecretKey;
            string executableName = ExecutableName;
            var api = PatchKitUnity.API;

            ThreadPool.QueueUserWorkItem(state =>
            {
                _status.State = PatchKitUnityPatcherState.Patching;
                Dispatcher.Invoke(OnPatchingStarted.Invoke);
                try
                {
                    Patch(secretKey, executableName, new ApplicationData(ApplicationDataLocation.GetPath()),  api, _cancellationTokenSource.Token);
                    _status.State = PatchKitUnityPatcherState.Succeed;
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

        private void Patch(string secretKey, string exectuableName, ApplicationData applicationData, PatchKitAPI api, AsyncCancellationToken cancellationToken)
        {
            int currentVersion = api.GetAppLatestVersionId(secretKey).Id;


        }
    }
}