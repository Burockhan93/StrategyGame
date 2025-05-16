using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    public abstract class RefineryBuilding : ProgressBuilding
    {
        /// <summary>The current input ressources consumed by the building.</summary>
        public abstract Dictionary<RessourceType, int> InputRecipe { get; }

        /// <summary>The current output ressources produced by the building.</summary>
        public abstract Dictionary<RessourceType, int> OutputRecipe { get; }

        public RefineryBuilding() : base()
        {
            Inventory.UpdateIncoming(InputRecipe.Keys.ToList());
            Inventory.UpdateOutgoing(OutputRecipe.Keys.ToList());
        }

        public RefineryBuilding(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            int progress
        ) : base(cell, tribe, level, inventory, progress) {}

        public override void OnMaxProgress()
        {
            Inventory.AddRessource(OutputRecipe);
        }

        public override void DoTick()
        {
            base.DoTick();
            if (Progress == 0 && Inventory.RecipeApplicable(InputRecipe) && Inventory.HasAvailableSpace(OutputRecipe))
            {
                Progress = 1;
                Inventory.ApplyRecipe(InputRecipe);
            }
        }
    }
}
