// InventoryEnums.cs
public enum ItemType 
{ 
    None,       // Универсальный слот (принимает всё)
    Brain,     // Мозг
    Heart,      // Сердце
    Intestines, // Кишечник
    Lungs    // Легкие
}

public enum DropResult 
{ 
    Success, 
    Swap, 
    TypeMismatch, 
    Occupied 
}