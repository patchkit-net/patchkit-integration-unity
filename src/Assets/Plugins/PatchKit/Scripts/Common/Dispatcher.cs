using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Action = Newtonsoft.Json.Serialization.Action;

namespace PatchKit.Unity.Common
{
    [AddComponentMenu("")]
    public class Dispatcher : MonoBehaviour
    {
        private static Dispatcher _instance;

        public static void Initialize()
        {
            if (_instance == null)
            {
                var gameObject = new GameObject("_CoroutineDispatcher");

                DontDestroyOnLoad(gameObject);

                _instance = gameObject.AddComponent<Dispatcher>();
            }
        }

        public static void Invoke(Action action)
        {
            lock (_instance._pendingActions)
            {
                _instance._pendingActions.Enqueue(action);
            }
        }

        public static EventWaitHandle InvokeCoroutine(IEnumerator coroutine)
        {
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);

            Invoke(() => _instance.StartCoroutine(CoroutineWithEventWaitHandle(coroutine, manualResetEvent)));

            return manualResetEvent;
        }

        private static IEnumerator CoroutineWithEventWaitHandle(IEnumerator coroutine, ManualResetEvent manualResetEvent)
        {
            try
            {
                while (coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }
            }
            finally
            {
                manualResetEvent.Set();
            }
        }

        private readonly Queue<Action> _pendingActions = new Queue<Action>();

        private void Update()
        {
            lock (_pendingActions)
            {
                while (_pendingActions.Count > 0)
                {
                    Action action = _pendingActions.Dequeue();
                    action();
                }
            }
        }
    }
}
