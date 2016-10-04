using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using UnityEditor;

namespace PatchKit.Unity.Editor
{
    public static class PatchKitTools
    {
        private static void ThrowIfNotAvailable()
        {
            if (!AreAvailable())
            {
                throw new InvalidOperationException("PatchKit Tools aren't available. Make sure that everything is setup correctly.");
            }
        }

        private static string GetPath()
        {
            if (PatchKitToolsSettings.Editor.Path != null || string.IsNullOrEmpty(PatchKitToolsSettings.Editor.Path))
            {
                return PatchKitToolsSettings.Editor.Path;
            }

            return FindPath();
        }

        /// <summary>
        /// Finds the path of patchkit-tools by searching locations in PATH variable.
        /// </summary>
        [CanBeNull]
        public static string FindPath()
        {
            var paths = Environment.GetEnvironmentVariable("PATH");

            if (paths != null)
            {
                foreach (var path in paths.Split(';'))
                {
                    var fullPath = Path.Combine(path, "patchkit-tools");

                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }

        public static bool AreAvailable()
        {
            string path = GetPath();

            return path != null && File.Exists(path);
        }

        /// <summary>
        /// Executes the specified tool.
        /// </summary>
        /// <param name="toolName">Name of the tool.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>Exit code.</returns>
        /// <exception cref="InvalidOperationException">PatchKit Tools aren't available. Make sure that everything is setup correctly.</exception>
        public static int Execute(string toolName, string arguments)
        {
            ThrowIfNotAvailable();

            Process process = new Process
            {
                StartInfo =
            {
                FileName = GetPath(),
                Arguments = toolName + " " + arguments
            }
            };

            process.Start();

            while (!process.HasExited)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Waiting for finish of tool execution...",
                        "Click cancel to abort tool execution.", 0.0f))
                {
                    process.Kill();
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            return process.ExitCode;
        }

        public static int MakeVersion(string secret, string apiKey, string versionPath, string versionLabel)
        {
            return Execute("make-version", string.Format("-f \"{0}\" -l \"{1}\" -s \"{2}\" -a \"{3}\"", versionPath, versionLabel, secret, apiKey));
        }
    }
}
