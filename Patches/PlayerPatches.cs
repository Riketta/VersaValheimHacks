using HarmonyLib;
using System.Collections.Generic;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    internal static class PlayerPatches
    {
        [HarmonyPatch(typeof(Player.Food), nameof(Player.Food.CanEatAgain))]
        internal class CanEatAgain
        {
            private static void Postfix(ref bool __result) => BetterEating.AllowReEating(ref __result);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
        internal class EatFood
        {
            private static void Postfix(Player __instance) => BetterEating.ResetFoodTimers(__instance);
        }

        [HarmonyPatch(typeof(Player), "StartGuardianPower")]
        internal class StartGuardianPower
        {
            private static void Prefix(ref float ___m_guardianPowerCooldown) => BetterPowers.SuppressCooldown(ref ___m_guardianPowerCooldown);

            private static void Postfix(ref float ___m_guardianPowerCooldown) => BetterPowers.RestoreCooldown(ref ___m_guardianPowerCooldown);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.ActivateGuardianPower))]
        internal class ActivateGuardianPower
        {
            private static void Prefix(ref float ___m_guardianPowerCooldown) => BetterPowers.SuppressCooldown(ref ___m_guardianPowerCooldown);

            private static void Postfix(Player __instance, ref float ___m_guardianPowerCooldown, StatusEffect ___m_guardianSE)
            {
                BetterPowers.ApplyExtraPowers(__instance, ___m_guardianSE);
                BetterPowers.RestoreCooldown(ref ___m_guardianPowerCooldown);
            }
        }

        [HarmonyPatch(typeof(Player), "SetCrouch")]
        internal class SetCrouch
        {
            private static void Postfix(Player __instance, bool crouch) => PlayerState.Update(__instance, crouch);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.IsEncumbered))]
        internal class IsEncumbered
        {
            private static void Postfix(ref bool __result) => NeverEncumbered.ForceNotEncumbered(ref __result);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetMaxCarryWeight))]
        internal class GetMaxCarryWeight
        {
            private static void Postfix(ref float __result) => NeverEncumbered.ScaleMaxCarryWeight(ref __result);
        }

        [HarmonyPatch(typeof(Player), "UpdatePlacement")]
        internal class UpdatePlacement
        {
            private static void Postfix(int ___m_placeRotation) => PlantBuilding.NotifyPlacementAngle(___m_placeRotation);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.NoCostCheat))]
        internal class NoCostCheat
        {
            private static void Postfix(ref bool __result) => FreeCrafting.ForceNoCost(ref __result);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
        internal class OnDeath
        {
            private static void Prefix(Player __instance) => NoDeathPenalties.BackupFoods(__instance);

            private static void Postfix(Player __instance) => NoDeathPenalties.RestoreFoods(__instance);
        }

        [HarmonyPatch(typeof(Player), "UpdateKnownRecipesList")]
        internal class UpdateKnownRecipesList
        {
            private static bool Prefix(Player __instance) => RecipeUnlocker.UnlockAll(__instance);
        }
    }
}
