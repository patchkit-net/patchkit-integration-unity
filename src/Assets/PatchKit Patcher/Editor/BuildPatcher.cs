using System.IO;
using UnityEditor;

namespace PatchKit.Unity.Examples.Editor
{
    public static class BuildPatcher
    {
        private static void Build(string buildPath, BuildTarget buildTarget)
        {
            PlayerSettings.companyName = "UpSoft";
            PlayerSettings.productName = "PatchKit Patcher";
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            PlayerSettings.showUnitySplashScreen = false;
            PlayerSettings.allowFullscreenSwitch = false;
            PlayerSettings.defaultIsFullScreen = false;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = 600;
            PlayerSettings.defaultScreenHeight = 400;

            var levels = new[] {"Assets/PatchKit/Examples/Patcher Example.unity"};

            BuildPipeline.BuildPlayer(levels, buildPath, buildTarget, BuildOptions.None);

            EditorUtility.OpenWithDefaultApp(Path.GetDirectoryName(buildPath));
        }

        [MenuItem("PatchKit/Build Patcher/Windows")]
        public static void BuildWindows()
        {
            Build(EditorUtility.SaveFilePanel("Choose Location", string.Empty, string.Empty, "exe"), BuildTarget.StandaloneWindows);
        }

        [MenuItem("PatchKit/Build Patcher/Mac OSX")]
        public static void BuildMacOSX()
        {
            Build(EditorUtility.SaveFilePanel("Choose Location", string.Empty, string.Empty, "app"), BuildTarget.StandaloneOSXUniversal);
        }

        [MenuItem("PatchKit/Build Patcher/Linux")]
        public static void BuildLinux()
        {
            Build(EditorUtility.SaveFilePanel("Choose Location", string.Empty, string.Empty, string.Empty), BuildTarget.StandaloneLinuxUniversal);
        }
    }
}
