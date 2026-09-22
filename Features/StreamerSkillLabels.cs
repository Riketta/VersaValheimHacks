using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Streamer mode: renders the skill menu's numbers multiplied by the
    /// configured factor, so viewers can't read real skill values. Each
    /// skill's level label and the two absolute-level bars (level fill and
    /// total fill) all scale by the same factor, so a row reads as
    /// "50 out of 100" instead of a maxed bar next to a small number.
    /// The within-level XP bar shows only progress toward the next point
    /// (no absolute value), so it stays real. Nothing is modified in memory;
    /// the real skill values are never touched.
    /// </summary>
    internal static class StreamerSkillLabels
    {
        private const string Prefix = "StreamerSkillLabels";

        private static readonly FieldInfo ElementsField =
            AccessTools.Field(typeof(SkillsDialog), "m_elements");

        public static void SpoofSkillMenu(SkillsDialog dialog, Player player)
        {
            try
            {
                if (!GlobalState.Config.StreamerMode)
                {
                    HarmonyLog.Log($"[{Prefix}] Skill menu open: streamer mode is off - labels stay real.");
                    return;
                }

                float factor = GlobalState.Config.StreamerOptions.SkillLabelMultiplier;
                if (factor <= 0f || factor >= 1f)
                {
                    HarmonyLog.Log($"[{Prefix}] Skill menu open: SkillLabelMultiplier {factor} = off - labels stay real.");
                    return;
                }

                List<Skills.Skill> skills = player.GetSkills().GetSkillList();
                Skills playerSkills = player.GetSkills();
                if (ElementsField?.GetValue(dialog) is List<GameObject> elements)
                {
                    int count = Mathf.Min(skills.Count, elements.Count);
                    for (int i = 0; i < count; i++)
                    {
                        GameObject element = elements[i];
                        if (element == null || !element.activeSelf)
                            continue;

                        Skills.Skill skill = skills[i];
                        float fakeLevel = skill.m_level * factor;

                        Transform labelTransform = Utils.FindChild(element.transform, "leveltext");
                        TMP_Text label = labelTransform ? labelTransform.GetComponent<TMP_Text>() : null;
                        if (label != null)
                            label.text = Mathf.FloorToInt(fakeLevel).ToString();

                        SetBar(element, "levelbar", fakeLevel / 100f);
                        SetBar(element, "levelbar_total", playerSkills.GetSkillLevel(skill.m_info.m_skill) * factor / 100f);
                    }
                }

                dialog.m_totalSkillText.text =
                    "<color=orange>" + Mathf.FloorToInt(playerSkills.GetTotalSkill() * factor).ToString("0") + "</color>" +
                    "<color=white> / </color>" +
                    "<color=orange>" + Mathf.FloorToInt(playerSkills.GetTotalSkillCap() * factor).ToString("0") + "</color>";

                HarmonyLog.Log($"[{Prefix}] Skill menu rendered at x{factor:0.##} (streamer mode).");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] SpoofSkillMenu exception: {ex}.");
            }
        }

        private static void SetBar(GameObject element, string childName, float fill)
        {
            Transform barTransform = Utils.FindChild(element.transform, childName);
            GuiBar bar = barTransform ? barTransform.GetComponent<GuiBar>() : null;
            if (bar != null)
                bar.SetValue(Mathf.Clamp01(fill));
        }
    }
}
