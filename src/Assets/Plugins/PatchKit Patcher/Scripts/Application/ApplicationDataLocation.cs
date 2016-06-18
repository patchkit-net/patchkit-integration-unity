using System.ComponentModel;
using System.IO;

namespace PatchKit.Unity.Patcher.Application
{
    public enum ApplicationDataLocation
    {
        NextToPatcherExecutable
    }

    public static class ApplicationDataLocationExtensions
    {
        public static string GetPath(this ApplicationDataLocation @this)
        {
            if (!UnityEngine.Application.isEditor)
            {
                switch (@this)
                {
                    case ApplicationDataLocation.NextToPatcherExecutable:
                    {
                        return Path.GetDirectoryName(UnityEngine.Application.dataPath);
                    }
                }

                throw new InvalidEnumArgumentException("this", (int) @this, typeof(ApplicationDataLocation));
            }

            return UnityEngine.Application.persistentDataPath;
        }
    }
}
