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
        public bool ApplyAllBuffs { get; set; } = true;
        public float Duration { get; set; } = 3 * 60 * 60;

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
