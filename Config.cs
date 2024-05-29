using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class Config
    {
        public static readonly string DefaultConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{nameof(VersaValheimHacks)}.json");

        /// <summary>
        /// Path associated with the current config instance.
        /// </summary>
        [JsonIgnore]
        public string PathToConfig { get; private set; }

        #region Config Fields
        /// <summary>
        /// Is Debug mode enabled or not.
        /// </summary>
        public bool Debug { get; set; } = false;
        #endregion

        [JsonConstructor]
        public Config()
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

        public static Config SaveDefault()
        {
            Config config = new Config(DefaultConfigPath);
            config.Save();

            return config;
        }

        public Config Save()
        {
            string json = JsonConvert.SerializeObject(this, new JsonSerializerSettings() { Formatting = Formatting.Indented, });
            File.WriteAllText(PathToConfig, json);

            return this;
        }
    }
}
