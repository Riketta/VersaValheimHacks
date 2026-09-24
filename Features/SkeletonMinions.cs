using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Friendly skeletons (staff summons): raised summon limit and a fixed loadout
    /// instead of a random one (CapsLock ON = sword + shield, OFF = bow).
    /// </summary>
    internal static class SkeletonMinions
    {
        public static readonly MethodInfo GiveDefaultItemMethod = AccessTools.Method(typeof(Humanoid), "GiveDefaultItem");

        private const string FriendlySkeletonName = "Skeleton_Friendly(Clone)";

        private static GameObject _bow;
        private static GameObject _sword;
        private static GameObject _shield;

        /// <summary>Patch prefix returns false to replace the random default items.</summary>
        public static bool OverrideDefaultItems(Humanoid humanoid)
        {
            if (humanoid.name != FriendlySkeletonName)
                return true;

            try
            {
                CacheKnownItems(humanoid);

                if (!IsFollowingLocalPlayer(humanoid))
                    return true;

                NotifySummoned();

                var giveDefaultItem = AccessTools.MethodDelegate<Action<GameObject>>(GiveDefaultItemMethod, humanoid);
                if (WindowsManager.IsCapsLockOn)
                {
                    if (_sword != null)
                        giveDefaultItem(_sword);

                    if (_shield != null)
                        giveDefaultItem(_shield);
                }
                else if (_bow != null)
                {
                    giveDefaultItem(_bow);
                }

                return false;
            }
            catch (Exception ex)
            {
                // Degrade to vanilla loadout instead of leaving the skeleton empty-handed.
                HarmonyLog.Log($"[SkeletonMinions] Exception: {ex}.");
                return true;
            }
        }

        public static void OverrideSummonLimit(ref int maxInstances)
        {
            int limit = GlobalState.Config.GodModeOptions.SkeletonSummonLimit;
            if (limit <= 0)
            {
                HarmonyLog.Log($"[SkeletonMinions] Summon limit {limit} = off - passthrough (vanilla {maxInstances}).");
                return;
            }

            HarmonyLog.Log($"[SkeletonMinions] Summon limit: {maxInstances} -> {limit}.");
            maxInstances = limit;
        }

        /// <summary>
        /// Counts the local player's living summoned skeletons (including the
        /// one that just spawned - it is already registered and following by
        /// the time GiveDefaultItems runs) and reports its number.
        /// </summary>
        private static void NotifySummoned()
        {
            try
            {
                int count = 0;
                foreach (Character character in Character.GetAllCharacters())
                {
                    if (character == null || character.IsDead())
                        continue;

                    if (!character.name.StartsWith("Skeleton_Friendly", StringComparison.Ordinal))
                        continue;

                    if (IsFollowingLocalPlayer(character.GetComponent<Humanoid>()))
                        count++;
                }

                NotificationManager.Notification($"Skeleton number {count} summoned.", MessageHud.MessageType.TopLeft);
                HarmonyLog.Log($"[SkeletonMinions] Skeleton summoned: number {count} following the local player.");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[SkeletonMinions] NotifySummoned exception: {ex}.");
            }
        }

        private static void CacheKnownItems(Humanoid humanoid)
        {
            if (_bow is null || _sword is null)
            {
                HarmonyLog.Log("[SkeletonMinions] Caching friendly skeleton weapons...");
                foreach (var weapon in humanoid.m_randomWeapon)
                {
                    if (weapon is null)
                        continue;

                    switch (weapon.name)
                    {
                        case "skeleton_bow2":
                            _bow = weapon;
                            break;

                        case "skeleton_sword2":
                            _sword = weapon;
                            break;
                    }
                }
            }

            if (_shield is null)
            {
                HarmonyLog.Log("[SkeletonMinions] Caching friendly skeleton shields...");
                foreach (var shield in humanoid.m_randomShield)
                {
                    if (shield is null)
                        continue;

                    if (shield.name == "ShieldBronzeBuckler")
                        _shield = shield;
                }
            }
        }

        private static bool IsFollowingLocalPlayer(Humanoid humanoid)
        {
            var monster = humanoid.GetComponent<MonsterAI>();
            var followTarget = monster != null ? monster.GetFollowTarget() : null;

            Player player = followTarget != null ? followTarget.GetComponent<Player>() : null;
            string targetName = player != null ? player.GetPlayerName() : "none";
            string playerName = GlobalState.Player?.GetPlayerName() ?? "";

            HarmonyLog.Log($"[SkeletonMinions] Target: {targetName}; Player: {playerName}.");
            return targetName == playerName;
        }
    }
}
