using System.Diagnostics.CodeAnalysis;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    abstract class SearchFilter : ISearchFilter {
        public virtual void Dispose() { }
        public abstract string Name { get; }
        public abstract string NameLocalizationKey { get; }
        public virtual bool ShowFilter => true;
        public abstract bool IsSet { get; }
        public abstract bool HasChanged { get; }
        public abstract bool CheckFilter(Item item);
        public abstract void DrawEditor();

        public bool IsEnabled => !this.CanBeDisabled || !this.PluginConfig.DisabledFilters.Contains(this.NameLocalizationKey);

        public virtual bool CanBeDisabled => true;

        protected ItemSearchPluginConfig PluginConfig;

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
        protected SearchFilter(ItemSearchPluginConfig config) {
            this.PluginConfig = config;
            (string l, string e) a = (NameLocalizationKey, Name);
            
            if (CanBeDisabled && !PluginConfig.FilterNames.Contains(a)) {
                config.FilterNames.Add(a);
            }
        }
    }
}
