namespace PatchKit.Unity.Patcher
{
    public struct PatchKitUnityPatcherStatus
    {
        public PatchKitUnityPatcherState State;

        public float Progress;

        public bool IsDownloading;

        public float DownloadProgress;

        public string DownloadFileName;

        public float DownloadSpeed;
    }
}
