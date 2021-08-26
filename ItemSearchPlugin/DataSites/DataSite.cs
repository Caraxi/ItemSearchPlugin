using System.Diagnostics;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    public abstract class DataSite {
        public abstract string GetItemUrl(Item item);

        public abstract string Name { get; }

        public abstract string NameTranslationKey { get; }

        public virtual string Note { get; } = null;

        public virtual void OpenItem(Item item) {
            System.Diagnostics.Process.Start(new ProcessStartInfo() {
                UseShellExecute = true,
                FileName = GetItemUrl(item)
            });
        }
    }
}
