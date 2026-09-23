using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(SE_Shield), nameof(SE_Shield.Setup))]
    internal class SeShield_Setup
    {
        private static void Postfix(SE_Shield __instance) => ShieldTuning.OnShieldApplied(__instance);
    }

    [HarmonyPatch(typeof(SE_Shield), nameof(SE_Shield.SetLevel))]
    internal class SeShield_SetLevel
    {
        private static void Postfix(SE_Shield __instance) => ShieldTuning.OnShieldApplied(__instance);
    }

    // UpdateStatusEffect is declared on the base class, so the postfix runs
    // for every status effect; CheckExpiryWarning filters to SE_Shield.
    [HarmonyPatch(typeof(StatusEffect), nameof(StatusEffect.UpdateStatusEffect))]
    internal class StatusEffect_UpdateStatusEffect
    {
        private static void Postfix(StatusEffect __instance)
        {
            if (__instance is SE_Shield shield)
                ShieldTuning.CheckExpiryWarning(shield);
        }
    }

    [HarmonyPatch(typeof(SE_Shield), nameof(SE_Shield.OnDamaged))]
    internal class SeShield_OnDamaged
    {
        private static void Prefix(SE_Shield __instance, HitData hit, ref float ___m_damage, float ___m_totalAbsorbDamage)
            => ShieldTuning.CompensateShieldDamage(__instance, hit, ref ___m_damage, ___m_totalAbsorbDamage);
    }
}
