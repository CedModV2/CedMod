namespace CedMod.Addons.Events
{
    public interface IEvent
    {
        string EventName { get; }
        string EvenAuthor { get; }
        string EventDescription { get; set; }
        string EventPrefix { get; }
        bool BulletHolesAllowed { get; set; }
        IEventConfig Config { get; }
        
        void PrepareEvent();
        void StopEvent();
    }
}