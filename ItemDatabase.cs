using System;
using System.Collections.Generic;

namespace VersaValheimHacks
{
    /// <summary>
    /// Static item database for Valheim, independent of any feature.
    /// Built by hand from an in-game ObjectDB dump (debug bind Numpad6
    /// writes VersaValheimHacks.ItemDump.txt to the game root); refresh
    /// it the same way whenever the game adds new items.
    ///
    /// Lookup is case-insensitive; unknown prefab names simply return
    /// false and the caller decides its own fallback, so this class
    /// carries no feature-specific behavior.
    /// </summary>
    internal static class ItemDatabase
    {
        /// <summary>Region indices in progression order (palette slot order).</summary>
        public static class Regions
        {
            public const int Meadows = 0;
            public const int BlackForest = 1;
            public const int Swamp = 2;
            public const int Mountain = 3;
            public const int Plains = 4;
            public const int Mistlands = 5;
            public const int Ashlands = 6;
            public const int DeepNorth = 7;

            public const int Count = 8;
        }

        /// <summary>Display names per region index.</summary>
        public static readonly string[] RegionNames =
        {
            "Meadows", "Black Forest", "Swamp", "Mountain", "Plains",
            "Mistlands", "Ashlands", "Deep North",
        };

        /// <summary>
        /// Item prefab name -> home region index. Raw and biome-locked
        /// items, trophies (mob home region), and a handful of processed
        /// goods assigned explicitly. Biome-neutral upgrade idols stay
        /// unmapped so they can never skew the tier color; consumers pick
        /// the highest region among a recipe's ingredients. Ocean items
        /// have no dedicated palette slot and are colored as Swamp by
        /// choice.
        /// </summary>
        private static readonly Dictionary<string, int> ItemRegions =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // Meadows
            ["Wood"] = 0, ["Stone"] = 0, ["Flint"] = 0, ["LeatherScraps"] = 0, ["DeerHide"] = 0,
            ["Feathers"] = 0, ["Honey"] = 0, ["Raspberry"] = 0, ["Blueberries"] = 0, ["Mushroom"] = 0,
            ["Dandelion"] = 0, ["Resin"] = 0, ["FineWood"] = 0, ["NeckTail"] = 0, ["RawMeat"] = 0,
            ["Meat"] = 0, ["BoarMeat"] = 0, ["DeerMeat"] = 0, ["QueenBee"] = 0,
            ["Carrot"] = 0, ["ChickenEgg"] = 0, ["ChickenMeat"] = 0, ["HardAntler"] = 0,
            ["BjornHide"] = 0, ["BjornPaw"] = 0,
            ["QueensJam"] = 0, ["DeerStew"] = 0, ["CookedMeat"] = 0, ["CookedDeerMeat"] = 0,
            ["CookedBjornMeat"] = 0, ["Leatherstraps"] = 0, ["AxeHead1"] = 0, ["AxeHead2"] = 0,
            // Black Forest
            ["CopperOre"] = 1, ["TinOre"] = 1, ["Copper"] = 1, ["Tin"] = 1, ["Bronze"] = 1,
            ["CoreWood"] = 1, ["RoundLog"] = 1, ["SurtlingCore"] = 1, ["TrollHide"] = 1,
            ["GreydwarfEye"] = 1, ["AncientSeed"] = 1, ["Thistle"] = 1,
            ["Coal"] = 1, ["BoneFragments"] = 1, ["Ectoplasm"] = 1,
            // Swamp
            ["IronScrap"] = 2, ["Iron"] = 2, ["ElderBark"] = 2, ["AncientBark"] = 2, ["Guck"] = 2,
            ["Bloodbag"] = 2, ["Ooze"] = 2, ["Chain"] = 2, ["WitheredBone"] = 2, ["Root"] = 2,
            ["Entrails"] = 2, ["Turnip"] = 2, ["Sausages"] = 2, ["TurnipStew"] = 2,
            ["WrithanRoots"] = 2, ["BlobVial"] = 2, ["MushroomBzerker"] = 2,
            ["CuredSquirrelHamstring"] = 2, ["PungentPebbles"] = 2, ["FragrantBundle"] = 2,
            ["ScytheHandle"] = 2,
            // Ocean items (no dedicated palette slot; colored as Swamp by choice)
            ["Fish1"] = 2, ["Fish2"] = 2, ["Fish3"] = 2, ["Fish4_cave"] = 2, ["Fish5"] = 2,
            ["Fish6"] = 2, ["Fish7"] = 2, ["Fish8"] = 2, ["Fish9"] = 2, ["Fish10"] = 2,
            ["Fish11"] = 2, ["Fish12"] = 2, ["FishRaw"] = 2, ["FishCooked"] = 2,
            ["FishingBait"] = 2, ["Chitin"] = 2, ["FreshSeaweed"] = 2, ["SerpentScale"] = 2,
            ["SerpentMeatCooked"] = 2, ["TrophySerpent"] = 2, ["SpiceOceans"] = 2,
            // Mountain
            ["SilverOre"] = 3, ["Silver"] = 3, ["WolfFang"] = 3, ["WolfPelt"] = 3, ["WolfClaw"] = 3,
            ["WolfHairBundle"] = 3, ["Obsidian"] = 3, ["FreezeGland"] = 3, ["Crystal"] = 3,
            ["WolfMeat"] = 3, ["Onion"] = 3, ["OnionSoup"] = 3, ["WolfMeatSkewer"] = 3,
            ["PowderedDragonEgg"] = 3,
            // Plains
            ["BlackMetalScrap"] = 4, ["BlackMetal"] = 4, ["Flax"] = 4, ["Barley"] = 4,
            ["BarleyFlour"] = 4, ["Bread"] = 4, ["BreadDough"] = 4, ["LinenThread"] = 4,
            ["LoxPelt"] = 4, ["Needle"] = 4, ["Tar"] = 4, ["LoxMeat"] = 4, ["Cloudberry"] = 4,
            ["LoxPie"] = 4, ["TrophyBjornUndead"] = 4, ["UndeadBjornRibcage"] = 4,
            // Mistlands
            ["BlackMarble"] = 5, ["Sap"] = 5, ["Carapace"] = 5, ["YggdrasilWood"] = 5, ["Eitr"] = 5,
            ["ScaleHide"] = 5, ["Mandible"] = 5, ["HareMeat"] = 5, ["RoyalJelly"] = 5, ["Wisp"] = 5,
            ["Bilebag"] = 5, ["GiantBloodSack"] = 5, ["BugMeat"] = 5, ["DvergrKeyFragment"] = 5,
            ["MushroomJotunPuffs"] = 5, ["MushroomMagecap"] = 5,
            ["MisthareSupreme"] = 5, ["YggdrasilPorridge"] = 5, ["CookedBugMeat"] = 5,
            ["CeramicPlate"] = 5,
            // Ashlands
            ["FlametalOre"] = 6, ["Flametal"] = 6, ["FlametalNew"] = 6, ["CharredBone"] = 6,
            ["CharredBlood"] = 6, ["Ashwood"] = 6, ["Blackwood"] = 6, ["ProustitePowder"] = 6,
            ["AsksvinMeat"] = 6, ["AskHide"] = 6, ["AskBladder"] = 6, ["CookedAsksvinMeat"] = 6,
            ["Vineberry"] = 6, ["MushroomSmokePuff"] = 6, ["CelestialFeather"] = 6,
            ["Fiddleheadfern"] = 6,
            ["BonemawSerpentTooth"] = 6, ["VoltureEgg"] = 6, ["VoltureMeat"] = 6,
            ["MorgenHeart"] = 6, ["MorgenSinew"] = 6, ["Grausten"] = 6, ["SulfurStone"] = 6,
            ["MoltenCore"] = 6, ["FaderEmber"] = 6, ["ScorchingMedley"] = 6,
            ["GemstoneBlue"] = 6, ["GemstoneGreen"] = 6, ["GemstoneRed"] = 6,
            ["DyrnwynBladeFragment"] = 6, ["DyrnwynHiltFragment"] = 6, ["DyrnwynTipFragment"] = 6,
            ["BellFragment"] = 6,
            // Deep North
            ["Gold"] = 7, ["Frostwood"] = 7, ["BarkaBranch"] = 7, ["Kale"] = 7, ["Lingonberry"] = 7,
            ["Oat"] = 7, ["OatFlour"] = 7, ["OatMilk"] = 7, ["Poteitr"] = 7, ["SealBlubber"] = 7,
            ["SealHide"] = 7, ["MooseHide"] = 7, ["MooseMeat"] = 7, ["MooseSinew"] = 7,
            ["ElakingHairBundle"] = 7, ["NornThread"] = 7, ["MoleClaws"] = 7,
            ["CookedMooseMeat"] = 7,
            ["CrownJewel"] = 7, ["Ice"] = 7, ["Voidplasm"] = 7, ["OozeMork"] = 7,
            ["TrophyBlob_Morkhalla"] = 7,
            // Trophies (mob home region; trophies are crafting ingredients)
            ["TrophyDeer"] = 0, ["ElderTrophy"] = 1, ["TrophyGreydwarfShaman"] = 1,
            ["TrophySkeleton"] = 1, ["TrophyFrostTroll"] = 1,
            ["TrophyAbomination"] = 2, ["TrophyBlob"] = 2, ["TrophyDraugrElite"] = 2,
            ["TrophyLeech"] = 2, ["TrophySurtling"] = 2,
            ["TrophyFenring"] = 3, ["TrophyHatchling"] = 3, ["TrophySGolem"] = 3, ["TrophyWolf"] = 3,
            ["TrophyGoblin"] = 4, ["TrophyGoblinBrute"] = 4, ["TrophyGrowth"] = 4, ["TrophyLox"] = 4,
            ["TrophyGjall"] = 5, ["TrophySeeker"] = 5,
            ["TrophyCharredMelee"] = 6, ["TrophyFallenValkyrie"] = 6, ["TrophyMorgen"] = 6,
            ["TrophyJotunWitch"] = 7, ["TrophyMoose"] = 7,
            // Region spices (region encoded in the name)
            ["SpicePlains"] = 4, ["SpiceForests"] = 1, ["SpiceMistlands"] = 5,
            ["SpiceMountains"] = 3, ["SpiceAshlands"] = 6, ["SpiceDeepNorth"] = 7,
        };

        /// <summary>Number of mapped items.</summary>
        public static int Count => ItemRegions.Count;

        /// <summary>
        /// Region of an item prefab, e.g. "Frostwood" -> Deep North.
        /// Case-insensitive; returns false for items not in the database.
        /// </summary>
        public static bool TryGetRegion(string prefabName, out int region)
        {
            return ItemRegions.TryGetValue(prefabName, out region);
        }

        /// <summary>Whether the item prefab has a region assigned.</summary>
        public static bool Contains(string prefabName)
        {
            return ItemRegions.ContainsKey(prefabName);
        }

        /// <summary>Display name of a region index, or "Unknown" when out of range.</summary>
        public static string GetRegionName(int region)
        {
            return region >= 0 && region < RegionNames.Length ? RegionNames[region] : "Unknown";
        }
    }
}
