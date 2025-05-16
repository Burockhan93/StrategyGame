using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    public class Headquarter : ProtectedBuilding
    {
        public override string description => "The Headquarter can be placed to found a new Tribe. Different levels of the Headquarter grant access to other Buildings. The Headquarter also includes an Inventory for the Tribe which can be accessed from anywhere. Ressources in the Inventory can be used to build other Buildings or be refined into better Ressources or Troops.";

        public override byte MaxLevel => 3;

        public override byte Radius => 5;

        public override byte[] MaxHealths => new byte[]{
            25,
            30,
            35
        };

        public override Dictionary<RessourceType, int[]> InventoryLimits => BuildingInventory.AllRessourceLimit(new [] { 40, 80, 120 });

        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 20 }, { RessourceType.STONE, 10 } },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 25 }, { RessourceType.STONE, 20 }, { RessourceType.IRON, 10 } }
                };
                return result;
            }
        }

        public Headquarter() : base()
        {
            Inventory.UpdateIncoming(BuildingInventory.GetListOfAllRessources());
            Inventory.UpdateOutgoing(BuildingInventory.GetListOfAllRessources());

            // initial ressources for testing in debug
            #if DEBUG
                foreach (RessourceType ressource in Enum.GetValues(typeof(RessourceType)))
                {
                if(ressource == RessourceType.WOOD) 
                    Inventory.AddRessource(ressource, 20); //will be 200
                }
            #endif
        }

        public Headquarter(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            byte health,
            TroopInventory troops
        ) : base(cell, tribe, level, inventory, health, troops) {}


        public override void DoTick()
        {
            base.DoTick();
        }

        /// <summary>Checks whether this HQ is protected by another building.</summary>
        public bool isProtected()
        {
            return Cell.GetAllProtectors().Any(protector => protector != this && protector.Tribe == Tribe);
        }
    }
}
