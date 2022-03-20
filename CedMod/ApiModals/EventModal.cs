namespace CedMod.ApiModals
{
    public class EventModal
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Prefix { get; set; }
        public bool Active { get; set; }
        public int QueuePos { get; set; }
    }
}