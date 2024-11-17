using System;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin {
    public enum ActionButtonPosition {
        TOP,
        BOTTOM
    }

    public abstract class IActionButton : IDisposable {
        public abstract string GetButtonText(Item selectedItem);

        public abstract void OnButtonClicked(Item selectedItem);

        public abstract bool GetShowButton(Item selectedItem);

        public virtual string GetButtonText(EventItem selectedItem) => "";

        public virtual void OnButtonClicked(EventItem selectedItem) { }

        public virtual bool GetShowButton(EventItem selectedItem) => false;


        public string GetButtonText(GenericItem selectedItem) {
            return selectedItem.GenericItemType switch {
                GenericItem.ItemType.Item => GetButtonText((Item) selectedItem),
                GenericItem.ItemType.EventItem => GetButtonText((EventItem) selectedItem),
                _ => ""
            };
        }

        public void OnButtonClicked(GenericItem selectedItem) {
            switch (selectedItem.GenericItemType) {
                case GenericItem.ItemType.Item:
                    OnButtonClicked((Item) selectedItem);
                    break;
                case GenericItem.ItemType.EventItem:
                    OnButtonClicked((EventItem) selectedItem);
                    break;
                default:
                    break;
            }
        }

        public bool GetShowButton(GenericItem selectedItem) {
            return selectedItem.GenericItemType switch {
                GenericItem.ItemType.Item => GetShowButton((Item) selectedItem),
                GenericItem.ItemType.EventItem => GetShowButton((EventItem) selectedItem),
                _ => false
            };
        }

        public abstract ActionButtonPosition ButtonPosition { get; }
        
        public abstract void Dispose();
    }
}
