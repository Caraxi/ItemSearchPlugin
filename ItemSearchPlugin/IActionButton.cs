using System;
using Item = Dalamud.Data.TransientSheet.Item;

namespace ItemSearchPlugin {
    public enum ActionButtonPosition {
        TOP,
        BOTTOM
    }

    public interface IActionButton : IDisposable {
        public string GetButtonText(Item selectedItem);

        public void OnButtonClicked(Item selectedItem);

        public bool GetShowButton(Item selectedItem);

        public ActionButtonPosition ButtonPosition { get; }
    }
}
