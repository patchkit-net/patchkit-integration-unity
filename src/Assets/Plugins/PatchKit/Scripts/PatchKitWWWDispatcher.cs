using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PatchKit.Integration.Unity
{
    [AddComponentMenu("")]
    public class PatchKitWWWDispatcher : MonoBehaviour
    {
        private readonly Queue<IEnumerator> _coroutinesFromOtherThreads = new Queue<IEnumerator>();

        public void StartCoroutineFromOtherThread(IEnumerator coroutine)
        {
            _coroutinesFromOtherThreads.Enqueue(coroutine);
        }

        private void Update()
        {
            while(_coroutinesFromOtherThreads.Count > 0)
            {
                IEnumerator coroutine = _coroutinesFromOtherThreads.Dequeue();
                StartCoroutine(coroutine);
            }
        }
    }
}
