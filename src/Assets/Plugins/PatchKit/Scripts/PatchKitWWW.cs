using System;
using System.Collections;
using System.Net;
using System.Threading;
using PatchKit.API.Async;
using PatchKit.API.Web;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PatchKit.Integration.Unity
{
    public class PatchKitWWW : IWWW
    {
        private class CoroutineResult
        {
            public WWWResponse<string> Response;

            public Exception Exception;

            public bool HasBeenCancelled;
        }

        private readonly PatchKitWWWDispatcher _patchKitWWWDispatcher;

        public PatchKitWWW()
        {
            var gameObject = new GameObject("_PatchKitWWWDispatcher");
            Object.DontDestroyOnLoad(gameObject);

            _patchKitWWWDispatcher = gameObject.AddComponent<PatchKitWWWDispatcher>();
        }

        public ICancellableAsyncResult BeginDownloadString(string url, CancellableAsyncCallback asyncCallback = null,
            object state = null)
        {
            return new AsyncResult<WWWResponse<string>>(cancellationToken => DownloadString(url, cancellationToken), asyncCallback,
                state);
        }

        public WWWResponse<string> EndDownloadString(ICancellableAsyncResult asyncResult)
        {
            var result = asyncResult as AsyncResult<WWWResponse<string>>;

            if (result == null)
            {
                throw new ArgumentException("asyncResult");
            }

            return result.FetchResultsFromAsyncOperation();
        }

        private WWWResponse<string> DownloadString(string url, AsyncCancellationToken cancellationToken)
        {
            var waitHandle = new ManualResetEvent(false);

            var coroutineResult = new CoroutineResult();

            _patchKitWWWDispatcher.StartCoroutineFromOtherThread(DownloadStringCoroutine(url, cancellationToken, waitHandle, coroutineResult));

            waitHandle.WaitOne();

            if (coroutineResult.HasBeenCancelled)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (coroutineResult.Exception != null)
            {
                throw coroutineResult.Exception;
            }

            return coroutineResult.Response;
        }

        private IEnumerator DownloadStringCoroutine(string url, AsyncCancellationToken cancellationToken, ManualResetEvent waitHandle, CoroutineResult result)
        {
            WWW www = null;

            try
            {
                www = new WWW(url);
            }
            catch (Exception exception)
            {
                result.Exception = exception;
            }

            if (www != null)
            {
                while (!www.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result.HasBeenCancelled = true;

                        break;
                    }

                    yield return null;
                }

                if (www.isDone)
                {
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        result.Exception = new WebException(www.error);
                    }
                    else
                    {
                        try
                        {
                            result.Response = new WWWResponse<string>(www.text, GetResponseCode(www));
                        }
                        catch (Exception exception)
                        {
                            result.Exception = exception;
                        }
                    }
                }
            }

            waitHandle.Set();
        }

        public static int GetResponseCode(WWW www)
        {
            if (www.responseHeaders == null)
            {
                throw new WebException("Missing response headers.");
            }

            if (!www.responseHeaders.ContainsKey("STATUS"))
            {
                throw new WebException("Status missing from response headers.");
            }

            return ParseResponseCode(www.responseHeaders["STATUS"]);
        }

        public static int ParseResponseCode(string statusLine)
        {
            int statusCode;

            string[] components = statusLine.Split(' ');
            if (components.Length < 3)
            {
                throw new WebException(string.Format("Invaild response code: {0}", statusLine));
            }

            if (!int.TryParse(components[1], out statusCode))
            {
                throw new WebException(string.Format("Invaild response code: {0}", components[1]));
            }

            return statusCode;
        }
    }
}