namespace CedMod.Addons.Events.Interfaces
{
    public interface IEvent
    {
        string EventName { get; }
        string EvenAuthor { get; }
        string EventDescription { get; set; }
        string EventPrefix { get; }
        IEventConfig Config { get; }
        
        void PrepareEvent();
        void StopEvent();
    }
}