namespace CedMod.Addons.Events
{
    public interface IEvent
    {
        string EventName { get; }
        string EvenAuthor { get; }
        string EventDescription { get; set; }
        string EventPrefix { get; }
        bool OverrideWinConditions { get; }
        bool BulletHolesAllowed { get; set; }

        bool CanRoundEnd();
        void PrepareEvent();
        void StopEvent();
    }
}