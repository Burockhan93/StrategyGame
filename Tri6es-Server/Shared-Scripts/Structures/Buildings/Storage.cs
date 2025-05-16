using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    class Storage : InventoryBuilding
    {
        public override string description => "The Storage is used to store Ressources. Higher Levels will improve the Capacity of the Storage. The Storage can also be used as a distributor for the Ressources.";
        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => BuildingInventory.AllRessourceLimit(new int[] { 40, 80, 120 });

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8 }, { RessourceType.STONE, 3 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10 }, { RessourceType.STONE, 6 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 12 }, { RessourceType.STONE, 5 }, { RessourceType.IRON, 3 } }
                };
                return result;
            }
        }

        public Storage() : base()
        {
            Inventory.UpdateIncoming(BuildingInventory.GetListOfAllRessources());
            Inventory.UpdateOutgoing(BuildingInventory.GetListOfAllRessources());
        }

        public Storage(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory
        ) : base(Cell, Tribe, Level, Inventory) {}


        public override void DoTick()
        {
            base.DoTick();
        }
    }
}
