#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PatchKit.Unity
{
    /// <summary>
    /// Settings for <see cref="ApiInstance"/>.
    /// </summary>
    public class ApiInstanceSettings : ScriptableObject
    {
        private const string SettingsFileName = "PatchKit API Settings";

        [SerializeField]
        public ApiConnectionSettings ConnectionSettings;

        private static ApiConnectionSettings CreateApiConnectionSettings()
        {
            return new ApiConnectionSettings(10000, "http://api.patchkit.net");
        }

        private static ApiInstanceSettings FindInstance()
        {
            var settings = Resources.Load<ApiInstanceSettings>(SettingsFileName);

#if UNITY_EDITOR
            if (settings == null)
            {
                bool pingObject = false;

                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;

                    EditorUtility.DisplayDialog("PatchKit API Settings created!", "PatchKit API Settings has been created.", "OK");

                    pingObject = true;
                }

                settings = CreateInstance<ApiInstanceSettings>();
                settings.ConnectionSettings = CreateApiConnectionSettings();

                AssetDatabase.CreateAsset(settings, string.Format("Assets/Plugins/PatchKit/Resources/{0}.asset", SettingsFileName));
                EditorUtility.SetDirty(settings);

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                if (pingObject)
                {
                    EditorGUIUtility.PingObject(settings);
                }
            }
#endif
            return settings;
        }

        public static ApiConnectionSettings GetConnectionSettings()
        {
            var instance = FindInstance();

            if (instance == null)
            {
                return CreateApiConnectionSettings();
            }

            return instance.ConnectionSettings;
        }
    }
}
