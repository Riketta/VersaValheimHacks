using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// A clone of the vanilla health bar, shown next to it while a shield status
    /// effect (block absorb) is active. Displays remaining/total absorb damage,
    /// e.g. "540/700". Hidden while streamer mode is on.
    /// </summary>
    internal static class ShieldBar
    {
        public static readonly FieldInfo TotalAbsorbField = AccessTools.Field(typeof(SE_Shield), "m_totalAbsorbDamage");
        public static readonly FieldInfo DamageField = AccessTools.Field(typeof(SE_Shield), "m_damage");

        private const float Width = 140f;
        private const float Margin = 8f;

        private static GameObject _container;
        private static RectTransform _rect;
        private static readonly List<GuiBar> _bars = new List<GuiBar>();
        private static readonly List<Image> _barImages = new List<Image>();
        private static TMP_Text _label;

        private static readonly Color ShieldColor = Color.cyan;

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

            // Bar length scales with max durability on the vanilla health bar scale
            // (32px per 25 points, min 138) so it is directly comparable to the HP bar.
            float size = Mathf.Max(138f, Mathf.Ceil(total / 25f * 32f));
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);

            _container.SetActive(true);
            foreach (var bar in _bars)
            {
                bar.SetMaxValue(total);
                bar.SetValue(remaining);
            }

            foreach (var image in _barImages)
                image.color = ShieldColor;

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

        private static void CollectBarImages(GuiBar bar)
        {
            // GuiBar lives in assembly_guiutils (no decompile handy) - grab its
            // Image fields by type instead of relying on field names.
            var type = bar.GetType();
            while (type != null && type != typeof(MonoBehaviour))
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.FieldType != typeof(Image))
                        continue;

                    if (field.GetValue(bar) is Image image)
                        _barImages.Add(image);
                }

                type = type.BaseType;
            }
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
                CollectBarImages(bar);

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
