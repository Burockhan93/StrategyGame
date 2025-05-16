using System.Collections.Generic;
using Shared.DataTypes;
using Shared.Game;

namespace Shared.Structures
{
    public class TribeInventory
    {
        public Dictionary<RessourceType, int> Ressources;

        public BuildingInventory HQinventory;

        public List<BuildingInventory> storageInventories;

        public TribeInventory(Headquarter hq)
        {
            HQinventory = hq.Inventory;
            storageInventories = new List<BuildingInventory>();
            storageInventories.Add(hq.Inventory);
        }

        public void AddStorageInventory(BuildingInventory storageInventory)
        {
            storageInventories.Add(storageInventory);
        }

        public void RemoveStorageInventory(BuildingInventory storageInventory)
        {
            storageInventories.Remove(storageInventory);
        }

        public int GetRessourceAmount(RessourceType ressourceType)
        {
            int result = 0;
            foreach (BuildingInventory si in storageInventories)
            {
                result += si.GetRessourceAmount(ressourceType);
            }
            return result;
        }

        public int GetLimit(RessourceType type)
        {
            int result = 0;
            foreach (BuildingInventory si in storageInventories)
            {
                result += si.GetLimit(type);

            }
            return result;
        }

        public bool RecipeApplicable(Dictionary<RessourceType, int> recipe)
        {
            foreach (RessourceType ressourceType in recipe.Keys)
            {
                if (this.GetRessourceAmount(ressourceType) < recipe[ressourceType])
                    return false;
            }
            return true;
        }

        public bool RecipeApplicable(Dictionary<RessourceType, int> recipe, ref Feedback feedback)
        {
            feedback.message = "";
            bool applicable = true;
            foreach (RessourceType ressourceType in recipe.Keys)
            {
                int amount = this.GetRessourceAmount(ressourceType) - recipe[ressourceType];
                if (amount < 0)
                {
                    feedback.message += $"You need {-1 * amount} {ressourceType}\n";
                    applicable = false;
                }

            }
            if (applicable)
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        public void ApplyRecipe(Dictionary<RessourceType, int> recipe)
        {
            if (!RecipeApplicable(recipe))
                return;
            Dictionary<RessourceType, int> applyRecipe = recipe;
            foreach (RessourceType ressourceType in recipe.Keys)
            {
                RemoveRessource(ressourceType, recipe[ressourceType]);
            }
        }

        public void AddRessource(RessourceType ressourceType, int amount)
        {
            foreach (BuildingInventory inv in storageInventories)
            {
                int added = inv.AddRessource(ressourceType, amount);
                amount -= added;
                if (amount <= 0)
                    break;
            }
        }

        public void RemoveRessource(RessourceType ressourceType, int amount)
        {
            foreach (BuildingInventory inv in storageInventories)
            {
                int removed = inv.RemoveRessource(ressourceType, amount);
                amount -= removed;
                if (amount <= 0)
                    break;
            }
        }
    }
}