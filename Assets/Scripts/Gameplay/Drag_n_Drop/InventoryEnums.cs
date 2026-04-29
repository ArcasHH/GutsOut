// InventoryEnums.cs
public enum ItemType 
{ 
    None,
    Brain,
    Heart,
    Guts,
    Lungs,
    HumanDeleter,
    OrganDeleter
}

public enum DropResult 
{ 
    Success, 
    Swap, 
    TypeMismatch, 
    Occupied 
}