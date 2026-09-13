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

            CacheKnownItems(humanoid);

            if (!IsFollowingLocalPlayer(humanoid))
                return true;

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

        public static void OverrideSummonLimit(ref int maxInstances)
        {
            int limit = GlobalState.Config.GodModeOptions.SummonsLimit;
            HarmonyLog.Log($"[SkeletonMinions] Summon limit: {maxInstances} -> {limit}.");
            maxInstances = limit;
        }

        private static void CacheKnownItems(Humanoid humanoid)
        {
            if (_bow is null || _sword is null)
            {
                HarmonyLog.Log("[SkeletonMinions] Caching friendly skeleton weapons...");
                foreach (var weapon in humanoid.m_randomWeapon)
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

            if (_shield is null)
            {
                HarmonyLog.Log("[SkeletonMinions] Caching friendly skeleton shields...");
                foreach (var shield in humanoid.m_randomShield)
                    if (shield.name == "ShieldBronzeBuckler")
                        _shield = shield;
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
