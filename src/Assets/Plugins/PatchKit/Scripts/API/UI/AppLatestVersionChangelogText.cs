using System.Collections;
using PatchKit.Unity.API.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PatchKit.Unity.API.UI
{
    public class AppLatestVersionChangelogText : MonoBehaviour
    {
        public string SecretKey;

        public Text Text;

        protected virtual IEnumerator Start()
        {
            var request = PatchKitUnity.API.BeginGetAppLatestVersion(SecretKey);

            yield return request.WaitCoroutine();

            var version = PatchKitUnity.API.EndGetAppLatestVersion(request);

            Text.text = version.Changelog;
        }

        protected virtual void Reset()
        {
            if (Text == null)
            {
                Text = GetComponent<Text>();
            }
        }
    }
}