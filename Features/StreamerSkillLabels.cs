using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Streamer mode: renders the skill menu's numbers (each skill's level
    /// label and the total/cap line) multiplied by the configured factor, so
    /// viewers can't read real skill values. The progress bars and the
    /// temporary "+bonus" labels are left untouched, and the spoof only
    /// exists on screen - the real skill values are never modified.
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
                    return;

                float factor = GlobalState.Config.StreamerOptions.SkillLabelMultiplier;
                if (factor <= 0f || factor >= 1f)
                    return;

                List<Skills.Skill> skills = player.GetSkills().GetSkillList();
                if (ElementsField?.GetValue(dialog) is List<GameObject> elements)
                {
                    int count = Mathf.Min(skills.Count, elements.Count);
                    for (int i = 0; i < count; i++)
                    {
                        GameObject element = elements[i];
                        if (element == null || !element.activeSelf)
                            continue;

                        Transform labelTransform = Utils.FindChild(element.transform, "leveltext");
                        TMP_Text label = labelTransform ? labelTransform.GetComponent<TMP_Text>() : null;
                        if (label != null)
                            label.text = Mathf.FloorToInt(skills[i].m_level * factor).ToString();
                    }
                }

                Skills playerSkills = player.GetSkills();
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
    }
}
