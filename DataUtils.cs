using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace FileTheftVariationPlugin
{
    internal class DataUtils
    {
        public static List<string> flavorList;

        internal static string dataFilePath = Path.Combine(Paths.ConfigPath, "FileTheftVariationPatch.json");

        public static void LoadDatabase()
        {
            if (!File.Exists(dataFilePath))
            {
                Plugin.Logger.LogWarning("No flavor list found");
                flavorList = new List<string>()
                {
                    "confidential",
                    "engine_specs",
                    "build_dep",
                    "sales_report",
                    "secret_recipe",
                    "key_log",
                    "browser_history",
                    "launch_code",
                    "weapon_specs",
                    "election_results",
                    "medical_data",
                    "insurance_report",
                    "backup_key",
                    "homework"
                };
                string save = JsonConvert.SerializeObject(flavorList, Formatting.Indented);
                File.WriteAllText(dataFilePath, save);
                Plugin.Logger.LogInfo("Flavor list generated");
                return;
            }
            string load = File.ReadAllText(dataFilePath);
            flavorList = JsonConvert.DeserializeObject<List<string>>(load);
            Plugin.Logger.LogInfo("Flavor list found");
        }
    }
}