using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Logging;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace ItemSearchPlugin {
    internal class Loc {
        internal static readonly string[] ApplicableLangCodes = {"de", "ja", "fr"};

        private static Dictionary<string, string> localizationStrings = new Dictionary<string, string>();

        internal static void LoadLanguage(string langCode) {
            if (langCode.ToLower() == "en") {
                localizationStrings = new Dictionary<string, string>();
                return;
            }

            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ItemSearchPlugin.Localization.{langCode}.json");

            if (s == null) {
                PluginLog.LogError("Failed to find language file.");
                localizationStrings = new Dictionary<string, string>();
                return;
            }

            using var sr = new StreamReader(s);
            localizationStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
        }

        internal static string Localize(string key, string fallbackValue) {
            try {
                return localizationStrings[key];
            } catch {
                localizationStrings[key] = fallbackValue;
                return fallbackValue;
            }
        }


        internal static void LoadDefaultLanguage() {
            try {
                var currentUiLang = CultureInfo.CurrentUICulture;
                #if DEBUG
                PluginLog.Log("Trying to set up Loc for culture {0}", currentUiLang.TwoLetterISOLanguageName);
                #endif
                LoadLanguage(ApplicableLangCodes.Any(x => currentUiLang.TwoLetterISOLanguageName == x) ? currentUiLang.TwoLetterISOLanguageName : "en");
            } catch (Exception ex) {
                PluginLog.LogError("Could not get language information. Setting up fallbacks. {0}", ex.ToString());
                LoadLanguage("en");
            }
        }

        internal static void ExportLoadedDictionary() {
            string json = JsonConvert.SerializeObject(localizationStrings, Formatting.Indented);
            PluginLog.Log(json);
        }
    }
}
