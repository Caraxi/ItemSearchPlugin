using Dalamud.Data.TransientSheet;
using System;

namespace ItemSearchPlugin {
    public interface ISearchFilter : IDisposable {
        /// <summary>
        /// The name of the filter. Used for english display or when no translation is available
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// CheapLoc localization key
        /// </summary>
        public string NameLocalizationKey { get; }

        /// <summary>
        /// Whether or not the filter should be displayed.
        /// </summary>
        public bool ShowFilter { get; }


        /// <summary>
        ///	True if the filter should be used when building item list
        /// </summary>
        public bool IsSet { get; }

        /// <summary>
        /// True if the filter has changed since the last time HasChanged was called.
        /// Should only be true once, then false until the next change.
        /// </summary>
        public bool HasChanged { get; }

        /// <summary>
        /// Checks if an item passes against the filter.
        /// </summary>
        /// <param name="item">The item being checked</param>
        /// <returns>true if the item should be displayed</returns>
        public bool CheckFilter(Item item);

        /// <summary>
        /// Draw the ImGui widgets for the filter.
        /// </summary>
        public void DrawEditor();
    }
}
