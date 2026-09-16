namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Multiplied skill XP gain (different multipliers below/above level 50).
    /// </summary>
    internal static class SkillTraining
    {
        public static void ScaleGain(ref float factor, float skillLevel)
        {
            if (!GlobalState.ToggleHacks)
                return;

            factor *= skillLevel <= 50
                ? GlobalState.Config.SkillsOptions.GainMultiplierBelow50
                : GlobalState.Config.SkillsOptions.GainMultiplierAbove50;
        }
    }
}
