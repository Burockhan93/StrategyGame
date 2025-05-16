using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataTypes;

namespace Shared.Structures
{
    public class BuildingInventory : Inventory
    {
        public Dictionary<RessourceType, int> Limits;

        public List<RessourceType> Outgoing;

        public List<RessourceType> Incoming;

        public BuildingInventory(Dictionary<RessourceType, int> limits) : base()
        {
            Limits = limits;
            foreach (RessourceType ressource in limits.Keys)
            {
                Storage.Add(ressource, 0);
            }
            Outgoing = new List<RessourceType>();
            Incoming = new List<RessourceType>();
        }

        public BuildingInventory(
            Dictionary<RessourceType, int> limits,
            Dictionary<RessourceType, int> storage,
            List<RessourceType> outgoing,
            List<RessourceType> incoming
        ) : base(storage)
        {
            Limits = limits;
            Outgoing = outgoing;
            Incoming = incoming;
        }

        public override int AddRessource(RessourceType ressourceType, int count)
        {
            int space = AvailableSpace(ressourceType);
            if (space > 0)
            {
                int added = Math.Min(count, space);
                Storage[ressourceType] += added;
                return added;
            }
            else
            {
                return 0;
            }
        }

        public int GetLimit(RessourceType ressource)
        {
            return Limits.ContainsKey(ressource) ? Limits[ressource] : 0;
        }

        public int AvailableSpace(RessourceType ressource)
        {
            return Limits.ContainsKey(ressource) ? GetLimit(ressource) - Storage[ressource] : 0;
        }

        public bool HasAvailableSpace(Dictionary<RessourceType, int> recipe)
        {
            foreach(KeyValuePair<RessourceType, int> kvp in recipe)
            {
                if (AvailableSpace(kvp.Key) > 0)
                    return true;
            }
            return false;
        }

        public void UpdateRessourceLimits(Dictionary<RessourceType, int> newLimits)
        {
            // remove illegal ressources from storage
            foreach (RessourceType key in Storage.Keys.ToList())
            {
                if (!newLimits.ContainsKey(key))
                    Storage.Remove(key);
            }

            // add new limits & storage entries
            Limits.Clear();
            foreach (KeyValuePair<RessourceType, int> el in newLimits)
            {
                RessourceType ressource = el.Key;
                int limit = el.Value;

                Limits.Add(ressource, limit);

                if (!Storage.ContainsKey(ressource))
                    Storage.Add(ressource, 0);
                else
                    Storage[ressource] = Math.Min(Storage[ressource], limit);
            }
        }

        public void UpdateRessourceLimit(RessourceType ressource, int limit)
        {
            if (Limits.ContainsKey(ressource))
            {
                Limits[ressource] = limit;
                Storage[ressource] = Math.Min(Storage[ressource], limit);
            }
        }

        public void UpdateOutgoing(List<RessourceType> newOutgoing)
        {
            this.Outgoing.Clear();
            foreach(RessourceType ressourceType in newOutgoing)
            {
                if (Limits.ContainsKey(ressourceType))
                {
                    this.Outgoing.Add(ressourceType);
                }
            }
        }

        public void UpdateIncoming(List<RessourceType> newIncoming)
        {
            this.Incoming.Clear();
            foreach(RessourceType ressourceType in newIncoming)
            {
                if (Limits.ContainsKey(ressourceType))
                {
                    this.Incoming.Add(ressourceType);
                }
            }
        }

        /// <summary>Generates an inventory limit for all ressources with the same amount.</summary>
        public static Dictionary<RessourceType, int[]> AllRessourceLimit(int[] amount)
        {
            Dictionary<RessourceType, int[]> limit = new Dictionary<RessourceType, int[]>();
            foreach (RessourceType ressourceType in Enum.GetValues(typeof(RessourceType)))
            {
                limit.Add(ressourceType, amount);
            }
            return limit;
        }
    }
}
