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
        // A value of 0 keeps the vanilla duration for that part.
        public float RestDurationBase { get; set; } = 300f;
        public float RestDurationPerComfort { get; set; } = 60f;
    }
}
