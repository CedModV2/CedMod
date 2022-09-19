namespace CedMod.ApiModals
{
    /// <summary>
    /// Represents an event thingy???
    /// </summary>
    public struct EventModal
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The author of the event.
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// The description of the event.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The prefix of the event.
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Whether or not the event is enabled.
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// The queue position of the event.
        /// </summary>
        public int QueuePos { get; set; }
    }
}