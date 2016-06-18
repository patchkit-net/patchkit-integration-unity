using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PatchKit.Unity.Patcher
{
    [RequireComponent(typeof(PatchKitUnityPatcher))]
    public class PatcherController : MonoBehaviour
    {
        public string EditorCommandLineArgs;

        private PatchKitUnityPatcher _patchKitUnityPatcher;

        public void StartApplicationAndQuit()
        {
            StartApplication();

            UnityEngine.Application.Quit();
        }

        public void StartApplication()
        {
            var directoryInfo = new DirectoryInfo(_patchKitUnityPatcher.ApplicationDataPath);

            var executableFile = directoryInfo.GetFiles("*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (executableFile != null)
            {
                Process.Start(executableFile.FullName);
            }
        }

        public void Retry()
        {
            _patchKitUnityPatcher.StartPatching();
        }

        public void Quit()
        {
            UnityEngine.Application.Quit();
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
                // ReSharper disable once AssignNullToNotNullAttribute
                _patchKitUnityPatcher.ApplicationDataPath = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), applicationDataPath);
            }

            _patchKitUnityPatcher.StartPatching();
        }

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
    }
}
