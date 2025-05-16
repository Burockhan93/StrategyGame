using System.Linq;
using System.Collections.Generic;
using Shared.DataTypes;
using Shared.HexGrid;
using Shared.Game;

namespace Shared.Structures
{
    /// <summary>Building producing troops.</summary>
    public abstract class TroopProductionBuilding: ProgressBuilding
    {
		/// <summary>Ressources needed to produce one troop.</summary>
        public abstract Dictionary<RessourceType, int> InputRecipe { get; }

		/// <summary>Troop currently produced by the building.</summary>
        public abstract TroopType OutputTroop { get; }

        /// <summary>Inventory for produced troops.</summary>
        public TroopInventory TroopInventory;

        public TroopProductionBuilding() : base()
        {
            Inventory.UpdateIncoming(InputRecipe.Keys.ToList());
            TroopInventory = new TroopInventory();
            TroopInventory.TroopLimit = 60;
        }

        public TroopProductionBuilding(
            HexCell cell,
            byte tribe,
            byte level,
            BuildingInventory inventory,
            int progress,
            TroopInventory troops
		) : base(cell, tribe, level, inventory, progress)
        {
            TroopInventory = troops;
        }

        public override void DoTick()
        {
            base.DoTick();
            if (Progress == 0 && Inventory.RecipeApplicable(InputRecipe) && TroopInventory.GetAvailableSpace() > 0)
            {
                Progress = 1;
                Inventory.ApplyRecipe(InputRecipe);
            }
        }

        public override void OnMaxProgress()
        {   
            List<Building> aps = GameLogic.getAssemblyPointsInRangeWithoutCooldown(GameLogic.GetTribe(this.Tribe), this);
            int len = GameLogic.GetTribe(this.Tribe).CurrentBuildings[typeof(AssemblyPoint)].Count();
            for(int i = 1; i <= len; i++)
            {
                foreach(Building apt in aps){
                    AssemblyPoint ap = (AssemblyPoint)apt;
                    if(ap.priority == i)
                    {
                        if(ap.TroopInventory.GetAvailableSpace()>0)
                        {
                            ap.TroopInventory.AddUnit(OutputTroop,1);
                            return;
                        }
                    }
                    
                }
            }
           
            TroopInventory.AddUnit(OutputTroop, 1);
        }
    }
}
