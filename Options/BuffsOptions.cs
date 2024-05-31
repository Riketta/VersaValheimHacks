using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BuffsOptions
    {
        public float RestDurationBase { get; set; } = 5 * 60 * 60;
        public float RestDurationPerComfort { get; set; } = 1 * 60 * 60;
    }
}
