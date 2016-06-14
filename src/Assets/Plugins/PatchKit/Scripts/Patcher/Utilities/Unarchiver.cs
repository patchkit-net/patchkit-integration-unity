using PatchKit.API.Async;

namespace PatchKit.Unity.Patcher.Utilities
{
    public class Unarchiver
    {
        public delegate void UnarchiveProgress(float progress);

        public void Unarchive(string packagePath, string destinationPath, UnarchiveProgress onUnarchiveProgress, AsyncCancellationToken cancellationToken)
        {
            //TODO:
        }
    }
}