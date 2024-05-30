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
    }
}
