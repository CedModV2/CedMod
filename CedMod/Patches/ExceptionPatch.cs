using System;
using System.IO;
using System.Net.Sockets;
using Exiled.Events.Extensions;
using HarmonyLib;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(Event), "LogException",
        new Type[] {typeof(Exception), typeof(string), typeof(string), typeof(string)})]
    public static class ExceptionPatch
    {
        public static void Postfix(Exception ex, string methodName, string sourceClassName, string eventName) => SentrySdk.CaptureException(ex);
    }
}