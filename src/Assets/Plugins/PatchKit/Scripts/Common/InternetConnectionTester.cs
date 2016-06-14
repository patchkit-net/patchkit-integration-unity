using UnityEngine;
using System.Collections;

namespace PatchKit.Unity.Common
{
    internal static class InternetConnectionTester 
    {
        private class TestResult
        {
            public TestResult()
            {
                Result = false;
            }

            public bool Result;
        }

        private const int Timeout = 10000;

        private const string TestingUrl = "http://www.google.com";

        public static bool CheckInternetConnection(PatchKit.API.Async.AsyncCancellationToken cancellationToken)
        {
            var testResult = new TestResult();

            var waitHandle = Dispatcher.InvokeCoroutine(CheckInternetConnectionCoroutine(testResult, cancellationToken));

            waitHandle.WaitOne();

            cancellationToken.ThrowIfCancellationRequested();

            return testResult.Result;
        }

        private static IEnumerator CheckInternetConnectionCoroutine(TestResult testResult, PatchKit.API.Async.AsyncCancellationToken cancellationToken)
        {
            var www = new WWW(TestingUrl);

            float time = Time.realtimeSinceStartup;

            while(Time.realtimeSinceStartup - time < Timeout && !www.isDone && !cancellationToken.IsCancellationRequested)
            {
                yield return null;
            }

            testResult.Result = www.isDone && string.IsNullOrEmpty(www.error);

            www.Dispose();
        }
    }
}