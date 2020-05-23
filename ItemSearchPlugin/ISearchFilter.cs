using Dalamud.Data.TransientSheet;
using System;

namespace ItemSearchPlugin {
	public interface ISearchFilter : IDisposable {
		public string Name { get; }		
		public string NameLocalizationKey { get; }
		public bool IsSet { get; }
		public bool HasChanged { get; }


		public bool CheckFilter(Item item);
		public void DrawEditor();

	}
}
