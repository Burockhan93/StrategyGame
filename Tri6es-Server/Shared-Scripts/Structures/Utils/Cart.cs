
namespace Shared.Structures
{
    public class Cart
    {
        public Inventory Inventory;
        public InventoryBuilding Origin;
        public InventoryBuilding Destination;

        public bool HasMoved;

        public int Capacity = 2;

        public Cart()
        {
            this.Inventory = new Inventory();
        }

        public Cart(InventoryBuilding origin)
        {
            this.Inventory = new Inventory(Inventory.GetDictionaryOfAllRessources());
            this.Origin = origin;
            this.Destination = origin;
        }

        public Cart(Inventory Inventory, InventoryBuilding Origin, InventoryBuilding Destination)
        {
            this.Inventory = Inventory;
            this.Origin = Origin;
            this.Destination = Destination;
        }

        public void DoTick()
        {
            HasMoved = false;
        }

        public void Clear()
        {
            this.Inventory.Clear();
        }

        public int AvailableSpace()
        {
            return Capacity - Inventory.TotalCount();
        }
    }
}
