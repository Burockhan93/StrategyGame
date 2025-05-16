using System;
using System.Linq;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;

namespace Shared.Structures
{
    using RessourceMap = Dictionary<RessourceType, int>;

    public abstract class MultiRefineryBuilding : RefineryBuilding
    {
        /// <summary>Mapping of all output ressource types to their input recipes.</summary>
        protected abstract Dictionary<RessourceType, RessourceMap> AvailableRecipes { get; }

        public override RessourceMap InputRecipe => AvailableRecipes.ContainsKey(CurrentOutput) ? AvailableRecipes[CurrentOutput] : new RessourceMap();

        public override RessourceMap OutputRecipe => new RessourceMap { { CurrentOutput, 1 } };

        /// <summary>The current output ressource type.</summary>
        public RessourceType CurrentOutput;

        public MultiRefineryBuilding() : base()
        {
           ChangeRecipe(AvailableRecipes.Keys.First());
        }

        public MultiRefineryBuilding(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            int progress,
            RessourceType output
        ) : base(cell, tribe, level, inventory, progress)
        {
            CurrentOutput = output;
        }

        /// <summary>Returns a lot of all possible outputs for this building.</summary>
        public List<RessourceType> GetRecipeOutputs()
        {
            return AvailableRecipes.Keys.ToList();
        }

        /// <summary>Changes the current recipe of the building to produce the given <paramref name="output"/> ressource.</summary>
        public void ChangeRecipe(RessourceType output)
        {
            // sanity check
            if (!AvailableRecipes.ContainsKey(output))
            {
                throw new Exception($"Invalid output ressource {output} for multi refinery building");
            }

            // reset progress
            Progress = 0;

            // update output
            CurrentOutput = output;

            // update inventory
            RefreshInventory();
        }

        private void RefreshInventory()
        {
            // update incoming/outgoing
            Inventory.UpdateIncoming(InputRecipe.Keys.ToList());
            Inventory.UpdateOutgoing(new List<RessourceType> { CurrentOutput });
        }
    }
}
