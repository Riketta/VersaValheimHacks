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
    internal class UnlockRecipes
    {
        //[HarmonyPatch(typeof(Player), "GetAvailableRecipes")]
        internal class GetAvailableRecipes
        {
            private const string Prefix = "Player.GetAvailableRecipes";

            [HarmonyPostfix]
            public static void GetAvailableRecipesIncludingLocked(ref List<Recipe> available)
            {
                if (!GlobalState.ToggleExtraHacks)
                    return;
                else
                    HarmonyLog.Log($"[{Prefix}.Postfix] Not modifying access to recipes...");

                HarmonyLog.Log($"[{Prefix}.Postfix] Trying to unlock all recipes...");
                available.Clear();
                foreach (Recipe recipe in ObjectDB.instance.m_recipes)
                {
                    HarmonyLog.Log($"[{Prefix}.Postfix] Unlocking (soft) recipe: {recipe.name}.");
                    available.Add(recipe);
                }
            }
        }

        //[HarmonyPatch(typeof(Player), "IsRecipeKnown")]
        internal class IsRecipeKnown
        {
            private const string Prefix = "Player.IsRecipeKnown";

            [HarmonyPostfix]
            public static void SetAllRecipesAsKnown(ref bool __result, string name)
            {
                if (!GlobalState.ToggleExtraHacks)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] Set recipe as known: {name}.");
                __result = true;
            }
        }

        //[HarmonyPatch(typeof(Player), "HaveRequirements", new Type[] { typeof(Recipe), typeof(bool), typeof(int) })]
        internal class HaveRequirements
        {
            private const string Prefix = "Player.HaveRequirements";

            [HarmonyPostfix]
            public static void NoRequirements(ref bool __result, Recipe recipe, bool discover, int qualityLevel)
            {
                if (!GlobalState.ToggleExtraHacks)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] No requirements for recipe: {recipe.name} ({discover}, {qualityLevel}).");
                __result = true;
            }
        }

        [HarmonyPatch(typeof(Player), "UpdateKnownRecipesList")]
        internal class UpdateKnownRecipesList
        {
            private const string Prefix = "Player.UpdateKnownRecipesList";
            private static bool AlreadyApplied = false;

            //public static FieldInfo pawnField = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");
            //public static MethodInfo PrimaryPropertySet = AccessTools.PropertySetter(typeof(Pawn_EquipmentTracker), "Primary");
            public static MethodInfo AddKnownRecipeMethod = AccessTools.Method(typeof(Player), "AddKnownRecipe"); // (global::Recipe recipe)

            [HarmonyPrefix]
            public static bool UnlockRecipies(Player __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Prefix] DEBUG: {AlreadyApplied}.");
                if (!GlobalState.ToggleExtraHacks || __instance is null || AlreadyApplied)
                    return true;

                Player player = __instance;
                HarmonyLog.Log($"[{Prefix}.Prefix] Unlocking all enabled recipes.");

                var AddKnownRecipe = AccessTools.MethodDelegate<Action<Recipe>>(AddKnownRecipeMethod, player);
                foreach (Recipe recipe in ObjectDB.instance.m_recipes)
                {
                    HarmonyLog.Log($"[{Prefix}.Prefix] Trying to unlock recipe: {recipe.name} ({recipe.m_enabled}).");

                    if (recipe.m_enabled)
                    {
                        //AddKnownRecipeMethod.Invoke(player, new object[] { recipe });
                        AddKnownRecipe(recipe);
                    }
                }

                AlreadyApplied = true;
                return false;
            }
        }
    }
}
