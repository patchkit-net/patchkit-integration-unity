using UnityEngine;

namespace PatchKit.Integration.Unity.Examples
{
    public class OpenURL : MonoBehaviour
    {
        public void Open(string url)
        {
            Application.OpenURL(url);
        }
    }
}