namespace CedMod.EventManager
{
    public interface IEvent
    {
        public string EventName { get; }
        public string EvenAuthor { get; }
        public string EventDescription { get; set; }
        public string EventPrefix { get; }
        public bool OverrideWinConditions { get; }
        public bool BulletHolesAllowed { get; set; }

        public bool CanRoundEnd();
        public void PrepareEvent();
        public void StopEvent();
    }
}