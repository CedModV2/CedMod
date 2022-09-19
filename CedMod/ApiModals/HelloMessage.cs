using System.Collections.Generic;

namespace CedMod.ApiModals
{
    /// <summary>
    /// Represents an hello message.
    /// </summary>
    public class HelloMessage
    {
        /// <summary>
        /// Whether or not the server should send stats.
        /// </summary>
        public bool SendStats { get; set; }
        /// <summary>
        /// Whether or not the server should send events.
        /// </summary>
        public bool SendEvents { get; set; }
        /// <summary>
        /// Whether or not the server has Exp enabled.
        /// </summary>
        public bool ExpEnabled { get; set; }
    }
}