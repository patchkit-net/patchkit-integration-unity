using System.Collections;
using PatchKit.API;
using UnityEngine;

namespace PatchKit.Integration.Unity
{
    public abstract class PatchKitBehaviour : MonoBehaviour
    {
        private static PatchKitAPI _api;

        public static PatchKitAPI API
        {
            get { return _api ?? (_api = new PatchKitAPI(new PatchKitAPISettings(), new PatchKitWWW())); }
        }

        private bool _isRequesting;

        protected bool IsDone;

        protected virtual void Awake()
        {
            IsDone = false;
            _isRequesting = false;
        }

        protected virtual void Update()
        {
            if (API == null)
            {
                Debug.LogError("API is missing! Please either attach API to this component or disable it.", this);
            }
            else if (!IsDone && !_isRequesting)
            {
                StartCoroutine(DoRequest());
            }
        }

        protected abstract IEnumerator Request();

        private IEnumerator DoRequest()
        {
            _isRequesting = true;

            var request = Request();

            while (request.MoveNext())
            {
                yield return request.Current;
            }

            _isRequesting = false;
        }
    }
}