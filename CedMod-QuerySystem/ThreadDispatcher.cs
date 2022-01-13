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
            Log.Debug("ThreadDispatcher started", QuerySystem.Singleton.Config.Debug);
        }

        public static ConcurrentQueue<Action> ThreadDispatchQueue = new ConcurrentQueue<Action>();
        public void Update()
        {
            if (ThreadDispatchQueue.TryDequeue(out Action action))
            {
                Log.Debug($"Invoking action", QuerySystem.Singleton.Config.Debug);
                action.Invoke();
                Log.Debug($"Action Invoked", QuerySystem.Singleton.Config.Debug);
            }
        }
    }
}