using System;
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

        private bool _publish;

        private BuildTarget _settingsSecretBuildTarget;

        private Tab _tab = Tab.MakeVersion;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private void OnGUI()
        {
            GUILayout.BeginVertical();
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
            GUILayout.EndVertical();
        }

        private bool IsSupportedPlatform()
        {
            if (AvailableBuildTargets.Contains(EditorUserBuildSettings.activeBuildTarget))
            {
                return true;
            }

            return false;
        }

        private void DisplayTabSelection()
        {
            _tab = (Tab) GUILayout.Toolbar((int) _tab, new []{"Make Version", "Settings"});
        }

        private void DisplayMakeVersionInformation()
        {
            if (IsSupportedPlatform())
            {
                EditorGUILayout.BeginVertical(EditorStyles.textArea);
                {
                    EditorGUILayout.LabelField(string.Format("Build target - {0}", EditorUserBuildSettings.activeBuildTarget));
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("Unsupported build target - {0}", EditorUserBuildSettings.activeBuildTarget),
                    MessageType.Error);
            }

            if (GUILayout.Button("Open Build Settings"))
            {
                GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);
            }
        }


        private void DisplayMakeVersionBuildBar()
        {
            EditorGUI.BeginDisabledGroup(!IsSupportedPlatform());

            GUILayout.BeginHorizontal();
            {
                GUILayout.Button("Build and make version", GUILayout.ExpandWidth(true));

                _publish = EditorGUILayout.ToggleLeft("Publish", _publish, GUILayout.Width(65.0f));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10.0f);

            EditorGUI.EndDisabledGroup();
        }

        private void DisplayMakeVersionTab()
        {
            DisplayMakeVersionInformation();

            GUILayout.FlexibleSpace();

            DisplayMakeVersionBuildBar();
        }

        private void DisplaySettingsTab()
        {
            string apiKey = PatchKitToolsSettings.Project.GetApiKey();

            apiKey = EditorGUILayout.TextField("API key", apiKey);

            PatchKitToolsSettings.Project.SetApiKey(apiKey);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Secret");
                _settingsSecretBuildTarget = (BuildTarget) EditorGUILayout.IntPopup((int) _settingsSecretBuildTarget,
                    AvailableBuildTargets.Select(target => target.ToString()).ToArray(),
                    AvailableBuildTargets.Select(target => (int) target).ToArray());

                if (!AvailableBuildTargets.Contains(_settingsSecretBuildTarget))
                {
                    _settingsSecretBuildTarget = AvailableBuildTargets[0];
                }

                string secret = PatchKitToolsSettings.Project.GetSecret(_settingsSecretBuildTarget);

                secret = EditorGUILayout.TextField(secret);

                PatchKitToolsSettings.Project.SetSecret(_settingsSecretBuildTarget, secret);
            }
            GUILayout.EndHorizontal();
        }
    }
}
