using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Soften death penalties: scale the death skill drain down, keep food
    /// buffs and keep beneficial status effects (rested, powers, demister...).
    /// Debuffs you died with are left to vanilla's wipe on purpose.
    /// </summary>
    internal static class NoDeathPenalties
    {
        public static readonly FieldInfo FoodsField = AccessTools.Field(typeof(Player), "m_foods");
        public static readonly FieldInfo TimeField = AccessTools.Field(typeof(StatusEffect), "m_time");

        private static readonly List<Player.Food> _foodBackup = new List<Player.Food>(3);
        private static readonly List<EffectSnapshot> _effectBackup = new List<EffectSnapshot>();

        private sealed class EffectSnapshot
        {
            public int Hash;
            public float Time;
            public float Ttl;
        }

        public static void ScaleDeathDrain(ref float factor)
        {
            if (!GlobalState.ToggleHacks)
                return;

            factor *= Math.Max(0f, GlobalState.Config.SkillsOptions.DeathDrainMultiplier);
        }

        public static void BackupFoods(Player player)
        {
            if (player is null)
                return;

            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            HarmonyLog.Log($"[NoDeathPenalties] Backing up {foods.Count} food buff(s).");

            _foodBackup.Clear();
            _foodBackup.AddRange(foods);
        }

        public static void RestoreFoods(Player player)
        {
            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            HarmonyLog.Log($"[NoDeathPenalties] Restoring {_foodBackup.Count} food buff(s).");

            foreach (var food in _foodBackup)
                foods.Add(food);

            _foodBackup.Clear();
            NotificationManager.Notification($"Death penalties: food & buffs preserved, {DescribeDrain()}.", MessageHud.MessageType.TopLeft);
        }

        public static void BackupStatusEffects(Player player)
        {
            if (player is null)
                return;

            _effectBackup.Clear();
            var seman = player.GetSEMan();
            if (seman is null)
                return;

            foreach (var se in seman.GetStatusEffects())
            {
                if (se is null || !IsPreservedBuff(se))
                    continue;

                _effectBackup.Add(new EffectSnapshot
                {
                    Hash = se.NameHash(),
                    Time = (float)TimeField.GetValue(se),
                    Ttl = se.m_ttl,
                });
            }
        }

        public static void RestoreStatusEffects(Player player)
        {
            if (_effectBackup.Count == 0)
                return;

            var seman = player.GetSEMan();
            if (seman is null)
            {
                _effectBackup.Clear();
                return;
            }

            HarmonyLog.Log($"[NoDeathPenalties] Restoring {_effectBackup.Count} status effect(s).");
            foreach (var snapshot in _effectBackup)
            {
                var se = seman.AddStatusEffect(snapshot.Hash, resetTime: false);
                if (se is null)
                    continue;

                // Preserve what was left of the effect instead of a fresh timer.
                se.m_ttl = snapshot.Ttl;
                TimeField.SetValue(se, snapshot.Time);
            }
            _effectBackup.Clear();
        }

        /// <summary>
        /// Only beneficial effects survive death: rested, wisplight demister,
        /// cozy, guardian powers (GP_*) and effects granting a positive
        /// attribute (e.g. frost resistance from mead). Everything else -
        /// including all debuffs - is left to vanilla's wipe.
        /// </summary>
        private static bool IsPreservedBuff(StatusEffect se)
        {
            return se is SE_Rested
                || se is SE_Demister
                || se is SE_Cozy
                || se.name.StartsWith("GP_")
                || se.HaveAttribute(StatusEffect.StatusAttribute.ColdResistance)
                || se.HaveAttribute(StatusEffect.StatusAttribute.SailingPower)
                || se.HaveAttribute(StatusEffect.StatusAttribute.TamingBoost);
        }

        private static string DescribeDrain()
        {
            if (!GlobalState.ToggleHacks)
                return "skill drain normal";

            float multiplier = Math.Max(0f, GlobalState.Config.SkillsOptions.DeathDrainMultiplier);
            return multiplier == 0f ? "no skill drain" : $"skill drain x{multiplier:0.##}";
        }
    }
}
