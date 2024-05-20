using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Patches
{
    internal class SkillsMultiplier
    {
        [HarmonyPatch(typeof(Skills.Skill), "Raise")]
        internal class Raise
        {
            private const string Prefix = "Skills.Skill.Raise";

            [HarmonyPrefix]
            public static void AddSkillsGainMultiplier(ref float factor, float ___m_level)
            {
                if (!GlobalState.EnableHacks)
                    return;

                //HarmonyLog.Log($"[{Prefix}.Prefix] Factor (real): {factor}.");
                if (___m_level <= 50)
                    factor *= 30f;
                else
                    factor *= 10f;
            }
        }
    }
}
