using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class GodModeOptions
    {
        public bool FreeCraftingEnabled { get; set; } = true;
        public bool NeverEncumbered { get; set; } = true;
        public float CarryWeightMultiplier { get; set; } = 5f;
        public bool DisableMistlandsMist { get; set; } = true;
        public int SummonsLimit { get; set; } = 9;
        public float ShieldDamageMultiplier { get; set; } = 0.5f;
        public float MapRevealRadiusMultiplier { get; set; } = 3f;

        /// <summary>
        /// Perfect-block (parry) window multiplier. Vanilla window is 0.25 s
        /// (hardcoded); 2 = 0.5 s, 0.5 = 0.125 s. 1 = vanilla (feature off).
        /// </summary>
        public float ParryWindowMultiplier { get; set; } = 1f;
    }
}
