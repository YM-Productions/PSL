using SpacetimeDB;

namespace StdbModule.Modules;

public static partial class Module
{
    #region Tables

    [Table(Name = nameof(Inventory), Public = true)]
    public partial class Inventory
    {
        [PrimaryKey]
        public Guid identity;
        public Identity OwnerIdentity;

        public float MaxVolume;
        public int SlotCount;
    }

    [Table(Name = nameof(InventorySlot), Public = true)]
    public partial class InventorySlot
    {
        [PrimaryKey]
        public Guid identity;
        public Guid InventoryIdentity;

        public int Index;
        // Item
    }

    #endregion

    #region Reducers

    public partial class InventoryReducers
    {
        [Reducer]
        public static void CreateInventory(ReducerContext ctx)
        {

        }
    }

    #endregion
}
