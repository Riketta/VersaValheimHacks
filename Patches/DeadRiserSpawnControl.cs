using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VersaValheimHacks.Patches
{
    internal class DeadRiserSpawnControl
    {
        //[HarmonyPatch(typeof(SpawnAbility), "Setup")]
        internal class SpawnAbilitySetup
        {
            private const string Prefix = "SpawnAbility.Setup";

            [HarmonyPostfix]
            public static void Debug(SpawnAbility __instance, Character owner, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Spawn: {__instance.name}; Owner: {owner.name} ({owner.m_name}); Item: [{item}]; Ammo: [{ammo}].");
                foreach (var go in __instance.m_spawnPrefab)
                {
                    // [SpawnAbility.Spawn.Postfix] Spawn.
                    // [SpawnAbility.Setup.Postfix] Spawn: staff_skeleton_spawn(Clone); Owner: Player(Clone) (Human); Item: [ItemData: stack: 1, quality: 4, Shared: SharedData: $item_staffskeleton, max stack: 1, attacks: Attack: staff_summon, Projectile / Attack: , Horizontal]; Ammo: [].
                    // > Object: Skeleton_Friendly; Layer: 9; Tag: Untagged.
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (Humanoid)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (MonsterAI)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (Tameable)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (UnityEngine.Transform)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (UnityEngine.CapsuleCollider)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (UnityEngine.Rigidbody)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (ZNetView)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (ZSyncTransform)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (ZSyncAnimation)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (VisEquipment)).
                    // > [+] Component: Skeleton_Friendly (Skeleton_Friendly (FootStep)).
                    HarmonyLog.Log($"[{Prefix}.Postfix] > Object: {go.name}; Layer: {go.layer}; Tag: {go.tag}.");

                    Component[] components = go.GetComponents(typeof(Component));
                    foreach (Component component in components)
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [+] Component: {component.name} ({component}).");

                    Humanoid humanoid = go.GetComponent<Humanoid>();
                    foreach (var humanoidWeapon in humanoid.m_randomWeapon)
                    {
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [!] Weapon: {humanoidWeapon.name}.");
                        
                        var itemDrop = humanoidWeapon.GetComponent<ItemDrop>();
                        HarmonyLog.Log($"[{Prefix}.Postfix] > ItemDrop: {itemDrop.name} ({itemDrop.m_itemData.m_shared.m_name}); Data: [{itemDrop.m_itemData}]; {itemDrop.m_itemData.m_quality}; {itemDrop.m_itemData.m_variant}.");
                    }

                    foreach (var humanoidShield in humanoid.m_randomShield)
                    {
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [!] Shield: {humanoidShield.name}.");

                        var itemDrop = humanoidShield.GetComponent<ItemDrop>();
                        HarmonyLog.Log($"[{Prefix}.Postfix] > ItemDrop: {itemDrop.name} ({itemDrop.m_itemData.m_shared.m_name}); Data: [{itemDrop.m_itemData}]; {itemDrop.m_itemData.m_quality}; {itemDrop.m_itemData.m_variant}.");
                    }

                    foreach (var humanoidItem in humanoid.m_randomItems)
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [!] Item: {humanoidItem.m_prefab.name} ({humanoidItem} / {humanoidItem.m_prefab}).");

                    if (humanoid.LeftItem != null)
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [!] Left Hand: {humanoid.LeftItem}.");
                    
                    if (humanoid.RightItem != null)
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [!] Right Hand: {humanoid.RightItem}.");


                    var currentWeapon = humanoid.GetCurrentWeapon();
                    if (currentWeapon != null)
                        HarmonyLog.Log($"[{Prefix}.Postfix] > [!] Current Weapon: {currentWeapon}.");
                }

                //HarmonyLog.DumpStackTrace();
            }
        }

        //[HarmonyPatch(typeof(SpawnAbility), "Spawn")]
        internal class SpawnAbilitySpawn
        {
            private const string Prefix = "SpawnAbility.Spawn";

            [HarmonyPostfix]
            public static void Debug()
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Spawn.");
                //HarmonyLog.DumpStackTrace();
            }
        }

        // Humanoid.EquipItem(global::ItemDrop.ItemData item, bool triggerEquipEffects = true)
        // Humanoid.EquipBestWeapon(global::Character targetCreature, global::StaticTarget targetStatic, global::Character hurtFriend, global::Character friend)

        //[HarmonyPatch(typeof(Humanoid), "GiveDefaultItem")]
        internal class HumanoidGiveDefaultItem
        {
            private const string Prefix = "Humanoid.GiveDefaultItem";

            [HarmonyPostfix]
            public static void Debug(Humanoid __instance, GameObject prefab)
            {
                //HarmonyLog.Log($"[{Prefix}.Postfix] Unit: {__instance.name}.");

                if (__instance.name != "Skeleton_Friendly(Clone)")
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] Unit: {__instance.name}; Item: {prefab.name}.");
            }
        }

        [HarmonyPatch(typeof(Humanoid), "GiveDefaultItems")]
        internal class HumanoidGiveDefaultItems
        {
            private const string Prefix = "Humanoid.GiveDefaultItems";

            public static MethodInfo GiveDefaultItemMethod = AccessTools.Method(typeof(Humanoid), "GiveDefaultItem");

            public static GameObject Bow;
            public static GameObject Sword;
            public static GameObject Shield;

            [HarmonyPrefix]
            public static bool SaveSkeletonDefaultItems(Humanoid __instance)
            {
                //HarmonyLog.Log($"[{Prefix}.Prefix] Unit: {__instance.name}.");

                if (__instance.name != "Skeleton_Friendly(Clone)")
                    return true;

                if (Bow is null || Sword is null)
                {
                    HarmonyLog.Log($"[{Prefix}.Prefix] Saving friendly skeleton weapons...");

                    for (int i = 0; i < __instance.m_randomWeapon.Length; i++)
                    {
                        HarmonyLog.Log($"[{Prefix}.Prefix] Item: {__instance.m_randomWeapon[i]}.");

                        switch (__instance.m_randomWeapon[i].name)
                        {
                            case "skeleton_bow2":
                                Bow = __instance.m_randomWeapon[i];
                                break;

                            case "skeleton_sword2":
                                Sword = __instance.m_randomWeapon[i];
                                break;
                        }
                    }
                }

                if (Shield is null)
                {
                    HarmonyLog.Log($"[{Prefix}.Prefix] Saving friendly skeleton shields...");

                    for (int i = 0; i < __instance.m_randomShield.Length; i++)
                    {
                        HarmonyLog.Log($"[{Prefix}.Prefix] Item: {__instance.m_randomShield[i]}.");

                        switch (__instance.m_randomShield[i].name)
                        {
                            case "ShieldBronzeBuckler":
                                Shield = __instance.m_randomShield[i];
                                break;
                        }
                    }
                }

                MonsterAI monster = __instance.GetComponent<MonsterAI>();
                GameObject followTarget = monster.GetFollowTarget();
                string targetName = "none";
                if (followTarget != null)
                {
                    Player player = followTarget.GetComponent<Player>();
                    targetName = player.GetPlayerName();
                }
                string playerName = GlobalState.Player?.GetPlayerName() ?? "";

                //HarmonyLog.Log($"[{Prefix}.Prefix] Monster: {__instance.name} ({__instance.m_name}); Follow target: {monster?.GetFollowTarget().name}.");
                HarmonyLog.Log($"[{Prefix}.Prefix] Target: {targetName}; Player: {playerName}.");

                if (targetName != playerName)
                    return true;

                var GiveDefaultItem = AccessTools.MethodDelegate<Action<GameObject>>(GiveDefaultItemMethod, __instance);
                if (WindowsManager.IsCapsLockOn)
                {
                    if (Sword != null)
                        GiveDefaultItem(Sword);

                    if (Shield != null)
                        GiveDefaultItem(Shield);
                }
                else if (Bow != null)
                    GiveDefaultItem(Bow);

                return false;
            }

            //[HarmonyPostfix]
            //public static void UpdateSkeletonDefaultItems(Humanoid __instance)
            //{
            //    HarmonyLog.Log($"[{Prefix}.Postfix] Updating unit items: {__instance.name} ({__instance.m_name}).");

            //    var GiveDefaultItem = AccessTools.MethodDelegate<Action<GameObject>>(GiveDefaultItemMethod, __instance);

            //    if (WindowsManager.IsCapsLockOn)
            //    {
            //        if (Sword != null)
            //            GiveDefaultItem(Sword);

            //        if (Shield != null)
            //            GiveDefaultItem(Shield);
            //    }
            //    else if (Bow != null)
            //        GiveDefaultItem(Bow);
            //}
        }

        [HarmonyPatch(typeof(Tameable), "UnsummonMaxInstances")]
        internal class TameableUnsummonMaxInstances
        {
            private const string Prefix = "Tameable.UnsummonMaxInstances";

            [HarmonyPrefix]
            public static bool Debug(Tameable __instance, ref int maxInstances)
            {
                HarmonyLog.Log($"[{Prefix}.Prefix] Unit: {__instance.name}; Max: {maxInstances}.");
                maxInstances = GlobalState.Config.GodModeOptions.SummonsLimit;

                return true;
            }
        }
    }
}
