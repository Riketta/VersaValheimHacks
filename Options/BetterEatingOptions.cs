using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BetterEatingOptions
    {
        public bool Enabled { get; set; } = true;
        public float FoodBuffDuration { get; set; } = 1800f; // 30 min ≈ vanilla food burn times
        public float HealingMultiplier { get; set; } = 2.5f;
        public bool FoodCycling { get; set; } = true;
    }
}
