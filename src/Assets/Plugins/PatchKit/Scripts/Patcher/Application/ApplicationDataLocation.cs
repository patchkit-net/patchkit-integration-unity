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
            switch (@this)
            {
                case ApplicationDataLocation.NextToPatcherExecutable:
                {
                    return Path.GetDirectoryName(UnityEngine.Application.dataPath);
                }
            }
            
            throw new InvalidEnumArgumentException("this", (int)@this, typeof(ApplicationDataLocation));
        }
    }
}
