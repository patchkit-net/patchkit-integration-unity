using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace PatchKit.Integration.Unity.UI
{
    public class PatchKitAppChangelog : PatchKitBehaviour
    {
        public string SecretKey;

        public Text Text;

        public int NumberOfBreakLines;

        protected override IEnumerator Request()
        {
            var request = API.BeginGetAppVersionsList(SecretKey);

            yield return request.Wait();

            var versionsList = API.EndGetAppVersionsList(request);

            string separator = string.Empty;
            for (int i = 0; i < NumberOfBreakLines; i++)
            {
                separator += "\n";
            }

            Text.text = string.Join(separator,
                versionsList.Select(version => string.Format("{0}\n{1}", version.Label, version.Changelog)).ToArray());

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