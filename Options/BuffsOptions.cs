using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BuffsOptions
    {
        /// <summary>
        /// Buff duration override gates. A disabled category keeps vanilla
        /// durations.
        /// </summary>
        public bool OverrideFood { get; set; } = true;
        public bool OverrideRest { get; set; } = true;
        public bool OverridePower { get; set; } = true;

        /// <summary>
        /// Buff auto-reset gates. A disabled auto-reset leaves the vanilla
        /// expiry behavior: food is removed when its timer ends and re-resting
        /// does not refresh rested. (The guardian-power no-cooldown switch
        /// lives in BetterPowersOptions.NoCooldown.)
        /// </summary>
        public bool FoodAutoReset { get; set; } = true;
        public bool RestAutoRefresh { get; set; } = true;

        // Vanilla: 300 s base + 60 s per comfort level.
        // A value of 0 keeps the vanilla duration for that part.
        public float RestDurationBase { get; set; } = 480f;
        public float RestDurationPerComfort { get; set; } = 60f;
    }
}
