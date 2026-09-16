using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class BetterEatingOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Extended food duration in seconds after every bite
        /// (0 = vanilla burn times, override disabled).
        /// </summary>
        public float ExtendedFoodDuration { get; set; } = 1800f; // 30 min ≈ vanilla food burn times
        /// <summary>
        /// Persistent food loadout (prefab names): saved by the SaveFood
        /// hotkey, applied by the ApplySavedFood hotkey at natural values.
        /// </summary>
        public List<string> SavedFood { get; set; } = new List<string>();

        public float HealingMultiplier { get; set; } = 2.5f;
        public bool FoodCycling { get; set; } = true;
    }
}
