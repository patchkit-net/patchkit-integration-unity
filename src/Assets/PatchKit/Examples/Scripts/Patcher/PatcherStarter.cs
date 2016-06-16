using UnityEngine;
using PatchKit.Unity.Common;
using PatchKit.Unity.Patcher;
#if !UNITY_EDITOR
using System.Linq;
#endif

namespace PatchKit.Unity.Examples
{
    [RequireComponent(typeof(PatchKitUnityPatcher))]
    public class PatcherStarter : MonoBehaviour
    {
        private PatchKitUnityPatcher _patchKitUnityPatcher;

        public PlatformDependentString DefaultSecretKey;

        public PlatformDependentString DefaultExecutableName;

        private void Awake()
        {
            _patchKitUnityPatcher = GetComponent<PatchKitUnityPatcher>();

#if UNITY_EDITOR
            _patchKitUnityPatcher.SecretKey = DefaultSecretKey;

            _patchKitUnityPatcher.ExecutableName = DefaultExecutableName;
#else
            var args = System.Environment.GetCommandLineArgs().ToList();

            int secretKeyIndex = args.IndexOf("--secret") + 1;

            if (secretKeyIndex < args.Count)
            {
                _patchKitUnityPatcher.SecretKey = args[secretKeyIndex];
            }
            else
            {
                _patchKitUnityPatcher.SecretKey = string.Empty;
            }
#endif

            _patchKitUnityPatcher.StartPatching();
        }
    }
}
