using System;
using System.Collections.Concurrent;
using Exiled.API.Features;
using UnityEngine;

namespace CedMod.QuerySystem
{
    public class ThreadDispatcher : MonoBehaviour
    {
        public void Start()
        {
            Log.Debug("ThreadDispatcher started", CedModMain.Singleton.Config.ShowDebug);
        }

        public static ConcurrentQueue<Action> ThreadDispatchQueue = new ConcurrentQueue<Action>();
        public void Update()
        {
            if (ThreadDispatchQueue.TryDequeue(out Action action))
            {
                Log.Debug($"Invoking action", CedModMain.Singleton.Config.ShowDebug);
                action.Invoke();
                Log.Debug($"Action Invoked", CedModMain.Singleton.Config.ShowDebug);
            }
        }
    }
}