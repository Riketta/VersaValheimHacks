using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BuffsOptions
    {
        // Vanilla values: rested auto-refreshes on every rest.
        public float RestDurationBase { get; set; } = 300f;
        public float RestDurationPerComfort { get; set; } = 60f;
    }
}
