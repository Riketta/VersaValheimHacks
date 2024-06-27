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
    }
}
