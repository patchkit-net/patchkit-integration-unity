using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PatchKit.Unity.Editor
{
    public class PatchKitToolsWindow : EditorWindow
    {
        private enum Tab
        {
            MakeVersion,
            Settings
        }

        private static readonly BuildTarget[] AvailableBuildTargets =
        {
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneOSXIntel,
            BuildTarget.StandaloneOSXIntel64,
            BuildTarget.StandaloneOSXUniversal,
            BuildTarget.StandaloneLinux,
            BuildTarget.StandaloneLinux64,
            BuildTarget.StandaloneLinuxUniversal
        };

        [MenuItem("Window/PatchKit/PatchKit Tools")]
        public static void Init()
        {
            var window = GetWindow<PatchKitToolsWindow>(false, "PatchKit Tools");

            window.Show();
        }

        private bool _makeVersionPublish;

        private string _makeVersionLabel;

        private bool _settingsHasPathBeenSearched;

        private BuildTarget _settingsPlatformSpecific = EditorUserBuildSettings.activeBuildTarget;

        private Tab _tab = Tab.MakeVersion;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                DisplayTabSelection();

                if (_tab == Tab.MakeVersion)
                {
                    DisplayMakeVersionTab();
                }
                else if (_tab == Tab.Settings)
                {
                    DisplaySettingsTab();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DisplayTabSelection()
        {
            Tab previousTab = _tab;
            _tab = (Tab) GUILayout.Toolbar((int) _tab, new[] {"Make Version", "Settings"});
            if (_tab != previousTab)
            {
                EditorGUI.FocusTextInControl(string.Empty);
            }
        }

        private void BuildAndMakeVersion(bool publish)
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            string secret = PatchKitToolsSettings.Project.GetSecret(buildTarget);
            string apiKey = PatchKitToolsSettings.Project.GetApiKey();
            string buildFileName = PatchKitToolsSettings.Project.GetBuildFileName(buildTarget);

            string buildDirectory = FileUtil.GetUniqueTempPathInProject();

            try
            {
                var buildDirectoryInfo = Directory.CreateDirectory(buildDirectory);

                BuildPipeline.BuildPlayer(
                    EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Path.Combine(buildDirectoryInfo.FullName, buildFileName), buildTarget, BuildOptions.None);

                // Clear build from PDB files

                foreach (var pdbFile in buildDirectoryInfo.GetFiles("*.pdb", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(pdbFile.FullName);
                }

                PatchKitTools.MakeVersion(secret, apiKey, buildDirectoryInfo.FullName, _makeVersionLabel, publish);
            }
            finally
            {
                if (Directory.Exists(buildDirectory))
                {
                    Directory.Delete(buildDirectory, true);
                }

                if (File.Exists(buildDirectory))
                {
                    File.Delete(buildDirectory);
                }
            }
        }

        private bool DisplayMakeVersionError()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            if (string.IsNullOrEmpty(_makeVersionLabel))
            {
                EditorGUILayout.HelpBox("Version label cannot be empty.", MessageType.Error);

                return true;
            }

            if (!AvailableBuildTargets.Contains(buildTarget))
            {
                EditorGUILayout.HelpBox(string.Format("Unsupported build target - {0}\n" +
                                                      "Please change it in Build Settings.",
                    buildTarget),
                    MessageType.Error);

                return true;
            }

            string secret = PatchKitToolsSettings.Project.GetSecret(buildTarget);

            if (string.IsNullOrEmpty(secret))
            {
                EditorGUILayout.HelpBox("Empty or invalid secret.\nPlease change it in Settings.", MessageType.Error);

                return true;
            }

            string apiKey = PatchKitToolsSettings.Project.GetApiKey();

            if (string.IsNullOrEmpty(apiKey))
            {
                EditorGUILayout.HelpBox("Empty or invalid API key.\nPlease change it in Settings.", MessageType.Error);

                return true;
            }

            string buildName = PatchKitToolsSettings.Project.GetBuildFileName(buildTarget);

            if (string.IsNullOrEmpty(buildName))
            {
                EditorGUILayout.HelpBox("Empty or invalid build name.\nPlease change it in Settings.", MessageType.Error);

                return true;
            }

            return false;
        }

        private void DisplayMakeVersionInformation()
        {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            {
                EditorGUILayout.LabelField(string.Format("Build target - {0}",
                    EditorUserBuildSettings.activeBuildTarget));

                string buildFileName = PatchKitToolsSettings.Project.GetBuildFileName(EditorUserBuildSettings.activeBuildTarget);

                EditorGUILayout.LabelField(string.Format("Build file name - {0}", buildFileName));

                string secret = PatchKitToolsSettings.Project.GetSecret(EditorUserBuildSettings.activeBuildTarget);

                EditorGUILayout.LabelField(string.Format("Secret - {0}", secret));
            }
            EditorGUILayout.EndVertical();
        }

        private void DisplayMakeVersionBuildBar()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build and make version", GUILayout.ExpandWidth(true)))
                {
                    EditorApplication.delayCall += () => BuildAndMakeVersion(_makeVersionPublish);
                }

                _makeVersionPublish = EditorGUILayout.ToggleLeft("Publish", _makeVersionPublish, GUILayout.Width(65.0f));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DisplayMakeVersionTab()
        {
            _makeVersionLabel = EditorGUILayout.TextField("Version label", _makeVersionLabel);

            bool error = DisplayMakeVersionError();

            if (!error)
            {
                DisplayMakeVersionInformation();
            }

            if (GUILayout.Button("Open Build Settings"))
            {
                GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(error);

            DisplayMakeVersionBuildBar();

            EditorGUI.EndDisabledGroup();
        }

        private void DisplaySettingsToolsPath()
        {
            string path = PatchKitToolsSettings.Editor.Path ?? string.Empty;

            if (!_settingsHasPathBeenSearched && string.IsNullOrEmpty(path) && PatchKitTools.AreAvailable())
            {
                _settingsHasPathBeenSearched = true;
                path = PatchKitTools.FindPath() ?? string.Empty;
            }

            EditorGUILayout.BeginHorizontal();
            {
                path = EditorGUILayout.TextField("Path", path);
                if (GUILayout.Button("Open"))
                {
                    path = EditorUtility.OpenFilePanel("Open patchkit-tools", string.Empty, string.Empty);
                }
            }
            EditorGUILayout.EndHorizontal();

            PatchKitToolsSettings.Editor.Path = path;
        }

        private void DisplaySettingsTab()
        {
            DisplaySettingsToolsPath();

            string apiKey = PatchKitToolsSettings.Project.GetApiKey();

            apiKey = EditorGUILayout.TextField("API key", apiKey);

            PatchKitToolsSettings.Project.SetApiKey(apiKey);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Platform specific");
                _settingsPlatformSpecific = (BuildTarget) EditorGUILayout.IntPopup((int) _settingsPlatformSpecific,
                    AvailableBuildTargets.Select(target => target.ToString()).ToArray(),
                    AvailableBuildTargets.Select(target => (int) target).ToArray());

                if (!AvailableBuildTargets.Contains(_settingsPlatformSpecific))
                {
                    _settingsPlatformSpecific = AvailableBuildTargets[0];
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            {
                string secret = PatchKitToolsSettings.Project.GetSecret(_settingsPlatformSpecific);

                secret = EditorGUILayout.TextField("Secret", secret);

                PatchKitToolsSettings.Project.SetSecret(_settingsPlatformSpecific, secret);

                string buildFileName = PatchKitToolsSettings.Project.GetBuildFileName(_settingsPlatformSpecific);

                buildFileName = EditorGUILayout.TextField("Build file name", buildFileName);

                PatchKitToolsSettings.Project.SetBuildFileName(_settingsPlatformSpecific, buildFileName);

                EditorGUILayout.HelpBox("Remember to set file extension prior to platform. For example - Game.exe",
                    MessageType.Info);

                EditorGUILayout.HelpBox("Changing build file name in update could result in patch equal to whole game size. Try to not change it after uploading first version.",
                    MessageType.Warning);
            }
            EditorGUILayout.EndVertical();
        }
    }
}