using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BetterPowersOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// No cooldown for guardian powers: activating a power skips the
        /// vanilla cooldown, so it can be re-cast immediately after it ends.
        /// </summary>
        public bool NoCooldown { get; set; } = true;

        public bool StackAllBossPowers { get; set; } = true;

        /// <summary>
        /// Extra power duration in seconds (0 = vanilla power duration).
        /// </summary>
        public float Duration { get; set; } = 600f; // 10 min; powers auto-expire and re-cast is instant

        public Dictionary<string, bool> BuffExtraPowers { get; set; } = new Dictionary<string, bool>()
        {
            ["GP_Eikthyr"] = true,
            ["GP_TheElder"] = true,
            ["GP_Bonemass"] = true,
            ["GP_Moder"] = false,
            ["GP_Yagluth"] = false,
            ["GP_Queen"] = false,
            ["GP_Ashlands"] = false,
            ["GP_DeepNorth"] = false,
        };
    }
}
