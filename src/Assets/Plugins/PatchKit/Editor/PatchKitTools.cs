using UnityEngine;
using System.Collections;
using System.Diagnostics;

public static class PatchKitTools 
{
    public bool Available()
    {
        //TODO: Implementation.
        return true;
    }

    public void Execute(string toolName, params string[] arguments)
    {
        string argumentsText = ;

        string commandLine = string.Format("patchkit-tools {0} {1}", toolName, argumentsText);

        ProcessStartInfo processStartInfo;

        processStartInfo.RedirectStandardOutput = true;

        processStartInfo.Arguments = toolName + " " + string.Join(" ", arguments);

        Process process = new Process(processStartInfo);

        process.Start();
    }

    public string MakeVersion(string secret, string versionPath, string versionLabel)
    {
        Execute("make-version", string.Format("-f \"{0}", versionPath));
    }
}
