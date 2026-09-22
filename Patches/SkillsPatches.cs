using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Skills.Skill), nameof(Skills.Skill.Raise))]
    internal class Skills_Skill_Raise
    {
        private static void Prefix(ref float factor, float ___m_level) => SkillTraining.ScaleGain(ref factor, ___m_level);
    }

    [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
    internal class Skills_LowerAllSkills
    {
        private static void Prefix(ref float factor) => NoDeathPenalties.ScaleDeathDrain(ref factor);
    }

    [HarmonyPatch(typeof(SkillsDialog), nameof(SkillsDialog.Setup))]
    internal class SkillsDialog_Setup
    {
        private static void Postfix(SkillsDialog __instance, Player player) => StreamerSkillLabels.SpoofSkillMenu(__instance, player);
    }
}
