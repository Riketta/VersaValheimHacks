using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class SkillsOptions
    {
        public float PreFiftyMultiplier { get; set; } = 10f;
        public float PostFiftyMultiplier { get; set; } = 3f;

        /// <summary>
        /// Fraction of the vanilla death skill drain kept on death
        /// (0 = no drain, 1 = vanilla 25% loss).
        /// </summary>
        public float DeathDrainMultiplier { get; set; } = 0.25f;
    }
}
