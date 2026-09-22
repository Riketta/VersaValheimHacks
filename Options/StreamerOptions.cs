namespace VersaValheimHacks.Options
{
    internal class StreamerOptions
    {
        /// <summary>
        /// While StreamerMode is on, the skill menu's numbers are rendered
        /// multiplied by this factor — 0.5 shows 50 for a real 100. The
        /// level labels, the absolute-level bar fills and the total/cap line
        /// all scale, so nothing reads as maxed. The within-level XP progress
        /// bar stays real. 1 or higher shows real values.
        /// </summary>
        public float SkillLabelMultiplier { get; set; } = 0.5f;
    }
}
