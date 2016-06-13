using System.Collections;
using PatchKit.Unity.API.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PatchKit.Unity.API.UI
{
    public class AppLatestVersionLabelText : MonoBehaviour
    {
        public string SecretKey;

        public Text Text;

        protected virtual IEnumerator Start()
        {
            var request = PatchKitUnity.API.BeginGetAppLatestVersion(SecretKey);

            yield return request.WaitCoroutine();

            var latestVersion = PatchKitUnity.API.EndGetAppLatestVersion(request);

            Text.text = latestVersion.Label;
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