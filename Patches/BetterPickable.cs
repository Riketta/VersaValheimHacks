using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MeleeWeaponTrail;
using UnityEngine;

namespace VersaValheimHacks.Patches
{
    internal class BetterPickable
    {
        //[HarmonyPatch(typeof(ItemDrop), "Pickup")]
        internal class ItemDropPickup
        {
            private const string Prefix = "ItemDrop.Pickup";

            [HarmonyPostfix]
            public static void Debug()
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Pickup.");
            }
        }

        //[HarmonyPatch(typeof(ItemDrop), "CanPickup")]
        internal class ItemDropCanPickup
        {
            private const string Prefix = "ItemDrop.CanPickup";

            [HarmonyPostfix]
            public static void Debug()
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] CanPickup.");
            }
        }

        //[HarmonyPatch(typeof(Humanoid), "Pickup")]
        internal class HumanoidPickup
        {
            private const string Prefix = "Humanoid.Pickup";

            [HarmonyPostfix]
            public static void Debug()
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Pickup.");
            }
        }

        //[HarmonyPatch(typeof(Humanoid), "PickupPrefab")]
        internal class HumanoidPickupPrefab
        {
            private const string Prefix = "Humanoid.PickupPrefab";

            [HarmonyPostfix]
            public static void Debug()
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] PickupPrefab.");
            }
        }

        //[HarmonyPatch(typeof(Character), "ShowPickupMessage")]
        internal class CharacterShowPickupMessage
        {
            private const string Prefix = "Character.ShowPickupMessage";

            [HarmonyPostfix]
            public static void Debug()
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] ShowPickupMessage.");
                HarmonyLog.DumpStackTrace();
            }
        }

        //[HarmonyPatch(typeof(PickableItem), "GetHoverText")]
        internal class PickableItemGetHoverText
        {
            private const string Prefix = "PickableItem.GetHoverText";

            [HarmonyPostfix]
            public static void Debug(PickableItem __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] GetHoverText.");
                HarmonyLog.DumpStackTrace();
                HarmonyLog.Log($"[{Prefix}.Postfix] GetHoverText: {__instance.name} ({__instance.GetHoverName()} / {__instance.GetHoverText()}).");
            }
        }

        //[HarmonyPatch(typeof(ItemDrop), "GetHoverText")]
        internal class ItemDropGetHoverText
        {
            private const string Prefix = "ItemDrop.GetHoverText";

            [HarmonyPostfix]
            public static void Debug(ItemDrop __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] GetHoverText.");
                HarmonyLog.DumpStackTrace();
                HarmonyLog.Log($"[{Prefix}.Postfix] GetHoverText: {__instance.name} ({__instance.GetHoverName()} / {__instance.GetHoverText()}).");
            }
        }

        //[HarmonyPatch(typeof(Pickable), "GetHoverText")]
        internal class PickableGetHoverText
        {
            private const string Prefix = "Pickable.GetHoverText";

            [HarmonyPostfix]
            public static void Debug(Pickable __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] GetHoverText.");
                HarmonyLog.DumpStackTrace();
                HarmonyLog.Log($"[{Prefix}.Postfix] GetHoverText: {__instance.name} ({__instance.GetHoverName()} / {__instance.GetHoverText()}).");
            }
        }

        [HarmonyPatch(typeof(Pickable), "Interact")]
        internal class PickableInteract
        {
            private const string Prefix = "Pickable.Interact";

            public static bool SkipCalls = false;

            [HarmonyPostfix]
            public static void Debug(Pickable __instance, Humanoid character, bool repeat, bool alt, int ___m_enabled)
            {
                if (GlobalState.Config.PickableOptions.AreaPickupRadius == 0)
                    return;

                if (SkipCalls)
                {
                    HarmonyLog.Log($"[{Prefix}.Postfix] Skipping calls!");
                    return;
                }
                SkipCalls = true;

                //HarmonyLog.Log($"[{Prefix}.Postfix] Object: {__instance.name} ({__instance.GetHoverName()}); Prefab: {character.name} ({character.m_name}); Repeat: {repeat}; Alt: {alt}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Enabled: {___m_enabled}; Can be picked: {__instance.CanBePicked()}; Tag: {__instance.gameObject.tag}; Layer: {__instance.gameObject.layer}.");

                //HarmonyLog.Log($"[{Prefix}.Postfix] Untagged: {GameObject.FindGameObjectsWithTag("").Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Piece: {GameObject.FindObjectsOfType<Piece>().Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Piece All GO: {Resources.FindObjectsOfTypeAll<Piece>().Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Piece All: {Resources.FindObjectsOfTypeAll(typeof(Piece)).Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Plant: {GameObject.FindObjectsOfType<Plant>().Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Plant All GO: {Resources.FindObjectsOfTypeAll<Plant>().Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Plant All: {Resources.FindObjectsOfTypeAll(typeof(Plant)).Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Pickable: {GameObject.FindObjectsOfType<Pickable>().Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Pickable All GO: {Resources.FindObjectsOfTypeAll<Pickable>().Length}.");
                //HarmonyLog.Log($"[{Prefix}.Postfix] Pickable All: {Resources.FindObjectsOfTypeAll(typeof(Pickable)).Length}.");

                //HarmonyLog.Log($"[{Prefix}.Postfix] Iterating pickables.");
                var validPickables = Resources.FindObjectsOfTypeAll<Pickable>().Where(p => p.name == __instance.name); // && p.gameObject.layer == 16 && p.CanBePicked()
                foreach (var pickable in validPickables)
                {
                    try
                    {
                        float distance = __instance.transform.position.DistanceTo(pickable.transform.position);
                        bool isInRange = distance < GlobalState.Config.PickableOptions.AreaPickupRadius;
                        //HarmonyLog.Log($"[{Prefix}.Postfix] [Pickable] Name: {pickable.name} ({pickable.GetHoverName()}); Can be picked: {pickable.CanBePicked()}; Distance: {distance}; Is in range: {isInRange}; Layer: {pickable.gameObject.layer}; Tag: {pickable.gameObject.tag}.");
                        
                        //Component[] components = pickable.GetComponents(typeof(Component));
                        //foreach (Component component in components)
                        //    HarmonyLog.Log($"[{Prefix}.Postfix] > [+] Component: {component.name} ({component}.");
                        
                        if (isInRange && pickable.CanBePicked())
                        {
                            //HarmonyLog.Log($"[{Prefix}.Postfix] > Trying to pick...");
                            pickable.Interact(character, repeat, alt);
                        }
                    }
                    catch (Exception ex)
                    {
                        HarmonyLog.Log($"[{Prefix}.Postfix] > Exception: {ex}.");
                    }
                }

                SkipCalls = false;
            }
        }

        //[HarmonyPatch(typeof(Pickable), "Drop")]
        internal class PickableDrop
        {
            private const string Prefix = "Pickable.Drop";

            [HarmonyPostfix]
            public static void Debug(Pickable __instance, UnityEngine.GameObject prefab, int offset, int stack)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Object: {__instance.name} ({__instance.GetHoverName()}); Prefab: {prefab.name}; Stack: {stack}.");
            }
        }
    }
}
