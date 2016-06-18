using PatchKit.Unity.API.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace PatchKit.Unity.Patcher
{
    [RequireComponent(typeof(PatchKitUnityPatcher))]
    public class PatcherLayout : MonoBehaviour
    {
        private PatchKitUnityPatcher _patchKitUnityPatcher;
        
        public int Width;

        public int Height;

        public Button CloseButton;

        public Button MinimizeButton;

        public Text Status;

        public Slider ProgressBar;

        public Text ProgressText;

        public Text DownloadStatus;

        public Slider DownloadProgressBar;

        public Text DownloadProgressText;

        public Button PlayButton;

        public RectTransform RetryPopup;

        public AppChangelogText ChangelogText;

        private void UpdateTitleBarButtons()
        {
            bool areActive = UnityEngine.Application.platform == RuntimePlatform.WindowsPlayer;

            CloseButton.gameObject.SetActive(areActive);
            MinimizeButton.gameObject.SetActive(areActive);
        }

        private void Awake()
        {
            _patchKitUnityPatcher = GetComponent<PatchKitUnityPatcher>();

            Assert.IsNotNull(_patchKitUnityPatcher);

            Screen.SetResolution(Width, Height, false);

            UpdateTitleBarButtons();
        }

        private void Start()
        {
            ChangelogText.SecretKey = _patchKitUnityPatcher.SecretKey;

            ChangelogText.Refresh();
        }

        private void UpdateProgress()
        {
            ProgressBar.value = _patchKitUnityPatcher.Status.Progress;

            ProgressText.text = _patchKitUnityPatcher.Status.Progress.ToString("0.0%");

            DownloadProgressBar.value = _patchKitUnityPatcher.Status.DownloadProgress;

            DownloadProgressText.text = _patchKitUnityPatcher.Status.DownloadProgress.ToString("0.0%");
        }

        private void UpdateStatus()
        {
            switch (_patchKitUnityPatcher.Status.State)
            {
                case PatcherState.None:
                    {
                        Status.text = string.Empty;
                        DownloadStatus.text = string.Empty;
                        break;
                    }
                case PatcherState.Patching:
                    {
                        Status.text = "Processing...";
                        DownloadStatus.text = _patchKitUnityPatcher.Status.IsDownloading
                            ? string.Format("Downloading {0} kB/s",
                                _patchKitUnityPatcher.Status.DownloadSpeed.ToString("0.0"))
                            : string.Empty;
                        break;
                    }
                case PatcherState.Succeed:
                    {
                        Status.text = "Ready!";
                        DownloadStatus.text = string.Empty;
                        break;
                    }
                case PatcherState.Cancelled:
                    {
                        Status.text = "Patching has been cancelled.";
                        DownloadStatus.text = string.Empty;
                        break;
                    }
                case PatcherState.NoInternetConnection:
                    {
                        Status.text = "Please check your internet connection.";
                        DownloadStatus.text = string.Empty;
                        break;
                    }
                case PatcherState.Failed:
                    {
                        Status.text = "An error has occured!";
                        DownloadStatus.text = string.Empty;
                        break;
                    }
            }
        }

        private void UpdatePlayButton()
        {
            PlayButton.interactable = _patchKitUnityPatcher.Status.State == PatcherState.Succeed;
        }

        private void UpdateRetryPopup()
        {
            bool isVisible;

            switch (_patchKitUnityPatcher.Status.State)
            {
                case PatcherState.Cancelled:
                case PatcherState.Failed:
                case PatcherState.NoInternetConnection:
                case PatcherState.None:
                    {
                        isVisible = true;
                        break;
                    }
                default:
                    {
                        isVisible = false;

                        break;
                    }
            }

            var retryPopupLocalPosition = RetryPopup.anchoredPosition;

            float retryPopupWidth = RetryPopup.rect.width;

            float transitionSpeed = Time.deltaTime * retryPopupWidth * 3.0f;

            if (isVisible)
            {
                retryPopupLocalPosition.x -= transitionSpeed;
            }
            else
            {
                retryPopupLocalPosition.x += transitionSpeed;
            }

            retryPopupLocalPosition.x = Mathf.Clamp(retryPopupLocalPosition.x, 0.0f, retryPopupWidth);

            RetryPopup.anchoredPosition = retryPopupLocalPosition;
        }

        private void Update()
        {
            UpdateStatus();

            UpdateProgress();

            UpdatePlayButton();

            UpdateRetryPopup();
        }
    }
}