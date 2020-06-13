using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected ItemSearchPluginConfig PluginConfig;

        protected SearchFilter(ItemSearchPluginConfig config) {
            this.PluginConfig = config;
        }
    }
}
