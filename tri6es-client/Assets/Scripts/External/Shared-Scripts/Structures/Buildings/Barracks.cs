using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Communication;

namespace Shared.Structures
{
    public class Barracks: TroopProductionBuilding
    {
        public override string description => "The Barracks are used to train any kind of infantry Troop. Troops are needed to attack other Tribes or to defend your own Buildings from being attacked. Different Troops need different Ressources.";

        public override byte MaxLevel => 3;

        public override Dictionary<RessourceType, int[]> InventoryLimits => new Dictionary<RessourceType, int[]> {
            { RessourceType.BOW, new [] { 12, 12, 24 } },
            { RessourceType.SWORD, new [] { 12, 12, 24 } },
            { RessourceType.SPEAR, new [] { 12, 12, 24 } },
            { RessourceType.LEATHER_ARMOR, new [] { 12, 12, 24 } },
            { RessourceType.IRON_ARMOR, new [] { 12, 12, 24 } },
        };

        public override int[] MaxProgresses => new int[]
        {
            Constants.MinutesToGameTicks(1),
            Constants.MinutesToGameTicks(300),
            Constants.MinutesToGameTicks(120)
        };

        public override Dictionary<RessourceType, int>[] Recipes => new []
        {
            new Dictionary<RessourceType, int>{ { RessourceType.WOOD, 4 }, { RessourceType.STONE, 8 }, { RessourceType.IRON, 3 } },
            new Dictionary<RessourceType, int>{ { RessourceType.STONE, 10 }, { RessourceType.IRON, 5 }, { RessourceType.LEATHER, 5 } },
            new Dictionary<RessourceType, int>{ { RessourceType.STONE, 15 }, { RessourceType.IRON, 10 }, { RessourceType.LEATHER, 5 } }
        };

        public override Dictionary<RessourceType, int> InputRecipe
        {
            get
            {
                switch (CurrentOutput)
                {
                    case TroopType.ARCHER:
                    {
                        return new Dictionary<RessourceType, int> { { RessourceType.BOW, 1 }, { RessourceType.LEATHER_ARMOR, 1 } };
                    }
                    case TroopType.KNIGHT:
                    {
                        return new Dictionary<RessourceType, int> { { RessourceType.SWORD, 1 }, { RessourceType.IRON_ARMOR, 1 } };
                    }
                    case TroopType.SPEARMAN:
                    {
                        return new Dictionary<RessourceType, int> { { RessourceType.SPEAR, 1 }, { RessourceType.IRON_ARMOR, 1 } };
                    }
                    case TroopType.SCOUT:
                    {
                        return new Dictionary<RessourceType, int> { { RessourceType.LEATHER_ARMOR, 1 } };
                    }
                    default:
                    {
                        return new Dictionary<RessourceType, int> { };
                    }
                }
            }
        }

        public override TroopType OutputTroop => CurrentOutput;

        /// <summary>Current troop type produced by this barracks building.</summary>
        private TroopType CurrentOutput = TroopType.ARCHER;

        public Barracks() : base()
        {
            TroopInventory.TroopLimit = 60;
        }

        public Barracks(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            int progress,
            TroopInventory troops,
            TroopType output
        ) : base(cell, tribe, level, inventory, progress, troops)
        {
            CurrentOutput = output;
        }

        public TroopType[] GetAvailableTroopRecipes()
        {
            return new [] { TroopType.ARCHER, TroopType.KNIGHT, TroopType.SPEARMAN, TroopType.SCOUT };
        }

        public void ChangeTroopRecipe(TroopType troopType)
        {
            CurrentOutput = troopType;
            Inventory.UpdateIncoming(InputRecipe.Keys.ToList());
        }
    }
}
