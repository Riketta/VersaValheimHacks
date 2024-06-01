using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class PiecesOptions
    {
        public Dictionary<string, string> PlantPieces { get; set; } = new Dictionary<string, string>()
        {
            //["$piece_cultivate"] = "cultivate_v2",
            //["$piece_replant"] = "replant_v2",
            ["$piece_sapling_turnip"] = "sapling_turnip",
            ["$piece_sapling_seedturnip"] = "sapling_seedturnip",
            ["$piece_sapling_onion"] = "sapling_onion",
            ["$piece_sapling_seedonion"] = "sapling_seedonion",
            ["$piece_sapling_carrot"] = "sapling_carrot",
            ["$piece_sapling_seedcarrot"] = "sapling_seedcarrot",
            ["$piece_sapling_barley"] = "sapling_barley",
            ["$piece_sapling_flax"] = "sapling_flax",
            ["$item_jotunpuffs"] = "sapling_jotunpuffs",
            ["$item_magecap"] = "sapling_magecap",
            ["$prop_fir_sapling"] = "FirTree_Sapling",
            ["$prop_pine_sapling"] = "PineTree_Sapling",
            ["$prop_beech_sapling"] = "Beech_Sapling",
            ["$prop_birch_sapling"] = "Birch_Sapling",
            ["$prop_oak_sapling"] = "Oak_Sapling",
            ["$piece_sapling_vineash"] = "VineAsh_sapling",
        };
    }
}
