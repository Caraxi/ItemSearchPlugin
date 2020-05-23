using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CheapLoc;
using Serilog;

namespace ItemSearchPlugin
{ 

    /**
     * 
     * https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Localization.cs
     * 
     */

    class Localization {

        public static readonly string[] ApplicableLangCodes = { "de", "ja", "fr", "it", "es" };


        public void SetupWithUiCulture() {
            try
            {
                var currentUiLang = CultureInfo.CurrentUICulture;
                Log.Information("Trying to set up Loc for culture {0}", currentUiLang.TwoLetterISOLanguageName);

                if (ApplicableLangCodes.Any(x => currentUiLang.TwoLetterISOLanguageName == x)) {
                    SetupWithLangCode(currentUiLang.TwoLetterISOLanguageName);
                } else {
                    Loc.SetupWithFallbacks();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not get language information. Setting up fallbacks.");
                Loc.SetupWithFallbacks();
            }
        }

        public void SetupWithLangCode(string langCode) {
            if (langCode.ToLower() == "en") {
                Loc.SetupWithFallbacks();
                return;
            }

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"itemsearch_{langCode}")) {
                using (StreamReader sr = new StreamReader(s)){
                    Loc.Setup(sr.ReadToEnd());
                }
            }

        }
    }
}
