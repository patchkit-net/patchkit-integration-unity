namespace Assets.Plugins.PatchKit.Scripts.AssetBundles.Web
{
    internal interface IAssetBundleDownloader
    {
        void Download(string secret, string destinationPath);
    }
}
