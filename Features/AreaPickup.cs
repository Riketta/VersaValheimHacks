using System;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Picking one pickable (berry bush, mushroom, stone pile...) picks all identical
    /// pickables in the configured radius.
    /// </summary>
    internal static class AreaPickup
    {
        private static bool _picking;

        public static void PickNearby(Pickable picked, Humanoid character, bool repeat, bool alt)
        {
            float radius = GlobalState.Config.PickableOptions.AreaPickupRadius;
            if (radius == 0f || _picking)
                return;

            _picking = true;
            try
            {
                foreach (var pickable in Resources.FindObjectsOfTypeAll<Pickable>())
                {
                    if (pickable.name != picked.name)
                        continue;

                    try
                    {
                        float distance = picked.transform.position.DistanceTo(pickable.transform.position);
                        if (distance < radius && pickable.CanBePicked())
                            pickable.Interact(character, repeat, alt);
                    }
                    catch (Exception ex)
                    {
                        HarmonyLog.Log($"[AreaPickup] Exception: {ex}.");
                    }
                }
            }
            finally
            {
                _picking = false;
            }
        }
    }
}
