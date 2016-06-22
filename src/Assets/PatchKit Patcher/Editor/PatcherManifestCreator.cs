using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;

namespace PatchKit.Unity.Patcher.Editor
{
    public static class PatcherManifestCreator
    {
        [PostProcessBuild, UsedImplicitly]
        private static void PostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            string runCmd = string.Empty;

            if (buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64)
            {
                runCmd = string.Format("{0} --installDir {{0}} --secret {{1}}", Path.GetFileName(buildPath));
                    
            }

            if (buildTarget == BuildTarget.StandaloneOSXUniversal ||
                buildTarget == BuildTarget.StandaloneOSXIntel ||
                buildTarget == BuildTarget.StandaloneOSXIntel64)
            {
                runCmd = string.Format("open -a \"{0}\" --args --installDir {{0}} --secret {{1}}", Path.GetFileName(buildPath));
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            string manifestPath = Path.Combine(Path.GetDirectoryName(buildPath), "patcher.manifest");

            string manifestContent = "{" + "\n" +
                                        string.Format("    \"run_cmd\" : \"{0}\"", runCmd) + "\n" +
                                        "}";

            File.WriteAllText(manifestPath, manifestContent);
        }
    }
}