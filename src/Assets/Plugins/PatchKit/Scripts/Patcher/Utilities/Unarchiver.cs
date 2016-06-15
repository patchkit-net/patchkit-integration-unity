using System.IO;
using Ionic.Zip;
using PatchKit.API.Async;

namespace PatchKit.Unity.Patcher.Utilities
{
    public class Unarchiver
    {
        public delegate void UnarchiveProgress(float progress);

        public void Unarchive(string packagePath, string destinationPath, UnarchiveProgress onUnarchiveProgress, AsyncCancellationToken cancellationToken)
        {
            using (var zip = ZipFile.Read(packagePath))
            {
                int entryCounter = 0;

                onUnarchiveProgress(0.0f);

                Directory.CreateDirectory(destinationPath);

                foreach (ZipEntry zipEntry in zip)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    zipEntry.Extract(destinationPath, ExtractExistingFileAction.OverwriteSilently);

                    entryCounter++;

                    if (!zipEntry.IsDirectory)
                    {
                        onUnarchiveProgress(entryCounter / (float)zip.Count);
                    }
                }

                onUnarchiveProgress(1.0f);
            }
        }
    }
}