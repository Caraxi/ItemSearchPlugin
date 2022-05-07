using System.ComponentModel;

namespace ItemSearchPlugin; 

public enum SortType {
    [Description("Item ID [ASC]")]
    ItemID,
    
    [Description("Item ID [DESC]")]
    ItemIDDesc,
    
    [Description("Item Name [ASC]")]
    Name,
    
    [Description("Item Name [DESC]")]
    NameDesc,
    
    [Description("Item Level [ASC]")]
    ItemLevel,
    
    [Description("Item Level [DESC]")]
    ItemLevelDesc,
    
    [Description("Item Equip Level [ASC]")]
    EquipLevel,
    
    [Description("Item Equip Level [DESC]")]
    EquipLevelDesc,
}
