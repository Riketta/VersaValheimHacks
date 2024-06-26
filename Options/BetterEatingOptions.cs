﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BetterEatingOptions
    {
        public bool Enabled { get; set; } = true;
        public float FoodBuffDuration { get; set; } = 24 * 60 * 60;
        public float HealingMultiplier { get; set; } = 2f;
    }
}
