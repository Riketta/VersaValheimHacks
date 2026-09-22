namespace VersaValheimHacks.Options
{
    internal class StreamerOptions
    {
        /// <summary>
        /// While StreamerMode is on, the skill menu's numbers (each skill's
        /// level label and the total/cap line) are rendered multiplied by
        /// this factor — 0.5 shows 50 for a real 100. Progress bars always
        /// stay real. 1 or higher shows real values.
        /// </summary>
        public float SkillLabelMultiplier { get; set; } = 0.5f;
    }
}
