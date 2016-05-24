using System;
using UnityEngine;

public static class PatchKitAsyncUtilities
{
    private class AsyncWaitYieldInstruction : CustomYieldInstruction
    {
        private readonly IAsyncResult _asyncResult;

        public AsyncWaitYieldInstruction(IAsyncResult asyncResult)
        {
            _asyncResult = asyncResult;
        }

        public override bool keepWaiting
        {
            get { return !_asyncResult.IsCompleted; }
        }
    }

    public static CustomYieldInstruction Wait(this IAsyncResult @this)
    {
        return new AsyncWaitYieldInstruction(@this);
    }
}
