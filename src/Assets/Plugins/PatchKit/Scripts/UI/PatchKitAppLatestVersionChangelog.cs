using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace PatchKit.Integration.Unity.UI
{
    public class PatchKitAppLatestVersionChangelog : PatchKitBehaviour
    {
        public string SecretKey;

        public Text Text;

        protected override IEnumerator Request()
        {
            var request = API.BeginGetAppLatestVersion(SecretKey);

            yield return request.Wait();

            var version = API.EndGetAppLatestVersion(request);

            Text.text = version.Changelog;

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