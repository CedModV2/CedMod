﻿using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class HelloMessage
    {
        public bool SendStats { get; set; }
        public bool SendEvents { get; set; }
        public bool ExpEnabled { get; set; }
        public string Identity { get; set; }
        public bool SentinalPositions { get; set; }
    }
}