using System;
using System.Collections.Concurrent;
using Exiled.API.Features;
using UnityEngine;

namespace CedMod.Addons.QuerySystem
{
    public class ThreadDispatcher : MonoBehaviour
    {
        public void Start()
        {
            Log.Debug("ThreadDispatcher started", CedModMain.Singleton.Config.QuerySystem.Debug);
        }

        public static ConcurrentQueue<Action> ThreadDispatchQueue = new ConcurrentQueue<Action>();
        public void Update()
        {
            if (ThreadDispatchQueue.TryDequeue(out Action action))
            {
                Log.Debug($"Invoking action", CedModMain.Singleton.Config.QuerySystem.Debug);
                action.Invoke();
                Log.Debug($"Action Invoked", CedModMain.Singleton.Config.QuerySystem.Debug);
            }
        }
    }
}