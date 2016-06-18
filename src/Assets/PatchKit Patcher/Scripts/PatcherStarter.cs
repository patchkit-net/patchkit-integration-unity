using System;
using System.Linq;
using UnityEngine;

namespace PatchKit.Unity.Patcher
{
    [RequireComponent(typeof(PatchKitUnityPatcher))]
    public class PatcherStarter : MonoBehaviour
    {
        private PatchKitUnityPatcher _patchKitUnityPatcher;

        public string EditorCommandLineArgs;

        private string[] GetCommandLineArgs()
        {
#if UNITY_EDITOR
            return EditorCommandLineArgs.Split(' ');
#else
            return System.Environment.GetCommandLineArgs();
#endif
        }

        private bool TryReadArgument(string argumentName, out string value)
        {
            var args = GetCommandLineArgs().ToList();

            int index = args.IndexOf(argumentName) + 1;

            if (index < args.Count)
            {
                value = args[index];

                return true;
            }

            value = null;

            return false;
        }

        private static string DecodeSecret(string encodedSecret)
        {
            var bytes = Convert.FromBase64String(encodedSecret);

            for (int i = 0; i < bytes.Length; ++i)
            {
                byte b = bytes[i];
                bool lsb = (b & 1) > 0;
                b >>= 1;
                b |= (byte)(lsb ? 128 : 0);
                b = (byte)~b;
                bytes[i] = b;
            }

            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void Awake()
        {
            _patchKitUnityPatcher = GetComponent<PatchKitUnityPatcher>();

            string secretKey;

            if (TryReadArgument("--secret", out secretKey))
            {
                _patchKitUnityPatcher.SecretKey = DecodeSecret(secretKey);
            }

            string applicationDataPath;

            if (TryReadArgument("--installdir", out applicationDataPath))
            {
                _patchKitUnityPatcher.ApplicationDataLocation = applicationDataPath;
            }

            _patchKitUnityPatcher.StartPatching();
        }
    }
}
