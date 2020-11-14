using System.Diagnostics.CodeAnalysis;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    internal abstract class SearchFilter {
        // Temp Variables
        internal string _LocalizedName = "";
        internal float _LocalizedNameWidth = 0;

        protected bool Modified;

        public virtual void Dispose() { }

        /// <summary>
        /// The name of the filter. Used for english display or when no translation is available
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Localization key
        /// </summary>
        public abstract string NameLocalizationKey { get; }

        /// <summary>
        /// Whether or not the filter should be displayed.
        /// </summary>
        public virtual bool ShowFilter => true;

        /// <summary>
        ///	True if the filter should be used when building item list
        /// </summary>
        public abstract bool IsSet { get; }

        /// <summary>
        /// True if the filter has changed since the last time HasChanged was called.
        /// Should only be true once, then false until the next change.
        /// </summary>
        public virtual bool HasChanged {
            get {
                if (!Modified) return false;
                Modified = false;
                return true;
            }
        }

        /// <summary>
        /// Checks if an item passes against the filter.
        /// </summary>
        /// <param name="item">The item being checked</param>
        /// <returns>true if the item should be displayed</returns>
        public abstract bool CheckFilter(Item item);

        /// <summary>
        /// Draw the ImGui widgets for the filter.
        /// </summary>
        public abstract void DrawEditor();

        /// <summary>
        /// True if the filter is not disabled.
        /// </summary>
        public bool IsEnabled => !this.CanBeDisabled || !this.PluginConfig.DisabledFilters.Contains(this.NameLocalizationKey);

        /// <summary>
        /// True if the filter should be shown in the "Enabled Filters" section of config.
        /// </summary>
        public virtual bool CanBeDisabled => true;

        protected ItemSearchPluginConfig PluginConfig;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected SearchFilter(ItemSearchPluginConfig config) {
            this.PluginConfig = config;
        }

        internal void ConfigSetup() {
            (string l, string e) a = (NameLocalizationKey, Name);
            if (CanBeDisabled && !PluginConfig.FilterNames.Contains(a)) {
                PluginConfig.FilterNames.Add(a);
            }
        }
    }
}
