using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;

namespace Shared.Structures
{
    public class Inventory
    {
        protected Dictionary<RessourceType, int> Storage;
        public Dictionary<RessourceType, int> getStorage { get { return Storage; } }

        public Inventory()
        {
            Storage = new Dictionary<RessourceType, int>();
        }

        public Inventory(Dictionary<RessourceType, int> Storage)
        {
            this.Storage = Storage;
        }

        /// <summary>Read-only access to the inventories contents.</summary>
        public IReadOnlyDictionary<RessourceType, int> GetContents()
        {
            return Storage;
        }

        public int GetRessourceAmount(RessourceType ressourceType)
        {
            return Storage.ContainsKey(ressourceType) ? Storage[ressourceType] : 0;
        }

        public virtual int AddRessource(RessourceType ressourceType, int count)
        {
            if (Storage.ContainsKey(ressourceType))
            {
                Storage[ressourceType] += count;
                return count;
            }
            else
            {
                return 0;
            }
        }

        public int AddRessource(Dictionary<RessourceType, int> recipe)
        {
            int added = 0;
            foreach(KeyValuePair<RessourceType, int> ressource in recipe)
            {
                added += AddRessource(ressource.Key, ressource.Value);
            }
            return added;
        }

        public int RemoveRessource(RessourceType ressourceType, int amount)
        {
            int remaining = GetRessourceAmount(ressourceType);
            if (remaining >= 0)
            {
                int removed = Math.Min(amount, remaining);
                Storage[ressourceType] -= removed;
                return removed;
            }
            else
            {
                return 0;
            }
        }

        public void RemoveRessource(RessourceType ressourceType)
        {
            this.Storage.Remove(ressourceType);
        }

        /// <summary>
        /// Moves all resources into the target inventory.
        /// Returns true if all resources could be transferred.
        /// Not transferred resources remain in this inventory.
        /// </summary>
        public virtual bool MoveAllInto(Inventory receiver)
        {
            foreach (RessourceType resource in Storage.Keys.ToList())
            {
                int added = receiver.AddRessource(resource, this.Storage[resource]);
                this.RemoveRessource(resource, added);
            }
            return IsEmpty();
        }

        public bool RecipeApplicable(Dictionary<RessourceType, int> recipe)
        {
            foreach(RessourceType ressourceType in recipe.Keys)
            {
                if (this.GetRessourceAmount(ressourceType) < recipe[ressourceType])
                    return false;
            }
            return true;
        }

        public void ApplyRecipe(Dictionary<RessourceType, int> recipe)
        {
            if (!RecipeApplicable(recipe))
                return;
            foreach (RessourceType ressourceType in recipe.Keys)
            {
                this.RemoveRessource(ressourceType, recipe[ressourceType]);
            }
        }

        public static Dictionary<RessourceType, int> GetDictionaryOfAllRessources()
        {
            Dictionary<RessourceType, int> dict = new Dictionary<RessourceType, int>();
            foreach (RessourceType ressourceType in Enum.GetValues(typeof(RessourceType)))
            {
                dict.Add(ressourceType, 0);
            }
            return dict;
        }

        public static List<RessourceType> GetListOfAllRessources()
        {
            List<RessourceType> list = new List<RessourceType>();
            foreach (RessourceType ressourceType in Enum.GetValues(typeof(RessourceType)))
            {
                list.Add(ressourceType);
            }
            return list;
        }

        public void Clear()
        {
            foreach (RessourceType ressourceType in Storage.Keys.ToList())
            {
                Storage[ressourceType] = 0;
            }
        }

        public bool IsEmpty()
        {
            foreach(int amount in this.Storage.Values)
            {
                if (amount > 0)
                    return false;
            }
            return true;
        }

        public int TotalCount()
        {
            return Storage.Values.Sum();
        }

        public RessourceType GetMainRessource()
        {
            RessourceType mainRessource = RessourceType.WOOD;
            int mainRessourceAmount = 0;

            foreach (RessourceType ressourceType in this.Storage.Keys)
            {
                if(Storage[ressourceType] > mainRessourceAmount)
                {
                    mainRessourceAmount = Storage[ressourceType];
                    mainRessource = ressourceType;
                }
            }

            return mainRessource;
        }
    }
}
