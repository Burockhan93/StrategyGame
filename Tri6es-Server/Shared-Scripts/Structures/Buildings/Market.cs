using Shared.DataTypes;
using System.Collections.Generic;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    public class Market : RefineryBuilding
    {

        public override string description => "The Market can be used to exchange any Ressource into any other Ressource. Higher levels of the market offer will offer a better trade ratio between those Ressources.";

        public override byte MaxLevel => 3;

        // FIXME: error if input & output are same, disallow or handle?
        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { TradeInput, new [] { 11, 9, 7 } },
            { TradeOutput, new [] { 8, 12, 16 } },
        };

        public override int[] MaxProgresses => new int[] {
            Constants.MinutesToGameTicks(900),
            Constants.MinutesToGameTicks(500),
            Constants.MinutesToGameTicks(200)
        };

        public RessourceType TradeInput = RessourceType.WOOD;

        public RessourceType TradeOutput = RessourceType.IRON;

        public override Dictionary<RessourceType, int> InputRecipe => new Dictionary<RessourceType, int> { { TradeInput, CurrentLimit[TradeInput] - 1 } };
        public override Dictionary<RessourceType, int> OutputRecipe => new Dictionary<RessourceType, int> { { TradeOutput, 1 } };


        public override Dictionary<RessourceType, int>[] Recipes
        {
            get
            {
                Dictionary<RessourceType, int>[] result = {
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 6 }, { RessourceType.STONE, 2 }  },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 8 }, { RessourceType.STONE, 4 }, { RessourceType.IRON, 2 }  },
                    new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 10 }, { RessourceType.STONE, 6 }, { RessourceType.IRON, 4 } }
                };
                return result;
            }
        }

        public Market() : base() {}

        public Market(
            HexCell Cell,
            byte Tribe,
            byte Level,
            BuildingInventory Inventory,
            int Progress,
            RessourceType TradeInput,
            RessourceType TradeOutput
        ) : base(Cell, Tribe, Level, Inventory, Progress)
        {
            this.TradeInput = TradeInput;
            this.TradeOutput = TradeOutput;
        }

        public override bool IsPlaceableAt(HexCell cell)
        {
            return base.IsPlaceableAt(cell) ;
        }

        public void ChangeInputRecipe(RessourceType inputRessource)
        {
            Progress = 0;
            TradeInput = inputRessource;

            Inventory.UpdateRessourceLimits(CurrentLimit);
            Inventory.UpdateIncoming(new List<RessourceType> { inputRessource });
        }

        public void ChangeOutputRecipe(RessourceType outputRessource)
        {
            Progress = 0;
            TradeOutput = outputRessource;

            Inventory.UpdateRessourceLimits(CurrentLimit);
            Inventory.UpdateOutgoing(new List<RessourceType> { outputRessource });
        }
    }
}
