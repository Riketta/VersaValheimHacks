using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Skills;

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
                if (!GlobalState.ToggleHacks)
                    return;

                //HarmonyLog.Log($"[{Prefix}.Prefix] Factor (real): {factor}.");
                if (___m_level <= 50)
                    factor *= GlobalState.Config.SkillsOptions.PreFiftyMultiplier;
                else
                    factor *= GlobalState.Config.SkillsOptions.PostFiftyMultiplier;
            }
        }

        //[HarmonyPatch(typeof(Skills), "RaiseSkill")]
        internal class RaiseSkill
        {
            private const string Prefix = "Skills.RaiseSkill";

            [HarmonyPrefix]
            public static void MaxUpgradedSkill(SkillType skillType, float factor, Dictionary<SkillType, Skill> ___m_skillData)
            {
                if (!GlobalState.ToggleHacks)
                    return;

                HarmonyLog.Log($"[{Prefix}.Prefix] Maxing skill: {skillType}.");
                foreach (KeyValuePair<SkillType, Skill> skillDataPair in ___m_skillData)
                    if (skillDataPair.Key == skillType)
                    {
                        skillDataPair.Value.m_level = 99;
                        break;
                    }
            }
        }
    }
}
