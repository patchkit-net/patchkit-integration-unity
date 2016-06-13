using System.Collections;
using System.Linq;
using PatchKit.Unity.API.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PatchKit.Unity.API.UI
{
    public class AppChangelogText : MonoBehaviour
    {
        public string SecretKey;

        public Text Text;

        public int NumberOfBreakLines;

        protected virtual IEnumerator Start()
        {
            var request = PatchKitUnity.API.BeginGetAppVersionsList(SecretKey);

            yield return request.WaitCoroutine();

            var versionsList = PatchKitUnity.API.EndGetAppVersionsList(request);

            string separator = string.Empty;
            for (int i = 0; i < NumberOfBreakLines; i++)
            {
                separator += "\n";
            }

            Text.text = string.Join(separator,
                versionsList.Select(version => string.Format("{0}\n{1}", version.Label, version.Changelog)).ToArray());
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