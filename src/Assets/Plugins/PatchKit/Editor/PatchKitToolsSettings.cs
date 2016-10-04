using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace PatchKit.Unity.Editor
{
    public static class PatchKitToolsSettings
    {
        public static class Editor
        {
            [CanBeNull]
            public static string Path
            {
                get { return EditorPrefs.GetString("PatchKitToolsSettings.Editor.Path", null); }
                set { EditorPrefs.SetString("PatchKitToolsSettings.Editor.Path", value);}
            }
        }

        public static class Project
        {
            private static string ProjectIdentifier
            {
                get { return Application.dataPath; }
            }

            private static string GetSecretEditorPrefsKey(BuildTarget platform)
            {
                return string.Format("PatchKitToolsSettings.Project.{0}.Secret.{1}", ProjectIdentifier, platform);
            }

            [CanBeNull]
            public static string GetSecret(BuildTarget platform)
            {
                return EditorPrefs.GetString(GetSecretEditorPrefsKey(platform), null);
            }

            public static void SetSecret(BuildTarget platform, string value)
            {
                EditorPrefs.SetString(GetSecretEditorPrefsKey(platform), value);
            }

            [CanBeNull]
            public static string GetApiKey()
            {
                return EditorPrefs.GetString(string.Format("PatchKitToolsSettings.Project.{0}.ApiKey", ProjectIdentifier), null);
            }

            public static void SetApiKey(string value)
            {
                EditorPrefs.SetString(string.Format("PatchKitToolsSettings.Project.{0}.ApiKey", ProjectIdentifier), value);
            }
        }
    }
}