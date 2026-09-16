using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class SkillsOptions
    {
        /// <summary>
        /// Skill XP gain multiplier while the skill is below level 50.
        /// </summary>
        public float GainMultiplierBelow50 { get; set; } = 10f;

        /// <summary>
        /// Skill XP gain multiplier while the skill is at or above level 50.
        /// </summary>
        public float GainMultiplierAbove50 { get; set; } = 3f;

        /// <summary>
        /// Fraction of the vanilla death skill drain kept on death
        /// (0 = no drain, 1 = vanilla 25% loss).
        /// </summary>
        public float DeathDrainMultiplier { get; set; } = 0.25f;
    }
}
