using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Small label right under the HUD health value: seconds until the next
    /// food healing tick. Vanilla heals every 10 s, the sum of the eaten
    /// foods' m_foodRegen scaled by status effects. Hidden while no eaten
    /// food provides regeneration.
    /// </summary>
    internal static class HealthRegenCountdown
    {
        private const float RegenInterval = 10f;

        private static readonly FieldInfo FoodRegenTimerField =
            AccessTools.Field(typeof(Player), "m_foodRegenTimer");

        private static readonly FieldInfo FoodsField =
            AccessTools.Field(typeof(Player), "m_foods");

        private static TMP_Text _label;

        public static void UpdateLabel(Hud hud)
        {
            try
            {
                if (!GlobalState.Config.HudOptions.HealthRegenCountdown)
                {
                    if (_label != null)
                        _label.gameObject.SetActive(false);
                    return;
                }

                Player player = GlobalState.Player ?? Player.m_localPlayer;
                if (player is null || hud is null || hud.m_healthText is null)
                    return;

                if (_label is null && !CreateLabel(hud.m_healthText))
                    return;

                bool hasRegen = HasFoodRegen(player);
                _label.gameObject.SetActive(hasRegen);
                if (!hasRegen)
                    return;

                float timer = FoodRegenTimerField != null
                    ? Convert.ToSingle(FoodRegenTimerField.GetValue(player))
                    : 0f;

                _label.text = $"{Mathf.Max(0f, RegenInterval - timer):0.0}s";
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[HealthRegenCountdown] Exception: {ex}.");
            }
        }

        private static bool HasFoodRegen(Player player)
        {
            if (!(FoodsField?.GetValue(player) is List<Player.Food> foods))
                return false;

            foreach (var food in foods)
            {
                if (food.m_item != null && food.m_item.m_shared != null && food.m_item.m_shared.m_foodRegen > 0f)
                    return true;
            }

            return false;
        }

        private static bool CreateLabel(TMP_Text healthText)
        {
            _label = UnityEngine.Object.Instantiate(healthText, healthText.transform.parent);
            _label.name = "HealthRegenCountdown";
            _label.fontSize = healthText.fontSize * 0.55f;
            _label.color = new Color(1f, 1f, 1f, 0.8f);
            _label.text = string.Empty;

            _label.rectTransform.anchoredPosition = healthText.rectTransform.anchoredPosition
                + new Vector2(0f, -healthText.rectTransform.rect.height * 0.9f);

            return true;
        }
    }
}
