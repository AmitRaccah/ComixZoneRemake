using UnityEngine;

#region Core / Inventory 🗂️
public struct PlayerPickUpEvent { }                
public struct ItemPickedUpEvent { public PickupType pickupType; }
public struct ItemUsedEvent { public PickupType pickupType; }
public struct InventoryChangedEvent { }              
#endregion
