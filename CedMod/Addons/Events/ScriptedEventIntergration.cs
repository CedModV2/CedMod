namespace CedMod.Addons.Events
{
    public class ScriptedEventIntergration: IEvent
    {
        public string FilePath { get; set; }
        public string EventName
        {
            get => eventName;
        }
        public string eventName;
        public string EvenAuthor
        {
            get => eventAuthor;
        }
        public string eventAuthor;
        public string EventDescription { get; set; } = "Scripted-Event";
        public string EventPrefix
        {
            get => $"SCRPTEV-{eventPrefix}";
        }
        public string eventPrefix;
        public bool BulletHolesAllowed { get; set; }
        public IEventConfig Config
        {
            get => config;
        }
        public IEventConfig config;
        public void PrepareEvent()
        {
#if EXILED
            var script = ScriptedEvents.API.Helpers.ScriptHelper.ReadScript(eventName);
            ScriptedEvents.API.Helpers.ScriptHelper.RunScript(script);
#endif
        }

        public void StopEvent()
        {
#if EXILED
            ScriptedEvents.API.Helpers.ScriptHelper.StopAllScripts();
#endif
        }
    }

    public class ScriptedEventConfig : IEventConfig
    {
        public bool IsEnabled { get; set; }
    }
}