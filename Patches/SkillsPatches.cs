using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Skills.Skill), nameof(Skills.Skill.Raise))]
    internal class Skills_Skill_Raise
    {
        private static void Prefix(ref float factor, float ___m_level) => SkillTraining.ScaleGain(ref factor, ___m_level);
    }
}
