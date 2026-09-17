using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// A clone of the vanilla health bar with a fixed length, shown next to the
    /// HP bar while a shield status effect (block absorb) is active. The fill
    /// normalizes remaining/max absorb damage and the label displays it, e.g.
    /// "540/700". In streamer mode the bar stays visible; the shield takes at
    /// least 50% damage (see ShieldTuning).
    /// </summary>
    internal static class ShieldBar
    {
        public static readonly FieldInfo TotalAbsorbField = AccessTools.Field(typeof(SE_Shield), "m_totalAbsorbDamage");
        public static readonly FieldInfo DamageField = AccessTools.Field(typeof(SE_Shield), "m_damage");

        private const float Margin = 8f;
        private const float Width = 140f;

        private static readonly Color ShieldColor = Color.cyan;

        private static GameObject _container;
        private static RectTransform _rect;
        private static readonly List<GuiBar> _bars = new List<GuiBar>();
        private static TMP_Text _label;

        public static void UpdateBar(Hud hud)
        {
            if (!GlobalState.Config.HudOptions.ShieldDurabilityBar)
            {
                if (_container != null && _container.activeSelf)
                    _container.SetActive(false);
                return;
            }

            SE_Shield shield = FindShield(GlobalState.Player ?? Player.m_localPlayer);

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
            _rect.anchoredPosition = healthRect.anchoredPosition + new Vector2(healthRect.rect.width + Margin, 0f);

            // Fixed-length bar: the fill normalizes the value (remaining / max).
            foreach (var bar in _bars)
            {
                bar.SetMaxValue(total);
                bar.SetValue(remaining);
                bar.SetColor(ShieldColor);
            }

            if (_label != null)
                _label.text = $"{remaining:0}/{total:0}";

            _container.SetActive(true);
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
            var source = hud.m_healthBarRoot;
            var sourceRect = (RectTransform)source.transform;

            _container = Object.Instantiate(source.gameObject, sourceRect.parent);
            _container.name = "ShieldDurabilityBar";

            _rect = (RectTransform)_container.transform;
            _rect.anchorMin = sourceRect.anchorMin;
            _rect.anchorMax = sourceRect.anchorMax;
            _rect.pivot = sourceRect.pivot;
            _rect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(sourceRect.rect.width + Margin, 0f);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sourceRect.rect.height);

            _bars.Clear();
            _bars.AddRange(_container.GetComponentsInChildren<GuiBar>(true));

            foreach (var bar in _bars)
            {
                bar.SetColor(ShieldColor);
                bar.SetWidth(Width);
            }

            foreach (var bar in _bars)
                bar.SetColor(ShieldColor);

            _label = _container.GetComponentInChildren<TMP_Text>(true);
            if (_label != null)
            {
                _label.fontSize = 16f;
                _label.enableWordWrapping = false;
            }

            _container.SetActive(false);
        }
    }
}
