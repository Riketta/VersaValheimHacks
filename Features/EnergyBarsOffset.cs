using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Moves the energy bars (stamina, eitr, adrenaline) vertically by a
    /// configurable pixel offset. The game rewrites each bar's position
    /// every frame (UpdateStamina / UpdateAdrenaline / UpdateEitr set
    /// hardcoded base values), so postfixing those and adding the same delta
    /// keeps the bars moving together and preserves their relative
    /// arrangement in every HUD mode (normal / build / ship). 0 = vanilla
    /// position. The health bar block is not touched.
    /// </summary>
    internal static class EnergyBarsOffset
    {
        private const string Prefix = "EnergyBarsOffset";

        private static float Offset => GlobalState.Config.HudOptions.EnergyBarsVerticalOffset;

        public static void UpdateStamina(Hud hud) => OffsetBar(hud?.m_staminaBar2Root);

        public static void UpdateAdrenaline(Hud hud) => OffsetBar(hud?.m_adrenalineBarRoot);

        public static void UpdateEitr(Hud hud) => OffsetBar(hud?.m_eitrBarRoot);

        private static void OffsetBar(RectTransform root)
        {
            float offset = Offset;
            if (root == null || offset == 0f)
                return;

            // The game just reset this bar's base position this frame, so
            // adding the delta never accumulates while the bar is shown.
            // (Adrenaline skips its base write while empty - the bar is
            // hidden then, and the base reset on the next show corrects it.)
            root.anchoredPosition += new Vector2(0f, offset);
        }
    }
}
