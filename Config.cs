using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersaValheimHacks.Options;

namespace VersaValheimHacks
{
    internal class Config
    {
        public static readonly string DefaultConfigPath = $"{nameof(VersaValheimHacks)}.json";

        /// <summary>
        /// Path associated with the current config instance.
        /// </summary>
        [JsonIgnore]
        public string PathToConfig { get; private set; }

        #region Config Fields

        /// <summary>
        /// Are hacks enabled at all or not.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Is Debug mode enabled or not.
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Hacks keep working but use vanilla-looking values (no extended buffs,
        /// vanilla guardian cooldowns) and all mod messages are hidden, so
        /// streams don't reveal them.
        /// </summary>
        public bool StreamerMode { get; set; } = false;

        /// <summary>
        /// Auto-skip the first-spawn intro (video cinematic, Valkyrie flight,
        /// intro text) when entering a world.
        /// </summary>
        public bool SkipIntroCinematic { get; set; } = true;

        /// <summary>
        /// Is Harmony logging mode enabled or not.
        /// </summary>
        public bool Logging { get; set; } = false;

        public HotkeysOptions HotkeysOptions { get; set; } = new HotkeysOptions();
        public BetterEatingOptions BetterEatingOptions { get; set; } = new BetterEatingOptions();
        public BetterPowersOptions BetterPowersOptions { get; set; } = new BetterPowersOptions();
        public GodModeOptions GodModeOptions { get; set; } = new GodModeOptions();
        public SkillsOptions SkillsOptions { get; set; } = new SkillsOptions();
        public StaminaOptions StaminaOptions { get; set; } = new StaminaOptions();
        public BuffsOptions BuffsOptions { get; set; } = new BuffsOptions();
        public NotificationOptions NotificationOptions { get; set; } = new NotificationOptions();
        public PiecesOptions PiecesOptions { get; set; } = new PiecesOptions();
        public PickableOptions PickableOptions { get; set; } = new PickableOptions();
        public RecipeOptions RecipeOptions { get; set; } = new RecipeOptions();
        #endregion

        [JsonConstructor]
        private Config()
        {
            PathToConfig = DefaultConfigPath;
        }

        private Config(string pathToConfig)
        {
            if (string.IsNullOrEmpty(pathToConfig))
                throw new ArgumentNullException(nameof(pathToConfig));

            PathToConfig = pathToConfig;
        }

        ~Config()
        {
        }

        public static Config LoadOrCreateDefault(string pathToConfig)
        {
            if (string.IsNullOrEmpty(pathToConfig))
                throw new ArgumentNullException(nameof(pathToConfig));

            Config config;
            if (!File.Exists(pathToConfig) && pathToConfig == DefaultConfigPath)
            {
                HarmonyLog.Log($"No config file found: \"{pathToConfig}\"!");
                config = new Config(DefaultConfigPath);
            }
            else
            {
                string json = File.ReadAllText(pathToConfig);

                config = JsonConvert.DeserializeObject<Config>(json) ?? throw new InvalidOperationException();
                config.PathToConfig = pathToConfig;
            }

            // TODO: initialize default values here.

            config.Save(); // Re-save to add new or missing config fields.

            return config;
        }

        public Config Save()
        {
            string json = ToJson();
            File.WriteAllText(PathToConfig, json);

            return this;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { Formatting = Formatting.Indented, });
        }
    }
}
