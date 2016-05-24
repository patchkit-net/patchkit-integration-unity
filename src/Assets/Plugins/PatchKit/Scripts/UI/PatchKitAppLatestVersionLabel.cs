using System.Collections;
using UnityEngine.UI;

namespace PatchKit.Integration.Unity.UI
{
    public class PatchKitAppLatestVersionLabel : PatchKitBehaviour
    {
        public string SecretKey;

        public Text Text;

        protected override IEnumerator Request()
        {
            var request = API.BeginGetAppLatestVersion(SecretKey);

            yield return request.Wait();

            var latestVersion = API.EndGetAppLatestVersion(request);

            Text.text = latestVersion.Label;

            IsDone = true;
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