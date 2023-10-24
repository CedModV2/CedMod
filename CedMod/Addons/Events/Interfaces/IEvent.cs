namespace CedMod.Addons.Events.Interfaces
{
    public interface IEvent
    {
        string EventName { get; }
        string EventAuthor { get; }
        string EventDescription { get; set; }
        string EventPrefix { get; }
        IEventConfig Config { get; }
        
        void PrepareEvent();
        void StopEvent();
    }
}