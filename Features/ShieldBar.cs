using HarmonyLib;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Custom shield durability bar, drawn next to the vanilla health bar.
    /// The dark background always spans the max absorb value ("100% is 100%"),
    /// the cyan fill spans the remaining fraction, and the label shows
    /// remaining/max (e.g. "540/700"). Bar length scales with max durability on
    /// the vanilla health bar scale (32px per 25 points, min 138). Hidden while
    /// streamer mode is on.
    /// </summary>
    internal static class ShieldBar
    {
        public static readonly FieldInfo TotalAbsorbField = AccessTools.Field(typeof(SE_Shield), "m_totalAbsorbDamage");
        public static readonly FieldInfo DamageField = AccessTools.Field(typeof(SE_Shield), "m_damage");

        private const float Margin = 8f;
        private const float MinWidth = 138f;

        private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color FillColor = Color.cyan;

        private static GameObject _container;
        private static RectTransform _rect;
        private static RectTransform _fill;
        private static TMP_Text _label;

        public static void UpdateBar(Hud hud)
        {
            bool hidden = GlobalState.Config is { StreamerMode: true };
            SE_Shield shield = hidden ? null : FindShield(GlobalState.Player ?? Player.m_localPlayer);

            if (shield is null)
            {
                if (_container != null && _container.activeSelf)
                    _container.SetActive(false);
                return;
            }

            if (_container == null)
                Create(hud);

            float total = (float)TotalAbsorbField.GetValue(shield);
            float damage = (float)DamageField.GetValue(shield);
            float remaining = Mathf.Max(total - damage, 0f);
            float fraction = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;

            var healthRect = (RectTransform)hud.m_healthBarRoot.transform;

            // Bar length scales with max durability on the vanilla health bar scale
            // (32px per 25 points, min 138) so it is directly comparable to the HP bar.
            float width = Mathf.Max(MinWidth, Mathf.Ceil(total / 25f * 32f));
            _rect.anchoredPosition = healthRect.anchoredPosition + new Vector2(healthRect.rect.width + Margin, 0f);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            // Background = 100% (max value), fill = remaining fraction of it.
            _fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * fraction);

            _container.SetActive(true);

            if (_label != null)
                _label.text = $"{remaining:0}/{total:0}";
        }

        private static SE_Shield FindShield(Player player)
        {
            if (player is null)
                return null;

            foreach (var statusEffect in player.GetSEMan().GetStatusEffects())
                if (statusEffect is SE_Shield shield)
                    return shield;

            return null;
        }

        private static void Create(Hud hud)
        {
            var healthRect = (RectTransform)hud.m_healthBarRoot.transform;

            _container = new GameObject("ShieldDurabilityBar", typeof(RectTransform));
            _rect = (RectTransform)_container.transform;
            _rect.SetParent(healthRect.parent, false);
            _rect.anchorMin = healthRect.anchorMin;
            _rect.anchorMax = healthRect.anchorMax;
            _rect.pivot = healthRect.pivot;
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, healthRect.rect.height);
            _container.SetActive(false);

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.SetParent(_rect, false);
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            background.GetComponent<Image>().color = BackgroundColor;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            _fill = (RectTransform)fill.transform;
            _fill.SetParent(_rect, false);
            _fill.anchorMin = new Vector2(0f, 0f);
            _fill.anchorMax = new Vector2(0f, 1f);
            _fill.pivot = new Vector2(0f, 0.5f);
            _fill.anchoredPosition = Vector2.zero;
            fill.GetComponent<Image>().color = FillColor;

            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            _label = label.GetComponent<TextMeshProUGUI>();
            _label.transform.SetParent(_rect, false);
            _label.rectTransform.anchorMin = Vector2.zero;
            _label.rectTransform.anchorMax = Vector2.one;
            _label.rectTransform.offsetMin = Vector2.zero;
            _label.rectTransform.offsetMax = Vector2.zero;
            _label.font = hud.m_healthText.font;
            _label.fontSize = Mathf.Clamp(healthRect.rect.height * 0.5f, 8f, 18f);
            _label.alignment = TextAlignmentOptions.Center;
            _label.enableWordWrapping = false;
            _label.color = Color.white;
            _label.text = "";
        }
    }
}
