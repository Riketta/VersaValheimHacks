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
        /// <summary>
        /// Fraction of each hit the magic shield takes: 0.65 = shield drains
        /// at 65% speed (~1.5× longer), 0 = indestructible, outside 0..1 =
        /// vanilla (full drain).
        /// </summary>
        public float ShieldDamageMultiplier { get; set; } = 0.65f;

        /// <summary>
        /// Restore the shield's full durability every time it is applied or
        /// recast (vanilla keeps accumulated damage, so recasting used to
        /// hand you a full-duration broken shield). false = vanilla behaviour.
        /// </summary>
        public bool RefreshDurabilityOnCast { get; set; } = true;

        /// <summary>
        /// Tint of the magic shield bubble VFX as a hex string (#RRGGBB or
        /// #RRGGBBAA). Empty or invalid = vanilla color. Skipped in streamer
        /// mode so the game stays vanilla-looking on stream.
        /// </summary>
        public string ShieldBubbleColorHex { get; set; } = "#40E0FF";

        public float MapRevealRadiusMultiplier { get; set; } = 3f;

        /// <summary>
        /// Cap for simultaneously summoned friendly skeletons
        /// (StaffOfSkeletons minions). 0 or negative = vanilla (no override).
        /// </summary>
        public int SkeletonSummonLimit { get; set; } = 9;

        /// <summary>
        /// Perfect-block (parry) window multiplier. Vanilla window is 0.25 s
        /// (hardcoded); 2 = 0.5 s, 0.5 = 0.125 s. 1 = vanilla (feature off).
        /// </summary>
        public float ParryWindowMultiplier { get; set; } = 1f;
    }
}
